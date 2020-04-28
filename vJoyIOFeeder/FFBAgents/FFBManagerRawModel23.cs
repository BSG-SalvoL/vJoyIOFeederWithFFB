﻿//#define CONSOLE_DUMP

using System;
using System.Globalization;
using System.Threading;

// Don't forget to add this
using vJoyIOFeeder.Utils;

namespace vJoyIOFeeder.FFBAgents
{
    /// <summary>
    /// See :
    /// http://superusr.free.fr/model3.htm
    /// </summary>
    public class FFBManagerRawModel23 :
        AFFBManager
    {


        public FFBManagerRawModel23(int refreshPeriod_ms) :
            base(refreshPeriod_ms)
        {
        }


        protected override void ComputeTrqFromAllEffects()
        {
            // Inputs:
            // - R=angular reference
            // - P=angular position
            // - W=angular velocity (W=dot(P)=dP/dt)
            // - A=angular accel (A=dot(W)=dW/dt=d2P/dt2)
            // output:
            // OutputEffectCommand

            // Take a snapshot of all values - convert time base to period
            EnterBarrier();
            double R = RefPosition_u;
            double P = FiltPosition_u_0;
            double W = FiltSpeed_u_per_s_0;
            double A = FiltAccel_u_per_s2_0;
            // Release the lock
            ExitBarrier();


            TranslateCommand(Program.Manager.RawDriveOutput);

            this.CheckForEffectsDone();
        }


        protected virtual void TranslateCommand(
            uint inputCmd)
        {

            OutputEffectCommand = inputCmd;
        }

        protected override void State_INIT()
        {
            switch (Step) {
                case 0:
                    ResetAllEffects();
                    // Echo test
                    OutputEffectCommand = (long)FFBManagerModel3.GenericModel3CMD.PING;
                    TimeoutTimer.Restart();
                    GoToNextStep();
                    break;
                case 1:
                    if (TimeoutTimer.ElapsedMilliseconds>1000) {
                        // Play sequence ?
                        OutputEffectCommand = (long)FFBManagerModel3.GenericModel3CMD.NO_EFFECT;
                        TimeoutTimer.Restart();
                        GoToNextStep();
                    }
                    break;
                case 7:
                    if (TimeoutTimer.ElapsedMilliseconds>100) {
                        // Maximum power set to 100%
                        OutputEffectCommand = (long)75;
                        GoToNextStep();
                    }
                    break;
                case 8:
                    TransitionTo(FFBStates.DEVICE_READY);
                    break;
            }
        }
       
        protected override void State_DISABLE()
        {
            switch (Step) {
                case 0:
                    OutputEffectCommand = (long)FFBManagerModel3.GenericModel3CMD.NO_EFFECT;
                    break;
            }
        }
        protected override void State_READY()
        {
            switch (Step) {
                case 0:
                    OutputTorqueLevel = 0.0;
                    OutputEffectCommand = (long)FFBManagerModel3.GenericModel3CMD.NO_EFFECT;
                    break;
            }
        }
    }
}


