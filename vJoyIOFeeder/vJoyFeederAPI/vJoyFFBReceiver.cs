﻿#define CONSOLE_DUMP
//#define DUMP_PACKET

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// Don't forget to add this
using vJoyInterfaceWrap;
using vJoyIOFeeder.FFBAgents;
using vJoyIOFeeder.Utils;

namespace vJoyIOFeeder.vJoyIOFeederAPI
{
    public class vJoyFFBReceiver
    {
        vJoy.FFB_DEVICE_PID PIDBlock = new vJoy.FFB_DEVICE_PID();
        protected AFFBManager FFBManager;
        protected vJoy Joystick;
        protected uint Id;
        protected vJoy.FfbCbFunc wrapper;
        protected bool isRegistered = false;

        /// <summary>
        /// Scale FFB units, between -10000/+10000 to unit values
        /// between -1/+1.
        /// This is to help performs computations on a normalized
        /// scaled value, independant of the wheel ratio or
        /// digital values.
        /// </summary>
        protected double Scale_FFB_to_u = (1.0/10000.0);




        protected void Log(string text, LogLevels level = LogLevels.DEBUG)
        {
            Logger.Log("[FFBRECV] " + text, level);
        }

        protected void LogFormat(LogLevels level, string text, params object[] args)
        {
            Logger.LogFormat(level, "[FFBRECV] " + text, args);
        }

        public vJoyFFBReceiver()
        {
            newEffectID = 1;
            PIDBlock.PIDBlockLoad.EffectBlockIndex = newEffectID;
            PIDBlock.PIDBlockLoad.LoadStatus = 0;
            PIDBlock.PIDBlockLoad.RAMPoolAvailable = 0xFFFF;
            PIDBlock.PIDPool.MaxSimultaneousEffects = 5;
            PIDBlock.EffectStates = new vJoy.FFB_PID_EFFECT_STATE_REPORT[vJoy.VJOY_FFB_MAX_EFFECTS_BLOCK_INDEX];
            for (int i = 0; i<PIDBlock.EffectStates.Length; i++) {
                PIDBlock.EffectStates[i].State = 0;
            }
        }

        /// <summary>
        /// Registers the base callback if not yet registered.
        /// </summary>
        public void RegisterBaseCallback(vJoy joystick, uint id, AFFBManager ffb)
        {
            FFBManager = ffb;
            Joystick = joystick;
            Id = id;
            // Read PID block
            Joystick.Ffb_h_ReadPID(Id, ref PIDBlock);

            if (!isRegistered) {
                wrapper = FfbFunction1; //needed to keep a reference!
                Joystick.FfbRegisterGenCB(wrapper, IntPtr.Zero);
                isRegistered = true;
            }
        }

        public void FfbFunction(IntPtr data)
        {
            unsafe {
                InternalFfbPacket* FfbData = (InternalFfbPacket*)data;
                int size = FfbData->DataSize;
                int command = (int)FfbData->Command;
                byte* bytes = (byte*)FfbData->PtrToData;
                Console.Write("FFB Size {0}", size);
                Console.Write(" Cmd:" + String.Format("{0:X08}", (int)FfbData->Command));
                Console.Write(" ID:" + String.Format("{0:X02}", command));
                Console.Write(" Size:" + String.Format("{0:D02}", (int)(size - 8)));
                Console.Write(" -");
                for (uint i = 0; i < size - 8; i++)
                    Console.Write(String.Format(" {0:X02}", (uint)(bytes[i])));
                Console.WriteLine();
            }
        }


        private enum CommandType : int
        {
            IOCTL_HID_SET_FEATURE = 0xB0191,
            IOCTL_HID_WRITE_REPORT = 0xB000F
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct InternalFfbPacket
        {
            public int DataSize;
            public CommandType Command;
            public IntPtr PtrToData;
        }

        protected enum ERROR : uint
        {
            ERROR_SUCCESS = 0,
        }

        protected Byte newEffectID = 1;

        /// <summary>
        /// Called when vJoy has a new FFB packet.
        /// WARNING This is called from a thread pool managed by windows.
        /// The thread itself is created and managed by vJoyInterface.dll.
        /// Do not overload it, else you will me missing FFB packets from
        /// third party application.
        /// </summary>
        /// <param name="ffbDataPtr"></param>
        /// <param name="userData"></param>
        public void FfbFunction1(IntPtr data, object userdata)
        {
            // Packet Header
            //copy ffb packet to managed structure
            InternalFfbPacket packet = (InternalFfbPacket)Marshal.PtrToStructure(data, typeof(InternalFfbPacket));

            /////// Packet Device ID, and Type Block Index (if exists)
            #region Packet Device ID, and Type Block Index
            int DeviceID = 0, BlockIndex = 0;
            FFBPType Type = new FFBPType();

            
            if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                Console.WriteLine("============= FFB Packet size Size {0} =============", (int)(packet.DataSize));
            }

            if ((uint)ERROR.ERROR_SUCCESS == Joystick.Ffb_h_DeviceID(data, ref DeviceID)) {
                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    Console.WriteLine(" > Device ID: {0}", DeviceID);
                }
            }

            // Effect block index only used when simultaneous effects should be done by
            // underlying hardware, which is not the case for a single motor driving wheel
            if ((uint)ERROR.ERROR_SUCCESS == Joystick.Ffb_h_EBI(data, ref BlockIndex)) {
                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    Console.WriteLine(" > Effect Block Index: {0}", BlockIndex);
                }
            }

            if ((uint)ERROR.ERROR_SUCCESS == Joystick.Ffb_h_Type(data, ref Type)) {
                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    if (!PacketType2Str(Type, out var TypeStr))
                        Console.WriteLine(" > Packet Type: {0}", Type);
                    else
                        Console.WriteLine(" > Packet Type: {0}", TypeStr);
                }
                switch (Type) {
                    case FFBPType.PT_POOLREP:
                        if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                            Console.WriteLine("POOL REPORT !!!");
                        }
                        break;
                    case FFBPType.PT_BLKLDREP:
                        Console.WriteLine("BLOCK LOAD REPORT !!!");
                        break;
                    case FFBPType.PT_BLKFRREP:
                        FfbFunction(data);
                        Console.WriteLine("BLOCK FREE REPORT !!!");
                        FFBManager.FreeEffect(BlockIndex);
                        //PIDBlock.PIDBlockLoad.EffectBlockIndex = (byte)0;
                        //PIDBlock.PIDBlockLoad.LoadStatus = 0;
                        Joystick.Ffb_h_ReadPID(Id, ref PIDBlock);
                        break;
                }
            }

            #endregion

            #region Create new effect Type
            FFBEType EffectType = new FFBEType();
            if ((uint)ERROR.ERROR_SUCCESS == Joystick.Ffb_h_EffNew(data, ref EffectType)) {

                FfbFunction(data);

                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    if (EffectType2Str(EffectType, out var TypeStr))
                        Console.WriteLine(" >> Effect Type: {0}", TypeStr);
                    else
                        Console.WriteLine(" >> Effect Type: Unknown");
                }
                var eid = FFBManager.CreateNewEffect();
                Joystick.Ffb_h_ReadPID(Id, ref PIDBlock);
                if (eid != PIDBlock.PIDBlockLoad.EffectBlockIndex)
                {
                    Console.WriteLine("!!! eid=" + eid + " != pid=" + ((int)PIDBlock.PIDBlockLoad.EffectBlockIndex));
                }
                PIDBlock.PIDBlockLoad.LoadStatus = 1;
                Joystick.Ffb_h_WritePID(Id, ref PIDBlock);
            }

            #endregion

            #region Condition
            vJoy.FFB_EFF_COND Condition = new vJoy.FFB_EFF_COND();
            if ((uint)ERROR.ERROR_SUCCESS == Joystick.Ffb_h_Eff_Cond(data, ref Condition)) {

                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    if (Condition.isY)
                        Console.WriteLine(" >> Y Axis");
                    else
                        Console.WriteLine(" >> X Axis");
                    Console.WriteLine(" >> Center Point Offset: {0}", TwosCompWord2Int(Condition.CenterPointOffset));
                    Console.WriteLine(" >> Positive Coefficient: {0}", TwosCompWord2Int(Condition.PosCoeff));
                    Console.WriteLine(" >> Negative Coefficient: {0}", TwosCompWord2Int(Condition.NegCoeff));
                    Console.WriteLine(" >> Positive Saturation: {0}", Condition.PosSatur);
                    Console.WriteLine(" >> Negative Saturation: {0}", Condition.NegSatur);
                    Console.WriteLine(" >> Dead Band: {0}", Condition.DeadBand);
                }
                // Skip all processing if Y axis (single axis for wheel FFB!)
                if (Condition.isY) {
                    // Leave early!
                    return;
                }

                FFBManager.SetLimitsParams(BlockIndex,
                    TwosCompWord2Int(Condition.CenterPointOffset) * Scale_FFB_to_u,
                    Condition.DeadBand * Scale_FFB_to_u,
                    TwosCompWord2Int(Condition.PosCoeff) * Scale_FFB_to_u,
                    TwosCompWord2Int(Condition.NegCoeff) * Scale_FFB_to_u,
                    Condition.PosSatur * Scale_FFB_to_u,
                    -Condition.NegSatur * Scale_FFB_to_u);

            }
            #endregion

            /////// Effect Report
            #region Effect Report
            vJoy.FFB_EFF_REPORT Effect = new vJoy.FFB_EFF_REPORT();
            if ((uint)ERROR.ERROR_SUCCESS == Joystick.Ffb_h_Eff_Report(data, ref Effect)) {
                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    if (!EffectType2Str(Effect.EffectType, out var TypeStr))
                        Console.WriteLine(" >> Effect Report: {0} {1}", (int)Effect.EffectType, Effect.EffectType.ToString());
                    else
                        Console.WriteLine(" >> Effect Report: {0}", TypeStr);
                }
                if (Effect.Polar) {
                    if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                        Console.WriteLine(" >> Direction: {0} deg ({1})", Polar2Deg(Effect.Direction), Effect.Direction);
                    }
                    FFBManager.SetDirection(BlockIndex, Polar2Deg(Effect.Direction));
                } else {
                    if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                        Console.WriteLine(" >> X Direction: {0}", Effect.DirX);
                        Console.WriteLine(" >> Y Direction: {0}", Effect.DirY);
                    }
                    FFBManager.SetDirection(BlockIndex, Effect.DirX);
                }

                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    if (Effect.Duration == 0xFFFF)
                        Console.WriteLine(" >> Duration: Infinit");
                    else
                        Console.WriteLine(" >> Duration: {0} MilliSec", (int)(Effect.Duration));

                    if (Effect.TrigerRpt == 0xFFFF)
                        Console.WriteLine(" >> Trigger Repeat: Infinit");
                    else
                        Console.WriteLine(" >> Trigger Repeat: {0}", (int)(Effect.TrigerRpt));

                    if (Effect.SamplePrd == 0xFFFF)
                        Console.WriteLine(" >> Sample Period: Infinit");
                    else
                        Console.WriteLine(" >> Sample Period: {0}", (int)(Effect.SamplePrd));

                    if (Effect.StartDelay == 0xFFFF)
                        Console.WriteLine(" >> Start Delay: max ");
                    else
                        Console.WriteLine(" >> Start Delay: {0}", (int)(Effect.StartDelay));


                    Console.WriteLine(" >> Gain: {0}%%", Byte2Percent(Effect.Gain));
                }

                if (Effect.Duration==65535)
                    FFBManager.SetDuration(BlockIndex, -1.0);
                else
                    FFBManager.SetDuration(BlockIndex, Effect.Duration);
                if (Effect.StartDelay == 65535)
                    FFBManager.SetStartDelay(BlockIndex, 0);
                else
                    FFBManager.SetStartDelay(BlockIndex, Effect.StartDelay);

                FFBManager.SetEffectGain(BlockIndex, Byte2Percent(Effect.Gain)*0.01);
                switch (Effect.EffectType) {
                    case FFBEType.ET_CONST:
                        FFBManager.SetEffect(BlockIndex, AFFBManager.EffectTypes.CONSTANT_TORQUE);
                        break;
                    case FFBEType.ET_RAMP:
                        FFBManager.SetEffect(BlockIndex, AFFBManager.EffectTypes.RAMP);
                        break;
                    case FFBEType.ET_INRT:
                        FFBManager.SetEffect(BlockIndex, AFFBManager.EffectTypes.INERTIA);
                        break;
                    case FFBEType.ET_SPRNG:
                        FFBManager.SetEffect(BlockIndex, AFFBManager.EffectTypes.SPRING);
                        break;
                    case FFBEType.ET_DMPR:
                        FFBManager.SetEffect(BlockIndex, AFFBManager.EffectTypes.DAMPER);
                        break;
                    case FFBEType.ET_FRCTN:
                        FFBManager.SetEffect(BlockIndex, AFFBManager.EffectTypes.FRICTION);
                        break;
                    // Periodic
                    case FFBEType.ET_SQR:
                        FFBManager.SetEffect(BlockIndex, AFFBManager.EffectTypes.SQUARE);
                        break;
                    case FFBEType.ET_SINE:
                        FFBManager.SetEffect(BlockIndex, AFFBManager.EffectTypes.SINE);
                        break;
                    case FFBEType.ET_TRNGL:
                        FFBManager.SetEffect(BlockIndex, AFFBManager.EffectTypes.TRIANGLE);
                        break;
                    case FFBEType.ET_STUP:
                        FFBManager.SetEffect(BlockIndex, AFFBManager.EffectTypes.SAWTOOTHUP);
                        break;
                    case FFBEType.ET_STDN:
                        FFBManager.SetEffect(BlockIndex, AFFBManager.EffectTypes.SAWTOOTHDOWN);
                        break;
                }
            }
            #endregion

            #region PID Device Control
            FFB_CTRL Control = new FFB_CTRL();
            if ((uint)ERROR.ERROR_SUCCESS == Joystick.Ffb_h_DevCtrl(data, ref Control) && DevCtrl2Str(Control, out var CtrlStr)) {

                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    Console.WriteLine(" >> PID Device Control: {0}", CtrlStr);
                }
                switch (Control) {
                    case FFB_CTRL.CTRL_DEVRST:
                        newEffectID = 1;
                        //PIDBlock.PIDBlockLoad.EffectBlockIndex = newEffectID;
                        Joystick.Ffb_h_ReadPID(Id, ref PIDBlock);
                        // device reset
                        FFBManager.DevReset();
                        break;
                    case FFB_CTRL.CTRL_ENACT:
                        FFBManager.DevEnable();
                        break;
                    case FFB_CTRL.CTRL_DISACT:
                        FFBManager.DevDisable();
                        break;
                    case FFB_CTRL.CTRL_STOPALL:
                        FFBManager.StopAllEffects();
                        break;
                }
            }

            #endregion

            #region Effect Operation
            vJoy.FFB_EFF_OP Operation = new vJoy.FFB_EFF_OP();
            if ((uint)ERROR.ERROR_SUCCESS == Joystick.Ffb_h_EffOp(data, ref Operation) && EffectOpStr(Operation.EffectOp, out var EffOpStr)) {

                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    Console.WriteLine(" >> Effect Operation: {0}", EffOpStr);
                    if (Operation.LoopCount == 0xFF)
                        Console.WriteLine(" >> Loop until stopped");
                    else
                        Console.WriteLine(" >> Loop {0} times", (int)(Operation.LoopCount));
                }

                switch (Operation.EffectOp) {
                    case FFBOP.EFF_START:
                        // Start the effect identified by the Effect Handle.
                        FFBManager.StartEffect(BlockIndex, (int)(Operation.LoopCount));
                        break;
                    case FFBOP.EFF_STOP:
                        // Stop the effect identified by the Effect Handle.
                        FFBManager.StopEffect(BlockIndex);
                        break;
                    case FFBOP.EFF_SOLO:
                        // Start the effect identified by the Effect Handle and stop all other effects.
                        FFBManager.StartEffect(BlockIndex, 1);
                        break;
                }

            }
            #endregion

            #region Global Device Gain
            byte Gain = 0;
            if ((uint)ERROR.ERROR_SUCCESS == Joystick.Ffb_h_DevGain(data, ref Gain)) {

                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    Console.WriteLine(" >> Global Device Gain: {0}", Byte2Percent(Gain));
                }
                FFBManager.SetDeviceGain(Byte2Percent(Gain)*0.01);
            }

            #endregion

            #region Envelope
            vJoy.FFB_EFF_ENVLP Envelope = new vJoy.FFB_EFF_ENVLP();
            if ((uint)ERROR.ERROR_SUCCESS == Joystick.Ffb_h_Eff_Envlp(data, ref Envelope)) {

                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    Console.WriteLine(" >> Attack Level: {0}", Envelope.AttackLevel);
                    Console.WriteLine(" >> Fade Level: {0}", Envelope.FadeLevel);
                    Console.WriteLine(" >> Attack Time: {0}", (int)(Envelope.AttackTime));
                    Console.WriteLine(" >> Fade Time: {0}", (int)(Envelope.FadeTime));
                }
                FFBManager.SetEnveloppeParams(BlockIndex, Envelope.AttackTime, Envelope.AttackLevel, Envelope.FadeTime, Envelope.FadeLevel);
            }

            #endregion

            #region Periodic
            vJoy.FFB_EFF_PERIOD EffPrd = new vJoy.FFB_EFF_PERIOD();
            if ((uint)ERROR.ERROR_SUCCESS == Joystick.Ffb_h_Eff_Period(data, ref EffPrd)) {

                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    Console.WriteLine(" >> Magnitude: {0}", EffPrd.Magnitude);
                    Console.WriteLine(" >> Offset: {0}", TwosCompWord2Int(EffPrd.Offset));
                    Console.WriteLine(" >> Phase: {0}", EffPrd.Phase * 3600 / 255);
                    Console.WriteLine(" >> Period: {0}", (int)(EffPrd.Period));
                }
                FFBManager.SetPeriodicParams(BlockIndex, (double)EffPrd.Magnitude* Scale_FFB_to_u, TwosCompWord2Int(EffPrd.Offset)* Scale_FFB_to_u, EffPrd.Phase * 0.01, EffPrd.Period);
            }
            #endregion


            #region Ramp Effect
            vJoy.FFB_EFF_RAMP RampEffect = new vJoy.FFB_EFF_RAMP();
            if ((uint)ERROR.ERROR_SUCCESS == Joystick.Ffb_h_Eff_Ramp(data, ref RampEffect)) {
                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    Console.WriteLine(" >> Ramp Start: {0}", TwosCompWord2Int(RampEffect.Start));
                    Console.WriteLine(" >> Ramp End: {0}", TwosCompWord2Int(RampEffect.End));
                }
                FFBManager.SetRampParams(BlockIndex, RampEffect.Start * Scale_FFB_to_u, RampEffect.End * Scale_FFB_to_u);
            }

            #endregion

            #region Constant Effect
            vJoy.FFB_EFF_CONSTANT CstEffect = new vJoy.FFB_EFF_CONSTANT();
            if ((uint)ERROR.ERROR_SUCCESS == Joystick.Ffb_h_Eff_Constant(data, ref CstEffect)) {
                if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                    Console.WriteLine(" >> Block Index: {0}", TwosCompWord2Int(CstEffect.EffectBlockIndex));
                    Console.WriteLine(" >> Magnitude: {0}", TwosCompWord2Int(CstEffect.Magnitude));
                }
                FFBManager.SetConstantTorqueEffect(BlockIndex, (double)CstEffect.Magnitude * Scale_FFB_to_u);
            }

            #endregion

#if DUMP_PACKET
            FfbFunction(data);
#endif
            if (vJoyManager.Config.VerbosevJoyFFBReceiverDumpFrames) {
                Console.WriteLine("====================================================");
            }
        }



        // Convert Packet type to String
        public static bool PacketType2Str(FFBPType Type, out string Str)
        {
            bool stat = true;
            Str = "";

            switch (Type) {
                case FFBPType.PT_EFFREP:
                    Str = "Effect Report";
                    break;
                case FFBPType.PT_ENVREP:
                    Str = "Envelope Report";
                    break;
                case FFBPType.PT_CONDREP:
                    Str = "Condition Report";
                    break;
                case FFBPType.PT_PRIDREP:
                    Str = "Periodic Report";
                    break;
                case FFBPType.PT_CONSTREP:
                    Str = "Constant Force Report";
                    break;
                case FFBPType.PT_RAMPREP:
                    Str = "Ramp Force Report";
                    break;
                case FFBPType.PT_CSTMREP:
                    Str = "Custom Force Data Report";
                    break;
                case FFBPType.PT_SMPLREP:
                    Str = "Download Force Sample";
                    break;
                case FFBPType.PT_EFOPREP:
                    Str = "Effect Operation Report";
                    break;
                case FFBPType.PT_BLKFRREP:
                    Str = "PID Block Free Report";
                    break;
                case FFBPType.PT_CTRLREP:
                    Str = "PID Device Contro";
                    break;
                case FFBPType.PT_GAINREP:
                    Str = "Device Gain Report";
                    break;
                case FFBPType.PT_SETCREP:
                    Str = "Set Custom Force Report";
                    break;
                case FFBPType.PT_NEWEFREP:
                    Str = "Create New Effect Report";
                    break;
                case FFBPType.PT_BLKLDREP:
                    Str = "Block Load Report";
                    break;
                case FFBPType.PT_POOLREP:
                    Str = "PID Pool Report";
                    break;
                default:
                    stat = false;
                    break;
            }

            return stat;
        }

        // Convert Effect type to String
        public static bool EffectType2Str(FFBEType Type, out string Str)
        {
            bool stat = true;
            Str = "";

            switch (Type) {
                case FFBEType.ET_NONE:
                    stat = false;
                    break;
                case FFBEType.ET_CONST:
                    Str = "Constant Force";
                    break;
                case FFBEType.ET_RAMP:
                    Str = "Ramp";
                    break;
                case FFBEType.ET_SQR:
                    Str = "Square";
                    break;
                case FFBEType.ET_SINE:
                    Str = "Sine";
                    break;
                case FFBEType.ET_TRNGL:
                    Str = "Triangle";
                    break;
                case FFBEType.ET_STUP:
                    Str = "Sawtooth Up";
                    break;
                case FFBEType.ET_STDN:
                    Str = "Sawtooth Down";
                    break;
                case FFBEType.ET_SPRNG:
                    Str = "Spring";
                    break;
                case FFBEType.ET_DMPR:
                    Str = "Damper";
                    break;
                case FFBEType.ET_INRT:
                    Str = "Inertia";
                    break;
                case FFBEType.ET_FRCTN:
                    Str = "Friction";
                    break;
                case FFBEType.ET_CSTM:
                    Str = "Custom Force";
                    break;
                default:
                    stat = false;
                    break;
            }

            return stat;
        }

        // Convert PID Device Control to String
        public static bool DevCtrl2Str(FFB_CTRL Ctrl, out string Str)
        {
            bool stat = true;
            Str = "";

            switch (Ctrl) {
                case FFB_CTRL.CTRL_ENACT:
                    Str = "Enable Actuators";
                    break;
                case FFB_CTRL.CTRL_DISACT:
                    Str = "Disable Actuators";
                    break;
                case FFB_CTRL.CTRL_STOPALL:
                    Str = "Stop All Effects";
                    break;
                case FFB_CTRL.CTRL_DEVRST:
                    Str = "Device Reset";
                    break;
                case FFB_CTRL.CTRL_DEVPAUSE:
                    Str = "Device Pause";
                    break;
                case FFB_CTRL.CTRL_DEVCONT:
                    Str = "Device Continue";
                    break;
                default:
                    stat = false;
                    break;
            }

            return stat;
        }

        // Convert Effect operation to string
        public static bool EffectOpStr(FFBOP Op, out string Str)
        {
            bool stat = true;
            Str = "";

            switch (Op) {
                case FFBOP.EFF_START:
                    Str = "Effect Start";
                    break;
                case FFBOP.EFF_SOLO:
                    Str = "Effect Solo Start";
                    break;
                case FFBOP.EFF_STOP:
                    Str = "Effect Stop";
                    break;
                default:
                    stat = false;
                    break;
            }

            return stat;
        }

        // Polar values (0x00-0xFF) to Degrees (0-360)
        public static int Polar2Deg(byte Polar)
        {
            return ((int)Polar * 360) / 255;
        }

        // Convert range 0x00-0xFF to 0%-100%
        public static int Byte2Percent(byte InByte)
        {
            return ((byte)InByte * 100) / 255;
        }

        // Convert One-Byte 2's complement input to integer
        public static int TwosCompByte2Int(byte inb)
        {
            int tmp;
            byte inv = (byte)~inb;
            bool isNeg = ((inb >> 7) != 0 ? true : false);
            if (isNeg) {
                tmp = (int)(inv);
                tmp = -1 * tmp;
                return tmp;
            } else
                return (int)inb;
        }

        // Convert One-Byte 2's complement input to integer
        public static int TwosCompWord2Int(short inb)
        {
            int tmp;
            int inv = (int)~inb + 1;
            bool isNeg = ((inb >> 15) != 0 ? true : false);
            if (isNeg) {
                tmp = (int)(inv);
                tmp = -1 * tmp;
                return tmp;
            } else
                return (int)inb;
        }

    }
}
