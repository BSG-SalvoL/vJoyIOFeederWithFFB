﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using vJoyIOFeeder;
using vJoyIOFeeder.FFBAgents;
using vJoyIOFeeder.Utils;

namespace vJoyIOFeederGUI.GUI
{

    public partial class TargetHdwForm : Form
    {
        public TargetHdwForm()
        {
            InitializeComponent();
        }


        private void TargetHdwForm_Load(object sender, EventArgs e)
        {
            ToolTip tooltip = new ToolTip();

            tooltip.SetToolTip(this.cmbSelectMode, "Translation mode can only be changed while manager is Stopped");
            tooltip.SetToolTip(this.btnStartStopManager, "Translation mode can only be changed while manager is Stopped");
            this.cmbSelectMode.Items.Clear();
            foreach (string mode in Enum.GetNames(typeof(FFBTranslatingModes))) {
                this.cmbSelectMode.Items.Add(mode);

                if (vJoyManager.Config.Hardware.TranslatingModes.ToString().Equals(mode, StringComparison.OrdinalIgnoreCase)) {
                    this.cmbSelectMode.SelectedIndex = this.cmbSelectMode.Items.Count - 1;
                }
            }
            
            this.txtWheelScale.Text = vJoyManager.Config.Hardware.WheelScaleFactor_u_per_cts.ToString("G8", CultureInfo.InvariantCulture);
            this.txtWheelCenter.Text = vJoyManager.Config.Hardware.WheelCenterOffset_u.ToString("G8", CultureInfo.InvariantCulture);
        }

        private void TargetHdwForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.Manager.SaveConfigurationFiles(Program.AppCfgFilename, Program.HwdCfgFilename, Program.CtlSetsCfgFilename);
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            this.checkBoxStartMinimized.Checked = vJoyManager.Config.Application.StartMinimized;
            this.checkBoxStartWithWindows.Checked = vJoyManager.Config.Application.ShortcutStartWithWindowsCreated;


            if (Program.Manager.IsRunning) {
                this.btnStartStopManager.BackColor = Color.Green;
                this.btnStartStopManager.Text = "Running (Stop)";

                this.cmbSelectMode.Enabled = false;
            } else {
                this.btnStartStopManager.BackColor = Color.Red;
                this.btnStartStopManager.Text = "Stopped (Start)";

                this.cmbSelectMode.Enabled = true;
            }

            if (Program.Manager.FFB!=null) {

                if (Program.Manager.FFB.IsDeviceReady) {
                    btnDeviceReady.BackColor = Color.Green;
                    btnDeviceReady.Text = "Ready";
                } else {
                    btnDeviceReady.BackColor = Color.Red;
                    btnDeviceReady.Text = "Not ready";
                }

                chkInvertWheel.Checked = vJoyManager.Config.Hardware.InvertWheelDirection;
                chkInvertTorque.Checked = vJoyManager.Config.Hardware.InvertTrqDirection;
            }
        }


        private void btnOpenJoyCPL_Click(object sender, EventArgs e)
        {
            ProcessAnalyzer.StartProcess(@"joy.cpl");
        }

        private void btnOpenvJoyMonitor_Click(object sender, EventArgs e)
        {
            ProcessAnalyzer.StartProcess(@"C:\Program Files\vJoy\x64\JoyMonitor.exe");
        }

        private void btnOpenvJoyConfig_Click(object sender, EventArgs e)
        {
            ProcessAnalyzer.StartProcess(@"C:\Program Files\vJoy\x64\vJoyConf.exe");
        }


        private void btnStartStopManager_Click(object sender, EventArgs e)
        {
            if (!Program.Manager.IsRunning) {
                if (Enum.TryParse<FFBTranslatingModes>(this.cmbSelectMode.SelectedItem.ToString(), out var mode)) {
                    vJoyManager.Config.Hardware.TranslatingModes = mode;
                }
                Program.Manager.Start();
            } else {
                Program.Manager.Stop();
            }
        }

        #region Application configuration
        private void checkBoxStartMinimized_Click(object sender, EventArgs e)
        {
            vJoyManager.Config.Application.StartMinimized = !vJoyManager.Config.Application.StartMinimized;
        }

        private void checkBoxStartWithWindows_Click(object sender, EventArgs e)
        {
            vJoyManager.Config.Application.ShortcutStartWithWindowsCreated = !vJoyManager.Config.Application.ShortcutStartWithWindowsCreated;
            if (vJoyManager.Config.Application.ShortcutStartWithWindowsCreated) {
                // Create shortcut
                OSUtilities.CreateStartupShortcut("vJoyIOFeederGUI", "vJoyIOFeederGUI auto-startup");
            } else {
                OSUtilities.DeleteStartupShortcut("vJoyIOFeederGUI");
            }
        }

        #endregion

        #region Hardware properties

        private void chkInvertWheel_Click(object sender, EventArgs e)
        {
            vJoyManager.Config.Hardware.InvertWheelDirection = !vJoyManager.Config.Hardware.InvertWheelDirection;
        }
        private void chkInvertTorque_Click(object sender, EventArgs e)
        {
            vJoyManager.Config.Hardware.InvertTrqDirection = !vJoyManager.Config.Hardware.InvertTrqDirection;
        }
        private void cmbSelectMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Program.Manager.IsRunning) {
                if (Enum.TryParse<FFBTranslatingModes>(this.cmbSelectMode.SelectedItem.ToString(), out var mode)) {
                    vJoyManager.Config.Hardware.TranslatingModes = mode;
                    Program.Manager.SaveConfigurationFiles(Program.AppCfgFilename, Program.HwdCfgFilename, Program.CtlSetsCfgFilename);
                }
            }
        }

        private void btnWheelCalibrate_Click(object sender, EventArgs e)
        {
            CalibrateWheelForm calibwheel = new CalibrateWheelForm();
            calibwheel.SelectedAxis = 0;
            var res = calibwheel.ShowDialog(this);
            if (res == DialogResult.OK) {
                double range_cts = calibwheel.RawMostLeft - calibwheel.RawMostRight;
                double scale_u_per_cts = 2.0/range_cts;
                vJoyManager.Config.Hardware.WheelScaleFactor_u_per_cts = scale_u_per_cts;
                txtWheelScale.Text = vJoyManager.Config.Hardware.WheelScaleFactor_u_per_cts.ToString("G8", CultureInfo.InvariantCulture);

                double center_u = calibwheel.RawMostCenter*scale_u_per_cts;
                vJoyManager.Config.Hardware.WheelCenterOffset_u = center_u;
                txtWheelCenter.Text = vJoyManager.Config.Hardware.WheelCenterOffset_u.ToString("G8", CultureInfo.InvariantCulture);
            }
        }

        private void txtWheelScale_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter)) {
                if (double.TryParse(txtWheelScale.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double scale_u_per_cts)) {
                    vJoyManager.Config.Hardware.WheelScaleFactor_u_per_cts = scale_u_per_cts;
                    txtWheelScale.Text = vJoyManager.Config.Hardware.WheelScaleFactor_u_per_cts.ToString("G8", CultureInfo.InvariantCulture);
                }
            }
        }

        private void txtWheelCenter_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter)) {
                if (double.TryParse(txtWheelCenter.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double center_u)) {
                    vJoyManager.Config.Hardware.WheelCenterOffset_u = center_u;
                    txtWheelCenter.Text = vJoyManager.Config.Hardware.WheelCenterOffset_u.ToString("G8", CultureInfo.InvariantCulture);
                }
            }
        }
        
        #endregion

    }
}
