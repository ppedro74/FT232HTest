namespace FT232HTest
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Forms;
    using FTD2XX_NET;
    using FT232HTest.Properties;

    public partial class MainForm : Form
    {
        enum ModuleAction
        {
            Start,
            Run,
            Terminate
        }
        private readonly FtdiDevice ftdiDevice = new FtdiDevice();
        private object lockObject = new object();
        private Adafruit_HMC5883L adafruit_HMC5883L = null;
        private Adafruit_ADS1115 adafruit_ADS1115 = null;
        private Adafruit_BMP085 adafruit_BMP085 = null;

        public MainForm()
        {
            this.InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.ftdiDevice.SetDebugAction(this.Debug);
            //this.Username.Text = Settings.Default.UserName;

            var items = new List<Item>();
            this.ftdiDevice.EnumerateDevices(null, (ix, dev) => items.Add(new Item { Id = dev.SerialNumber, DisplayName = string.Format("{0}/SN:{1}", dev.Type, dev.SerialNumber) }));
            this.DeviceComboBox.DataSource = items;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.SaveDefaultSettings();
        }

        private void SaveDefaultSettings()
        {
            //Settings.Default.Username = this.Username.Text;
            Settings.Default.Save();
        }

        public void Debug(object obj, bool clear = false)
        {
            string text = obj.ToString();
            this.DebugText.Invoke(new EventHandler(delegate
            {
                if (clear)
                {
                    this.DebugText.Text = string.Empty;
                }
                this.DebugText.Text = this.DebugText.Text + DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + ">" + text + "\r\n";

                this.DebugText.SelectionLength = 0;
                this.DebugText.SelectionStart = this.DebugText.Text.Length;
                this.DebugText.ScrollToCaret();
            }));
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            this.DebugText.Text = string.Empty;
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            this.ConnectButton.Enabled = false;

            var labels = this.ConnectButton.Tag.ToString().Split(new[] { '|' });

            if (this.ftdiDevice.IsOpen)
            {
                this.Debug("Closing device.");
                this.ftdiDevice.Close();
                this.ConnectButton.Text = labels[0];
                this.DeviceComboBox.Enabled = true;

                this.ADS1115Enable.Enabled = true;
            }
            else
            {
                var selectedDevice = (Item)this.DeviceComboBox.SelectedItem;

                this.Debug("Opening device.");
                var success = this.ftdiDevice.OpenBySerialNumber(selectedDevice.Id) == FTDI.FT_STATUS.FT_OK;
                if (success)
                {
                    success = this.ftdiDevice.I2CInit(Mpsse.I2C_CLOCKRATE.I2C_CLOCK_STANDARD_MODE, 100, Mpsse.I2C_INIT_OPTIONS.NONE) == FTDI.FT_STATUS.FT_OK;
                    if (success)
                    {
                        this.ConnectButton.Text = labels[1];
                        this.DeviceComboBox.Enabled = false;
                        this.ADS1115Enable.Enabled = false;

                        this.ModuleADS1115(ModuleAction.Start);
                        this.ModuleBMP085(ModuleAction.Start);
                        this.ModuleHMC5883L(ModuleAction.Start);

                        this.OneSecondTimer.Start();
                    }
                    else
                    {
                        this.OneSecondTimer.Stop();
                        lock (lockObject)
                        {
                            this.ModuleADS1115(ModuleAction.Terminate);
                            this.ModuleBMP085(ModuleAction.Terminate);
                            this.ModuleHMC5883L(ModuleAction.Terminate);

                            this.ftdiDevice.Close();
                        }
                    }
                }
            }

            this.ConnectButton.Enabled = true;
        }

        private void DeviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedDevice = (Item)this.DeviceComboBox.SelectedItem;
            this.ConnectButton.Enabled = selectedDevice != null;
        }

        private void ADS1115Enable_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = (CheckBox)sender;
            this.ADS1115Group.Enabled = checkBox.Checked;
        }

        private void BMP180Enable_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = (CheckBox)sender;
            this.BMP180Group.Enabled = checkBox.Checked;
        }

        private void HMC5883LEnable_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = (CheckBox)sender;
            this.HMC5883LGroup.Enabled = checkBox.Checked;
        }

        private void OneSecondTimer_Tick(object sender, EventArgs e)
        {
            lock (lockObject)
            {
                if (!this.ftdiDevice.IsOpen)
                {
                    return;
                }

                this.ModuleADS1115(ModuleAction.Run);
                this.ModuleBMP085(ModuleAction.Run);
                this.ModuleHMC5883L(ModuleAction.Run);
            }
        }

        public static void Debug(TextBox textBox, CheckBox scrollCheckBox, object obj, bool clear = false)
        {
            string text = obj.ToString();
            textBox.Invoke(new EventHandler(delegate
            {
                if (clear)
                {
                    textBox.Text = string.Empty;
                }
                textBox.Text = textBox.Text + DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + ">" + text + "\r\n";

                if (scrollCheckBox.Checked)
                {
                    textBox.SelectionLength = 0;
                    textBox.SelectionStart = textBox.Text.Length;
                    textBox.ScrollToCaret();
                }
            }));
        }



        private void ModuleHMC5883L(ModuleAction action)
        {
            if (!this.HMC5883LEnable.Checked)
            {
                return;
            }

            switch (action)
            {
                case ModuleAction.Start:
                    {
                        var device = new Adafruit_HMC5883L(this.ftdiDevice);
                        device.SetDebugAction((text, clear) => Debug(this.HMC5883LOutput, this.HMC5883LScroll, text, clear));
                        bool success = device.Initialize();
                        if (success)
                        {
                            lock (lockObject)
                            {
                                this.adafruit_HMC5883L = device;
                            }
                        }
                        break;
                    }
                case ModuleAction.Run:
                    {
                        if (this.adafruit_HMC5883L != null)
                        {
                            var d = this.adafruit_HMC5883L.read();
                            this.adafruit_HMC5883L.Debug(string.Format("heading={3:F2} x={0:F2} y={1:F2} z={2:F2}", d.x, d.y, d.z, d.orientation));
                        }
                        break;
                    }
                case ModuleAction.Terminate:
                    {
                        this.adafruit_HMC5883L = null;
                        break;
                    }
            }
        }

        private void ModuleBMP085(ModuleAction action)
        {
            if (!this.BMP180Enable.Checked)
            {
                return;
            }

            switch (action)
            {
                case ModuleAction.Start:
                    {
                        var device = new Adafruit_BMP085(this.ftdiDevice);
                        device.SetDebugAction((text, clear) => Debug(this.BMP180Output, this.BMP180Scroll, text, clear));
                        bool success = device.Initialize();
                        if (success)
                        {
                            lock (lockObject)
                            {
                                this.adafruit_BMP085 = device;
                            }
                        }
                        break;
                    }
                case ModuleAction.Run:
                    {
                        if (this.adafruit_BMP085 != null)
                        {
                            float t;
                            this.adafruit_BMP085.getTemperature(out t);

                            float p;
                            this.adafruit_BMP085.getPressure(out p);

                            var a = this.adafruit_BMP085.pressureToAltitude(Adafruit_BMP085.PRESSURE_SEALEVELHPA, p);

                            //int t;
                            //this.adafruit_BMP085.readRawTemperature(out t);
                            //int p;
                            //this.adafruit_BMP085.readRawPressure(out p);
                            //int a = 0;
                            
                            this.adafruit_BMP085.Debug(string.Format("Temperature={0} C Pressure={1} Pa Altitute={2}", t, p, a));
                        }

                        break;
                    }
                case ModuleAction.Terminate:
                    {
                        this.adafruit_BMP085 = null;
                        break;
                    }
            }
        }

        private void ModuleADS1115(ModuleAction action)
        {
            if (!this.ADS1115Enable.Checked)
            {
                return;
            }

            switch (action)
            {
                case ModuleAction.Start:
                    {
                        var device = new Adafruit_ADS1115(this.ftdiDevice);
                        device.SetDebugAction((text, clear) => Debug(this.ADS1115Output, this.ADS1115Scroll, text, clear));
                        var success = device.Initialize();
                        if (success)
                        {
                            lock (lockObject)
                            {
                                this.adafruit_ADS1115 = device;
                            }
                        }
                        break;
                    }
                case ModuleAction.Run:
                    {
                        if (this.adafruit_ADS1115 != null)
                        {
                            var a0 = this.adafruit_ADS1115.convertToVoltage(this.adafruit_ADS1115.readADC_SingleEnded(0));
                            var a1 = this.adafruit_ADS1115.convertToVoltage(this.adafruit_ADS1115.readADC_SingleEnded(1));
                            var a2 = this.adafruit_ADS1115.convertToVoltage(this.adafruit_ADS1115.readADC_SingleEnded(2));
                            var a3 = this.adafruit_ADS1115.convertToVoltage(this.adafruit_ADS1115.readADC_SingleEnded(3));
                            this.adafruit_ADS1115.Debug(string.Format("Voltage #0={0:F2} #1={1:F2} #2={2:F2} #3={3:F2}", a0, a1, a2, a3));
                        }
                        break;
                    }
                case ModuleAction.Terminate:
                    {
                        this.adafruit_ADS1115 = null;
                        break;
                    }
            }
        }



    }
}