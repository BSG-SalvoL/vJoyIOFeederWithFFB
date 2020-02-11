﻿namespace IOFeederGUI.GUI
{
    partial class TargetHdwForm
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timerRefresh = new System.Windows.Forms.Timer(this.components);
            this.cmbSelectMode = new System.Windows.Forms.ComboBox();
            this.btnStartStopManager = new System.Windows.Forms.Button();
            this.btnOpenvJoyConfig = new System.Windows.Forms.Button();
            this.btnOpenvJoyMonitor = new System.Windows.Forms.Button();
            this.btnOpenJoyCPL = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnDeviceReady = new System.Windows.Forms.Button();
            this.chkEmulateMissing = new System.Windows.Forms.CheckBox();
            this.chkPulsedTrq = new System.Windows.Forms.CheckBox();
            this.chkInvertTorque = new System.Windows.Forms.CheckBox();
            this.chkInvertWheel = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timerRefresh
            // 
            this.timerRefresh.Enabled = true;
            this.timerRefresh.Interval = 500;
            this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
            // 
            // cmbSelectMode
            // 
            this.cmbSelectMode.AllowDrop = true;
            this.cmbSelectMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSelectMode.FormattingEnabled = true;
            this.cmbSelectMode.Location = new System.Drawing.Point(12, 54);
            this.cmbSelectMode.Name = "cmbSelectMode";
            this.cmbSelectMode.Size = new System.Drawing.Size(121, 21);
            this.cmbSelectMode.TabIndex = 18;
            this.cmbSelectMode.SelectedIndexChanged += new System.EventHandler(this.cmbSelectMode_SelectedIndexChanged);
            // 
            // btnStartStopManager
            // 
            this.btnStartStopManager.Location = new System.Drawing.Point(143, 54);
            this.btnStartStopManager.Name = "btnStartStopManager";
            this.btnStartStopManager.Size = new System.Drawing.Size(121, 21);
            this.btnStartStopManager.TabIndex = 17;
            this.btnStartStopManager.UseVisualStyleBackColor = true;
            this.btnStartStopManager.Click += new System.EventHandler(this.btnStartStopManager_Click);
            // 
            // btnOpenvJoyConfig
            // 
            this.btnOpenvJoyConfig.Location = new System.Drawing.Point(276, 12);
            this.btnOpenvJoyConfig.Name = "btnOpenvJoyConfig";
            this.btnOpenvJoyConfig.Size = new System.Drawing.Size(121, 21);
            this.btnOpenvJoyConfig.TabIndex = 15;
            this.btnOpenvJoyConfig.Text = "Open vJoy Conf";
            this.btnOpenvJoyConfig.UseVisualStyleBackColor = true;
            this.btnOpenvJoyConfig.Click += new System.EventHandler(this.btnOpenvJoyConfig_Click);
            // 
            // btnOpenvJoyMonitor
            // 
            this.btnOpenvJoyMonitor.Location = new System.Drawing.Point(143, 12);
            this.btnOpenvJoyMonitor.Name = "btnOpenvJoyMonitor";
            this.btnOpenvJoyMonitor.Size = new System.Drawing.Size(121, 21);
            this.btnOpenvJoyMonitor.TabIndex = 14;
            this.btnOpenvJoyMonitor.Text = "Open vJoy Monitor";
            this.btnOpenvJoyMonitor.UseVisualStyleBackColor = true;
            this.btnOpenvJoyMonitor.Click += new System.EventHandler(this.btnOpenvJoyMonitor_Click);
            // 
            // btnOpenJoyCPL
            // 
            this.btnOpenJoyCPL.Location = new System.Drawing.Point(12, 12);
            this.btnOpenJoyCPL.Name = "btnOpenJoyCPL";
            this.btnOpenJoyCPL.Size = new System.Drawing.Size(121, 21);
            this.btnOpenJoyCPL.TabIndex = 13;
            this.btnOpenJoyCPL.Text = "Open Joy.cpl";
            this.btnOpenJoyCPL.UseVisualStyleBackColor = true;
            this.btnOpenJoyCPL.Click += new System.EventHandler(this.btnOpenJoyCPL_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Target hardware :";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.btnDeviceReady);
            this.splitContainer1.Panel1.Controls.Add(this.cmbSelectMode);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.btnOpenvJoyMonitor);
            this.splitContainer1.Panel1.Controls.Add(this.btnOpenvJoyConfig);
            this.splitContainer1.Panel1.Controls.Add(this.btnStartStopManager);
            this.splitContainer1.Panel1.Controls.Add(this.btnOpenJoyCPL);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label4);
            this.splitContainer1.Panel2.Controls.Add(this.chkEmulateMissing);
            this.splitContainer1.Panel2.Controls.Add(this.chkPulsedTrq);
            this.splitContainer1.Panel2.Controls.Add(this.chkInvertTorque);
            this.splitContainer1.Panel2.Controls.Add(this.chkInvertWheel);
            this.splitContainer1.Size = new System.Drawing.Size(634, 436);
            this.splitContainer1.SplitterDistance = 125;
            this.splitContainer1.TabIndex = 20;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(143, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "Manager status";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(276, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "Device status";
            // 
            // btnDeviceReady
            // 
            this.btnDeviceReady.Enabled = false;
            this.btnDeviceReady.Location = new System.Drawing.Point(276, 54);
            this.btnDeviceReady.Name = "btnDeviceReady";
            this.btnDeviceReady.Size = new System.Drawing.Size(121, 21);
            this.btnDeviceReady.TabIndex = 20;
            this.btnDeviceReady.Text = "--";
            this.btnDeviceReady.UseVisualStyleBackColor = true;
            // 
            // chkEmulateMissing
            // 
            this.chkEmulateMissing.AutoSize = true;
            this.chkEmulateMissing.Enabled = false;
            this.chkEmulateMissing.Location = new System.Drawing.Point(12, 85);
            this.chkEmulateMissing.Name = "chkEmulateMissing";
            this.chkEmulateMissing.Size = new System.Drawing.Size(372, 17);
            this.chkEmulateMissing.TabIndex = 3;
            this.chkEmulateMissing.Text = "Emulated missing effects (if uncheck, missing effects will not be emulated)";
            this.chkEmulateMissing.UseVisualStyleBackColor = true;
            this.chkEmulateMissing.Click += new System.EventHandler(this.chkEmulateMissing_Click);
            // 
            // chkPulsedTrq
            // 
            this.chkPulsedTrq.AutoSize = true;
            this.chkPulsedTrq.Enabled = false;
            this.chkPulsedTrq.Location = new System.Drawing.Point(12, 62);
            this.chkPulsedTrq.Name = "chkPulsedTrq";
            this.chkPulsedTrq.Size = new System.Drawing.Size(282, 17);
            this.chkPulsedTrq.TabIndex = 2;
            this.chkPulsedTrq.Text = "Use quarter-pulsed Torque (increase torque resolution)";
            this.chkPulsedTrq.UseVisualStyleBackColor = true;
            this.chkPulsedTrq.Click += new System.EventHandler(this.chkPulsedTrq_Click);
            // 
            // chkInvertTorque
            // 
            this.chkInvertTorque.AutoSize = true;
            this.chkInvertTorque.Enabled = false;
            this.chkInvertTorque.Location = new System.Drawing.Point(12, 39);
            this.chkInvertTorque.Name = "chkInvertTorque";
            this.chkInvertTorque.Size = new System.Drawing.Size(190, 17);
            this.chkInvertTorque.TabIndex = 1;
            this.chkInvertTorque.Text = "Invert Torque (change torque sign)";
            this.chkInvertTorque.UseVisualStyleBackColor = true;
            // 
            // chkInvertWheel
            // 
            this.chkInvertWheel.AutoSize = true;
            this.chkInvertWheel.Enabled = false;
            this.chkInvertWheel.Location = new System.Drawing.Point(12, 16);
            this.chkInvertWheel.Name = "chkInvertWheel";
            this.chkInvertWheel.Size = new System.Drawing.Size(230, 17);
            this.chkInvertWheel.TabIndex = 0;
            this.chkInvertWheel.Text = "Invert Wheel Direction (change wheel sign)";
            this.chkInvertWheel.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(334, 13);
            this.label4.TabIndex = 23;
            this.label4.Text = "Internal parameters (some may be changed when manager is running)";
            // 
            // TargetHdwForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(634, 436);
            this.Controls.Add(this.splitContainer1);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "TargetHdwForm";
            this.Text = "vJoyIOFeeder";
            this.Load += new System.EventHandler(this.TargetHdwForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timerRefresh;
        private System.Windows.Forms.Button btnOpenvJoyConfig;
        private System.Windows.Forms.Button btnOpenvJoyMonitor;
        private System.Windows.Forms.Button btnOpenJoyCPL;
        private System.Windows.Forms.Button btnStartStopManager;
        private System.Windows.Forms.ComboBox cmbSelectMode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.CheckBox chkEmulateMissing;
        private System.Windows.Forms.CheckBox chkPulsedTrq;
        private System.Windows.Forms.CheckBox chkInvertTorque;
        private System.Windows.Forms.CheckBox chkInvertWheel;
        private System.Windows.Forms.Button btnDeviceReady;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}

