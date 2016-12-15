namespace FT232HTest
{
    using System;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;
    using FTD2XX_NET;

    internal partial class FtdiDevice : IDisposable, I2CBus
    {
        private Action<object, bool> debugAction;
        private FTDI ftdi;
        private IntPtr ftdiHandle;
        private bool i2cInitialized;
        private object lockObject = new object();

        public FtdiDevice()
        {
            this.debugAction = (obj, clear) => { };
        }

        public void SetDebugAction(Action<object, bool> action)
        {
            this.debugAction = action;
        }

        public void Debug(object obj, bool clear = false)
        {
            this.debugAction(obj, clear);
        }

        public void Initialize()
        {
            Mpsse.Init_libMPSSE();
        }

        public void Dispose()
        {
            this.Close();
            Mpsse.Cleanup_libMPSSE();
        }

        public bool IsOpen
        {
            //get { return (this.ftdi != null && this.ftdi.IsOpen) || this.ftdiPtr != IntPtr.Zero; }
            get { return this.ftdi != null && this.ftdi.IsOpen; }
        }

        public FTDI.FT_STATUS Close()
        {
            var ftStatus = FTDI.FT_STATUS.FT_OK;

            if (this.ftdi != null && this.ftdi.IsOpen)
            {
                ftStatus = this.ftdi.Close();
                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    this.Debug("Error: FTDI.Close returned " + ftStatus);
                }
                else
                {
                    this.Debug("device closed");
                }
            }
            this.ftdi = null;

            //if (this.ftdiPtr != IntPtr.Zero)
            //{
            //    var ftStatus = (FTDI.FT_STATUS) Mpsse.I2C_CloseChannel(this.ftdiPtr);
            //    if (ftStatus != FTDI.FT_STATUS.FT_OK)
            //    {
            //        this.Debug("Error: FTDI.I2C_CloseChannel returned " + ftStatus);
            //        success = false;
            //    }
            //    else
            //    {
            //        this.Debug("i2c channel closed");
            //    }
            //}
            this.ftdiHandle = IntPtr.Zero;
            this.i2cInitialized = false;

            return ftStatus;
        }

        public FTDI.FT_STATUS EnumerateDevices(Action<UInt32> numberOfDevicesFoundAction, Action<int, FTDI.FT_DEVICE_INFO_NODE> deviceFoundAction)
        {
            UInt32 ftdiDeviceCount = 0;

            var ftdiDevice = new FTDI();
            var ftStatus = ftdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                this.Debug(string.Format("Error: FTDI.GetNumberOfDevices returned {0}", ftStatus));
                return ftStatus;
            }

            if (numberOfDevicesFoundAction != null)
            {
                numberOfDevicesFoundAction(ftdiDeviceCount);
            }

            if (ftdiDeviceCount == 0)
            {
                return FTDI.FT_STATUS.FT_OK;
            }

            var ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];
            ftStatus = ftdiDevice.GetDeviceList(ftdiDeviceList);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                this.Debug(string.Format("Error: FTDI.GetDeviceList returned {0}", ftStatus));
                return ftStatus;
            }

            for (var i = 0; i < ftdiDeviceCount; i++)
            {
                if (deviceFoundAction != null)
                {
                    deviceFoundAction(i, ftdiDeviceList[i]);
                }
            }

            return ftStatus;
        }

        public FTDI.FT_STATUS OpenBySerialNumber(string serialNumber)
        {
            if (this.IsOpen)
            {
                this.Debug("Device is already open");
                return (FTDI.FT_STATUS)1974;
            }

            this.ftdi = new FTDI();
            this.ftdiHandle = IntPtr.Zero;

            var ftStatus = this.ftdi.OpenBySerialNumber(serialNumber);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                this.Debug("Error: FTDI.OpenBySerialNumber returned " + ftStatus);
                return ftStatus;
            }

            this.EnumerateDevices(null, (ix, dev) =>
            {
                if (dev.SerialNumber == serialNumber)
                {
                    this.ftdiHandle = dev.ftHandle;
                }
            });

            if (this.ftdiHandle == IntPtr.Zero)
            {
                this.Debug("Error: cannot get open handle");
                return (FTDI.FT_STATUS)1975;
            }

            this.Debug(string.Format("FTDI device sn:{0} opened ", serialNumber));
            return ftStatus;
        }


        public FTDI.FT_STATUS I2CInit(Mpsse.I2C_CLOCKRATE clockRate, byte latencyTimer, Mpsse.I2C_INIT_OPTIONS options)
        {
            if (!IsOpen)
            {
                this.Debug("Error: device is not open");
                return FTDI.FT_STATUS.FT_DEVICE_NOT_OPENED; ;
            }

            //var ftStatus = (FTDI.FT_STATUS)Mpsse.I2C_OpenChannel(reqChannel, ref this.ftdiPtr);

            Mpsse.ChannelCfg chcfg;
            chcfg.ClockRate = (uint)clockRate;
            chcfg.LatencyTimer = latencyTimer;
            chcfg.Options = (uint)options;
            var ftStatus = (FTDI.FT_STATUS)Mpsse.I2C_InitChannel(this.ftdiHandle, ref chcfg);
            if (ftStatus != 0)
            {
                this.Debug("Error: Mpsse.I2C_InitChannel returned " + ftStatus);
                return ftStatus;
            }

            i2cInitialized = true;
            this.Debug(string.Format("i2c bus initialized clockRate={0} latencyTimer={1} options={2}", clockRate, latencyTimer, options));
            return ftStatus;
        }

        public FTDI.FT_STATUS I2CRead(byte deviceAddress, UInt32 sizeToTransfer, byte[] buffer, ref UInt32 sizeTransfered, Mpsse.I2C_TRANSFER_OPTIONS options)
        {
            if (!IsOpen)
            {
                this.Debug("Error: device is not open");
                return FTDI.FT_STATUS.FT_DEVICE_NOT_OPENED;
            }

            FTDI.FT_STATUS status;
            lock (lockObject)
            {
                status = (FTDI.FT_STATUS)Mpsse.I2C_DeviceRead(this.ftdiHandle, deviceAddress, sizeToTransfer, buffer, ref sizeTransfered, (uint)options);
            }

            if (status != FTDI.FT_STATUS.FT_OK || sizeToTransfer != sizeTransfered)
            {
            }

            return status;
        }

        public FTDI.FT_STATUS I2CWrite(byte deviceAddress, UInt32 sizeToTransfer, byte[] buffer, ref UInt32 sizeTransfered, Mpsse.I2C_TRANSFER_OPTIONS options)
        {
            if (!IsOpen)
            {
                this.Debug("Error: device is not open");
                return FTDI.FT_STATUS.FT_DEVICE_NOT_OPENED;
            }

            FTDI.FT_STATUS status;
            lock (lockObject)
            {
                status = (FTDI.FT_STATUS)Mpsse.I2C_DeviceWrite(this.ftdiHandle, deviceAddress, sizeToTransfer, buffer, ref sizeTransfered, (uint)options);
            }

            if (status != FTDI.FT_STATUS.FT_OK || sizeToTransfer != sizeTransfered)
            {
            }

            return status;
        }

        uint I2CBus.ReadBytes(byte deviceAddress, uint bytesToBeRead, byte[] buffer, ref uint bytesReceived, bool sendStart, bool sendStop, uint options)
        {
            var op = (Mpsse.I2C_TRANSFER_OPTIONS)options;
            if (sendStart) { op |= Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_START_BIT; }
            if (sendStop) { op |= Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_STOP_BIT; }

            return (uint)this.I2CRead(deviceAddress, bytesToBeRead, buffer, ref bytesReceived, op);
        }

        uint I2CBus.WriteBytes(byte deviceAddress, uint bytesToBeSent, byte[] buffer, ref uint bytesSent, bool sendStart, bool sendStop, uint options)
        {
            var op = (Mpsse.I2C_TRANSFER_OPTIONS)options;
            if (sendStart) { op |= Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_START_BIT; }
            if (sendStop) { op |= Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_STOP_BIT; }

            return (uint)this.I2CWrite(deviceAddress, bytesToBeSent, buffer, ref bytesSent, op);
        }

        uint I2CBus.WriteRegister8Bits(byte deviceAddress, byte registerAddress, byte value)
        {
            uint sizeTransfered = 0;
            var buffer = new[]
            {
                registerAddress,
                value
            };

            return (uint)this.I2CWrite(deviceAddress, (uint)buffer.Length, buffer, ref sizeTransfered, Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_START_BIT | Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_STOP_BIT);
        }

        uint I2CBus.WriteRegister16Bits(byte deviceAddress, byte registerAddress, UInt16 value)
        {
            uint sizeTransfered = 0;
            var buffer = new[]
            {
                registerAddress,
                Convert.ToByte((value >> 8) & 0xFF),
                Convert.ToByte(value & 0xFF)
            };

            return (uint)this.I2CWrite(deviceAddress, (uint)buffer.Length, buffer, ref sizeTransfered, Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_START_BIT | Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_STOP_BIT);
        }

        uint I2CBus.WriteRegister32Bits(byte deviceAddress, byte registerAddress, UInt32 value)
        {
            uint sizeTransfered = 0;
            var buffer = new[]
            {
                registerAddress,
                Convert.ToByte((value >> 24) & 0xFF),
                Convert.ToByte((value >> 16) & 0xFF),
                Convert.ToByte((value >> 8) & 0xFF),
                Convert.ToByte(value & 0xFF)
            };

            return (uint)this.I2CWrite(deviceAddress, (uint)buffer.Length, buffer, ref sizeTransfered, Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_START_BIT | Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_STOP_BIT);
        }

        uint I2CBus.ReadRegister8Bits(byte deviceAddress, byte registerAddress, out byte value)
        {
            value = 0;

            uint bytesSent = 0;
            var ftStatus = I2CWrite(deviceAddress, 1, new[] { registerAddress }, ref bytesSent, Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_START_BIT | Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_STOP_BIT);
            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                Thread.Sleep(10);
                var buffer = new Byte[1];
                uint bytesReceived = 0;
                ftStatus = I2CRead(deviceAddress, (uint)buffer.Length, buffer, ref bytesReceived, Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_START_BIT | Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_STOP_BIT);
                value = buffer[0];
            }

            return (uint)ftStatus;
        }

        uint I2CBus.ReadRegister16Bits(byte deviceAddress, byte registerAddress, out UInt16 value)
        {
            value = 0;

            uint bytesSent = 0;
            var ftStatus = I2CWrite(deviceAddress, 1, new[] { registerAddress }, ref bytesSent, Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_START_BIT);
            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                Thread.Sleep(10);
                var buffer = new Byte[2];
                uint bytesReceived = 0;
                ftStatus = I2CRead(deviceAddress, (uint)buffer.Length, buffer, ref bytesReceived, Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_START_BIT | Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_STOP_BIT);
                value = Convert.ToUInt16((buffer[0] << 8) + buffer[1]);
            }

            return (uint)ftStatus;
        }

        uint I2CBus.ReadRegister32Bits(byte deviceAddress, byte registerAddress, out UInt32 value)
        {
            value = 0;

            uint bytesSent = 0;
            var ftStatus = I2CWrite(deviceAddress, 1, new[] { registerAddress }, ref bytesSent, Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_START_BIT | Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_STOP_BIT);
            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                Thread.Sleep(10);
                var buffer = new Byte[4];
                uint bytesReceived = 0;
                ftStatus = I2CRead(deviceAddress, (uint)buffer.Length, buffer, ref bytesReceived, Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_START_BIT | Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_STOP_BIT);
                value = Convert.ToUInt16((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3]);
            }

            return (uint)ftStatus;
        }

        uint I2CBus.ReadRegisterBytes(byte deviceAddress, byte registerAddress, byte[] buffer)
        {
            uint bytesSent = 0;
            var ftStatus = I2CWrite(deviceAddress, 1, new[] { registerAddress }, ref bytesSent, Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_START_BIT | Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_NACK_LAST_BYTE);
           
            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                uint bytesReceived = 0;
                ftStatus = I2CRead(deviceAddress, (uint)buffer.Length, buffer, ref bytesReceived,
                    Mpsse.I2C_TRANSFER_OPTIONS.NONE
                    //| Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_START_BIT 
                    | Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_STOP_BIT 
                    | Mpsse.I2C_TRANSFER_OPTIONS.I2C_TRANSFER_OPTIONS_NACK_LAST_BYTE
                    );
            }

            return (uint)ftStatus;
        }


    }
}



