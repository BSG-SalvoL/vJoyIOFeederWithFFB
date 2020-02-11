﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using vJoyIOFeeder.Utils;

namespace vJoyIOFeeder.Outputs
{
    /// <summary>
    /// M2 emulator output agent.
    /// Use process memory read/write.
    /// Code converted from M2DUMP, M13DUMP, DaytonaUSB
    /// </summary>
    public class M2OutputAgent : AOutput
    {
        ulong DriveAddr;
        ulong LampAddr;
        ulong ProfileAddr;

        ProcessManipulation M2EmulatorProcess;

        public override void Start()
        {
            ScanForM2Emulator();
        }
        public override void Stop()
        {
            
        }


        public bool ScanForM2Emulator()
        {
            if (M2EmulatorProcess != null)
                M2EmulatorProcess.CloseProcess();
            M2EmulatorProcess = new ProcessManipulation();
            var procs = Process.GetProcessesByName("emulator.exe");
            if (procs.Length==0) {
                procs = Process.GetProcessesByName("emulator_multicpu.exe");
                if (procs.Length==0) {
                    return false;
                }
            }

            M2EmulatorProcess.OpenProcess(ProcessManipulation.ProcessAccess.PROCESS_WM_READ, procs[0]);
            M2EmulatorProcess.ReadUInt32(0x005AA888, out uint val);
            

            /*
            // Daytona USA (Saturn Ads)
            DWORD address = 0x0057285B; //v1.1 M2Emu TXaddressVR =0x005AA888;
            DWORD addressVR =0x005AA888;

            */
            return true;
        }



        public byte GetDriveData()
        {
            M2EmulatorProcess.ReadByte(DriveAddr, out byte value);
            this.DriveValue = value;
            return value;
        }

        public byte GetLampsData()
        {
            M2EmulatorProcess.ReadByte(LampAddr, out byte value);
            this.LampsValue = value;
            return value;
        }


    }
}
