namespace FT232HTest
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.TabControl1 = new System.Windows.Forms.TabControl();
            this.TabPage0 = new System.Windows.Forms.TabPage();
            this.VL53L0XGroup = new System.Windows.Forms.GroupBox();
            this.VL53L0XScroll = new System.Windows.Forms.CheckBox();
            this.VL53L0XOutput = new System.Windows.Forms.TextBox();
            this.BMP180Group = new System.Windows.Forms.GroupBox();
            this.BMP180Scroll = new System.Windows.Forms.CheckBox();
            this.BMP180Output = new System.Windows.Forms.TextBox();
            this.ADS1115Group = new System.Windows.Forms.GroupBox();
            this.ADS1115Scroll = new System.Windows.Forms.CheckBox();
            this.ADS1115Output = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.VL53L0XEnable = new System.Windows.Forms.CheckBox();
            this.BMP180Enable = new System.Windows.Forms.CheckBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.DeviceComboBox = new System.Windows.Forms.ComboBox();
            this.ADS1115Enable = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TabPage1 = new System.Windows.Forms.TabPage();
            this.DebugText = new System.Windows.Forms.TextBox();
            this.ClearButton = new System.Windows.Forms.Button();
            this.OneSecondTimer = new System.Windows.Forms.Timer(this.components);
            this.TabControl1.SuspendLayout();
            this.TabPage0.SuspendLayout();
            this.VL53L0XGroup.SuspendLayout();
            this.BMP180Group.SuspendLayout();
            this.ADS1115Group.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.TabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.TabPage0);
            this.TabControl1.Controls.Add(this.TabPage1);
            this.TabControl1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TabControl1.Location = new System.Drawing.Point(-1, 16);
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(783, 472);
            this.TabControl1.TabIndex = 5;
            // 
            // TabPage0
            // 
            this.TabPage0.Controls.Add(this.VL53L0XGroup);
            this.TabPage0.Controls.Add(this.BMP180Group);
            this.TabPage0.Controls.Add(this.ADS1115Group);
            this.TabPage0.Controls.Add(this.groupBox4);
            this.TabPage0.Location = new System.Drawing.Point(4, 22);
            this.TabPage0.Name = "TabPage0";
            this.TabPage0.Size = new System.Drawing.Size(775, 446);
            this.TabPage0.TabIndex = 4;
            this.TabPage0.Text = "I2C";
            this.TabPage0.UseVisualStyleBackColor = true;
            // 
            // VL53L0XGroup
            // 
            this.VL53L0XGroup.Controls.Add(this.VL53L0XScroll);
            this.VL53L0XGroup.Controls.Add(this.VL53L0XOutput);
            this.VL53L0XGroup.Enabled = false;
            this.VL53L0XGroup.Location = new System.Drawing.Point(3, 261);
            this.VL53L0XGroup.Name = "VL53L0XGroup";
            this.VL53L0XGroup.Size = new System.Drawing.Size(763, 88);
            this.VL53L0XGroup.TabIndex = 14;
            this.VL53L0XGroup.TabStop = false;
            this.VL53L0XGroup.Text = "VL53L0X";
            // 
            // VL53L0XScroll
            // 
            this.VL53L0XScroll.AutoSize = true;
            this.VL53L0XScroll.Location = new System.Drawing.Point(6, 20);
            this.VL53L0XScroll.Name = "VL53L0XScroll";
            this.VL53L0XScroll.Size = new System.Drawing.Size(58, 17);
            this.VL53L0XScroll.TabIndex = 4;
            this.VL53L0XScroll.Text = "Scroll";
            this.VL53L0XScroll.UseVisualStyleBackColor = true;
            // 
            // VL53L0XOutput
            // 
            this.VL53L0XOutput.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VL53L0XOutput.Location = new System.Drawing.Point(84, 18);
            this.VL53L0XOutput.Multiline = true;
            this.VL53L0XOutput.Name = "VL53L0XOutput";
            this.VL53L0XOutput.ReadOnly = true;
            this.VL53L0XOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.VL53L0XOutput.Size = new System.Drawing.Size(661, 64);
            this.VL53L0XOutput.TabIndex = 3;
            this.VL53L0XOutput.WordWrap = false;
            // 
            // BMP180Group
            // 
            this.BMP180Group.Controls.Add(this.BMP180Scroll);
            this.BMP180Group.Controls.Add(this.BMP180Output);
            this.BMP180Group.Enabled = false;
            this.BMP180Group.Location = new System.Drawing.Point(3, 167);
            this.BMP180Group.Name = "BMP180Group";
            this.BMP180Group.Size = new System.Drawing.Size(763, 88);
            this.BMP180Group.TabIndex = 13;
            this.BMP180Group.TabStop = false;
            this.BMP180Group.Text = "BMP180";
            // 
            // BMP180Scroll
            // 
            this.BMP180Scroll.AutoSize = true;
            this.BMP180Scroll.Location = new System.Drawing.Point(6, 20);
            this.BMP180Scroll.Name = "BMP180Scroll";
            this.BMP180Scroll.Size = new System.Drawing.Size(58, 17);
            this.BMP180Scroll.TabIndex = 4;
            this.BMP180Scroll.Text = "Scroll";
            this.BMP180Scroll.UseVisualStyleBackColor = true;
            // 
            // BMP180Output
            // 
            this.BMP180Output.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BMP180Output.Location = new System.Drawing.Point(84, 18);
            this.BMP180Output.Multiline = true;
            this.BMP180Output.Name = "BMP180Output";
            this.BMP180Output.ReadOnly = true;
            this.BMP180Output.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.BMP180Output.Size = new System.Drawing.Size(661, 64);
            this.BMP180Output.TabIndex = 3;
            this.BMP180Output.WordWrap = false;
            // 
            // ADS1115Group
            // 
            this.ADS1115Group.Controls.Add(this.ADS1115Scroll);
            this.ADS1115Group.Controls.Add(this.ADS1115Output);
            this.ADS1115Group.Enabled = false;
            this.ADS1115Group.Location = new System.Drawing.Point(3, 73);
            this.ADS1115Group.Name = "ADS1115Group";
            this.ADS1115Group.Size = new System.Drawing.Size(763, 88);
            this.ADS1115Group.TabIndex = 12;
            this.ADS1115Group.TabStop = false;
            this.ADS1115Group.Text = "ADS1115";
            // 
            // ADS1115Scroll
            // 
            this.ADS1115Scroll.AutoSize = true;
            this.ADS1115Scroll.Location = new System.Drawing.Point(6, 20);
            this.ADS1115Scroll.Name = "ADS1115Scroll";
            this.ADS1115Scroll.Size = new System.Drawing.Size(58, 17);
            this.ADS1115Scroll.TabIndex = 4;
            this.ADS1115Scroll.Text = "Scroll";
            this.ADS1115Scroll.UseVisualStyleBackColor = true;
            // 
            // ADS1115Output
            // 
            this.ADS1115Output.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ADS1115Output.Location = new System.Drawing.Point(84, 18);
            this.ADS1115Output.Multiline = true;
            this.ADS1115Output.Name = "ADS1115Output";
            this.ADS1115Output.ReadOnly = true;
            this.ADS1115Output.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ADS1115Output.Size = new System.Drawing.Size(661, 64);
            this.ADS1115Output.TabIndex = 3;
            this.ADS1115Output.WordWrap = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.VL53L0XEnable);
            this.groupBox4.Controls.Add(this.BMP180Enable);
            this.groupBox4.Controls.Add(this.ConnectButton);
            this.groupBox4.Controls.Add(this.DeviceComboBox);
            this.groupBox4.Controls.Add(this.ADS1115Enable);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Location = new System.Drawing.Point(3, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(763, 64);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Main";
            // 
            // VL53L0XEnable
            // 
            this.VL53L0XEnable.AutoSize = true;
            this.VL53L0XEnable.Location = new System.Drawing.Point(252, 41);
            this.VL53L0XEnable.Name = "VL53L0XEnable";
            this.VL53L0XEnable.Size = new System.Drawing.Size(75, 17);
            this.VL53L0XEnable.TabIndex = 4;
            this.VL53L0XEnable.Text = "VL53L0X";
            this.VL53L0XEnable.UseVisualStyleBackColor = true;
            this.VL53L0XEnable.CheckedChanged += new System.EventHandler(this.VL53L0XEnable_CheckedChanged);
            // 
            // BMP180Enable
            // 
            this.BMP180Enable.AutoSize = true;
            this.BMP180Enable.Location = new System.Drawing.Point(162, 41);
            this.BMP180Enable.Name = "BMP180Enable";
            this.BMP180Enable.Size = new System.Drawing.Size(71, 17);
            this.BMP180Enable.TabIndex = 3;
            this.BMP180Enable.Text = "BMP180";
            this.BMP180Enable.UseVisualStyleBackColor = true;
            this.BMP180Enable.CheckedChanged += new System.EventHandler(this.BMP180Enable_CheckedChanged);
            // 
            // ConnectButton
            // 
            this.ConnectButton.Enabled = false;
            this.ConnectButton.Location = new System.Drawing.Point(394, 14);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(122, 23);
            this.ConnectButton.TabIndex = 2;
            this.ConnectButton.Tag = "Connect|Disconnect";
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // DeviceComboBox
            // 
            this.DeviceComboBox.DisplayMember = "DisplayName";
            this.DeviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DeviceComboBox.FormattingEnabled = true;
            this.DeviceComboBox.Location = new System.Drawing.Point(84, 14);
            this.DeviceComboBox.Name = "DeviceComboBox";
            this.DeviceComboBox.Size = new System.Drawing.Size(302, 21);
            this.DeviceComboBox.TabIndex = 1;
            this.DeviceComboBox.SelectedIndexChanged += new System.EventHandler(this.DeviceComboBox_SelectedIndexChanged);
            // 
            // ADS1115Enable
            // 
            this.ADS1115Enable.AutoSize = true;
            this.ADS1115Enable.Location = new System.Drawing.Point(84, 41);
            this.ADS1115Enable.Name = "ADS1115Enable";
            this.ADS1115Enable.Size = new System.Drawing.Size(72, 17);
            this.ADS1115Enable.TabIndex = 0;
            this.ADS1115Enable.Text = "ADS115";
            this.ADS1115Enable.UseVisualStyleBackColor = true;
            this.ADS1115Enable.CheckedChanged += new System.EventHandler(this.ADS1115Enable_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Device";
            // 
            // TabPage1
            // 
            this.TabPage1.Controls.Add(this.DebugText);
            this.TabPage1.Location = new System.Drawing.Point(4, 22);
            this.TabPage1.Name = "TabPage1";
            this.TabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.TabPage1.Size = new System.Drawing.Size(775, 446);
            this.TabPage1.TabIndex = 0;
            this.TabPage1.Text = "Debug";
            this.TabPage1.UseVisualStyleBackColor = true;
            // 
            // DebugText
            // 
            this.DebugText.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DebugText.Location = new System.Drawing.Point(6, 6);
            this.DebugText.Multiline = true;
            this.DebugText.Name = "DebugText";
            this.DebugText.ReadOnly = true;
            this.DebugText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.DebugText.Size = new System.Drawing.Size(758, 342);
            this.DebugText.TabIndex = 2;
            this.DebugText.WordWrap = false;
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(707, 494);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(75, 23);
            this.ClearButton.TabIndex = 7;
            this.ClearButton.Text = "Clear";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // OneSecondTimer
            // 
            this.OneSecondTimer.Enabled = true;
            this.OneSecondTimer.Interval = 1000;
            this.OneSecondTimer.Tick += new System.EventHandler(this.OneSecondTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(791, 529);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.TabControl1);
            this.Name = "MainForm";
            this.Text = "FTDITest";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.TabControl1.ResumeLayout(false);
            this.TabPage0.ResumeLayout(false);
            this.VL53L0XGroup.ResumeLayout(false);
            this.VL53L0XGroup.PerformLayout();
            this.BMP180Group.ResumeLayout(false);
            this.BMP180Group.PerformLayout();
            this.ADS1115Group.ResumeLayout(false);
            this.ADS1115Group.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.TabPage1.ResumeLayout(false);
            this.TabPage1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TabControl TabControl1;
        internal System.Windows.Forms.TabPage TabPage1;
        internal System.Windows.Forms.TextBox DebugText;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.TabPage TabPage0;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.ComboBox DeviceComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer OneSecondTimer;
        private System.Windows.Forms.GroupBox ADS1115Group;
        private System.Windows.Forms.CheckBox ADS1115Scroll;
        internal System.Windows.Forms.TextBox ADS1115Output;
        private System.Windows.Forms.CheckBox ADS1115Enable;
        private System.Windows.Forms.GroupBox VL53L0XGroup;
        private System.Windows.Forms.CheckBox VL53L0XScroll;
        internal System.Windows.Forms.TextBox VL53L0XOutput;
        private System.Windows.Forms.GroupBox BMP180Group;
        private System.Windows.Forms.CheckBox BMP180Scroll;
        internal System.Windows.Forms.TextBox BMP180Output;
        private System.Windows.Forms.CheckBox VL53L0XEnable;
        private System.Windows.Forms.CheckBox BMP180Enable;
    }
}