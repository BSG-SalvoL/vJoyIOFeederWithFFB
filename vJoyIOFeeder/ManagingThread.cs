﻿using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.Threading;
using vJoyIOFeeder.FFBAgents;
using vJoyIOFeeder.IOCommAgents;
using vJoyIOFeeder.Utils;
using vJoyIOFeeder.vJoyIOFeederAPI;

namespace vJoyIOFeeder
{
    public class ManagingThread
    {
        /// <summary>
        /// Will be moved to configuration
        /// </summary>
        public string COMPort = "COM7";

        /// <summary>
        /// vJoy abstraction layer
        /// </summary>
        public static vJoyFeeder vJoy;
        /// <summary>
        /// IO abstraction layer
        /// </summary>
        public static USBSerialIO IOboard;
        /// <summary>
        /// Force feedback management/computations layer
        /// </summary>
        public static FFBManager FFB;

        /// <summary>
        /// Global refresh period for whole application, includes
        /// serial port comm + FFB computation.
        /// This needs to be tuned!
        /// </summary>
        public const int GlobalRefreshPeriod_ms = 5;

        /// <summary>
        /// 1 = every tick/period
        /// 2 = every 2 ticks/periods
        /// n = every n
        /// </summary>
        public const int vJoyUpdate = 3;

        protected static bool Running = true;
        protected static Thread MainThread = null;
        protected static ulong TickCount = 0;

        protected void MainThreadMethod()
        {
            IOboard = new USBSerialIO(COMPort); // Speed not used for Serial-over-USB
            vJoy = new vJoyFeeder();
            FFB = new FFBManager(GlobalRefreshPeriod_ms);

            // Use this to allow 1ms sleep granularity (else default is 16ms!!!)
            // This consumes more CPU cycles in the OS, but does improve
            // a lot reactivity when soft real-time work needs to be done.
            MultimediaTimer.Set1msTickGranularityOnWindows();

            vJoy.EnableJoystick(); // Create joystick interface
            vJoy.Acquire(1); // Use first enumerated vJoy device
            vJoy.StartAndRegisterFFB(FFB); // Start FFB callback mechanism in vJoy

            // In case we want to use XInput/DInput devices to gather multiple inputs?
            //XInput();
            //DirectInput();

            Console.WriteLine("Start feeding...");
            // Open com port and perform handshaking
            IOboard.OpenComm();
            // Enable auto-streaming
            IOboard.StartStreaming();
            // Enable safety watchdog
            IOboard.EnableWD();
            // Start FFB manager
            FFB.Start();
            var prev_angle = 0.0;
            while (Running) {
                TickCount++;
                try {
                    if (IOboard.IsOpen) {
                        // Update status on received packets
                        IOboard.UpdateOnStreaming();

                        // Refresh wheel angle (between -1...1)
                        if (IOboard.AnalogInputs.Length > 0) {
                            // Scale analog input between 0..0xFFF, then map it to -1/+1, 0 being center
                            var angle_u = ((double)IOboard.AnalogInputs[0]) * (2.0 / (double)0xFFF) - 1.0;
                            // Refresh values in FFB manager
                            if (IOboard.WheelStates.Length > 0) {
                                // If full state given by IO board (should be in unit_per_s!)
                                FFB.RefreshCurrentState(angle_u, IOboard.WheelStates[0], IOboard.WheelStates[1]);
                            } else {
                                // If only periodic position
                                FFB.RefreshCurrentPosition(angle_u);
                            }
                            prev_angle = angle_u;
                        }

                        // For debugging purpose, add a 4th axis to display torque output
                        uint[] axes3plusTrq = new uint[4];
                        IOboard.AnalogInputs.CopyTo(axes3plusTrq, 0);
                        axes3plusTrq[3] = (uint)(FFB.OutputTorqueLevel * 0x800 + 0x800);
                        // Set values into vJoy report:
                        // - axes
                        vJoy.UpdateAxes12(axes3plusTrq);
                        // - buttons
                        if (IOboard.DigitalInputs8.Length > 0)
                            vJoy.UpodateFirst32Buttons(IOboard.DigitalInputs8[0]);

                        // - 360deg POV to view for wheel angle
                        vJoy.UpodateContinuousPOV((uint)((IOboard.AnalogInputs[0] / (double)0xFFF) * 35900.0) + 18000);

                        // Update vJoy and send to driver every 2 ticks to limit workload on driver
                        if ((TickCount % vJoyUpdate) ==0) {
                            vJoy.PublishiReport();
                        }

                        // Now output torque to Pwm+Dir
                        if (FFB.OutputTorqueLevel >= 0.0) {
                            uint analogOut = (uint)(FFB.OutputTorqueLevel * 0xFFF);
                            // Save into IOboard
                            IOboard.AnalogOutputs[0] = analogOut;
                            IOboard.DigitalOutputs8[0] = 0;
                        } else {
                            uint analogOut = (uint)(-FFB.OutputTorqueLevel * 0xFFF);
                            // Save into IOboard
                            IOboard.AnalogOutputs[0] = analogOut;
                            IOboard.DigitalOutputs8[0] = 1;
                        }
                        // Send all outputs - this will revive the watchdog!
                        IOboard.SendOutputs();

                    } else {
                        Console.WriteLine("Connecting IO board...");
                        IOboard.OpenComm();
                        IOboard.StartStreaming();
                    }
                } catch (Exception ex) {
                    Console.WriteLine("IO board Failing: " + ex.Message);
                    if (IOboard.IsOpen)
                        IOboard.CloseComm();
                }


                //Console.WriteLine("Main loop");

                // Sleep until next tick
                System.Threading.Thread.Sleep(GlobalRefreshPeriod_ms);
            };

            MultimediaTimer.RestoreTickGranularityOnWindows();

            FFB.Stop();
            IOboard.CloseComm();
            vJoy.Release();
        }

        public void StartManagingThread()
        {
            if (MainThread != null) {
                StopManagingThread();
            }
            MainThread = new Thread(MainThreadMethod);
            Running = true;
            MainThread.Start();
        }
        public void StopManagingThread()
        {
            Running = false;
            if (MainThread == null)
                return;
            Thread.Sleep(GlobalRefreshPeriod_ms * 10);
            MainThread.Join();
            MainThread = null;
        }

        public static void DirectInput()
        {
            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
                foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                    joystickGuid = deviceInstance.InstanceGuid;

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty) {
                Console.WriteLine("No joystick/Gamepad found.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            // Instantiate the joystick
            var joystick = new Joystick(directInput, joystickGuid);

            Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

            // Query all suported ForceFeedback effects
            var allEffects = joystick.GetEffects();
            foreach (var effectInfo in allEffects)
                Console.WriteLine("Effect available {0}", effectInfo.Name);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();

            // Poll events from joystick
            while (true) {
                joystick.Poll();
                var datas = joystick.GetBufferedData();
                foreach (var state in datas)
                    Console.WriteLine(state);
            }
        }

        public static void XInput()
        {
            Console.WriteLine("Start XGamepadApp");
            // Initialize XInput
            var controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };

            // Get 1st controller available
            Controller controller = null;
            foreach (var selectControler in controllers) {
                if (selectControler.IsConnected) {
                    controller = selectControler;
                    break;
                }
            }

            if (controller == null) {
                Console.WriteLine("No XInput controller installed");
            } else {

                Console.WriteLine("Found a XInput controller available");
                Console.WriteLine("Press buttons on the controller to display events or escape key to exit... ");

                // Poll events from joystick
                var previousState = controller.GetState();
                while (controller.IsConnected) {
                    if (IsKeyPressed(ConsoleKey.Escape)) {
                        break;
                    }
                    var state = controller.GetState();
                    if (previousState.PacketNumber != state.PacketNumber)
                        Console.WriteLine(state.Gamepad);
                    Thread.Sleep(8);
                    previousState = state;
                }
            }
            Console.WriteLine("End XGamepadApp");
        }

        /// <summary>
        /// Determines whether the specified key is pressed.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key is pressed; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsKeyPressed(ConsoleKey key)
        {
            return Console.KeyAvailable && Console.ReadKey(true).Key == key;
        }


    }
}
