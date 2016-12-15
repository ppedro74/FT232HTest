namespace FT232HTest
{
    using System;
    using System.Threading;

    public abstract class I2CDeviceBase
    {
        private Action<object, bool> debugAction;
        protected I2CBus i2c;

        protected I2CDeviceBase(I2CBus i2c, byte address)
        {
            this.i2c = i2c;
            this.Address = address;
        }

        public byte Address { get; private set; }

        public abstract bool Initialize();

        public void SetDebugAction(Action<object, bool> action)
        {
            this.debugAction = action;
        }

        public void Debug(object obj, bool clear = false)
        {
            this.debugAction(obj, clear);
        }

        protected void WriteRegister8Bits(byte reg, byte value)
        {
            var result = this.i2c.WriteRegister8Bits(this.Address, reg, value);
            if (result != 0)
            {
                this.Debug(string.Format("Error: WriteRegister8Bits result={0}", result));
            }
        }

        protected byte ReadRegister8Bits(byte reg)
        {
            byte value;
            var result = this.i2c.ReadRegister8Bits(this.Address, reg, out value);
            if (result != 0)
            {
                this.Debug(string.Format("Error: ReadRegister8Bits result={0}", result));
            }
            return value;
        }

        protected void ReadRegister8Bits(byte reg, out byte value)
        {
            var result = this.i2c.ReadRegister8Bits(this.Address, reg, out value);
            if (result != 0)
            {
                this.Debug(string.Format("Error: ReadRegister8Bits result={0}", result));
            }
        }


        protected void WriteRegister16Bits(byte reg, UInt16 value)
        {
            var result = this.i2c.WriteRegister16Bits(this.Address, reg, value);
            if (result != 0)
            {
                this.Debug(string.Format("Error: WriteRegister16Bits result={0}", result));
            }
        }

        protected UInt16 ReadRegister16Bits(byte reg)
        {
            UInt16 value;
            var result = this.i2c.ReadRegister16Bits(this.Address, reg, out value);
            if (result != 0)
            {
                this.Debug(string.Format("Error: ReadRegister16Bits result={0}", result));
            }
            return value;
        }

        protected void ReadRegister16Bits(byte reg, out UInt16 value)
        {
            var result = this.i2c.ReadRegister16Bits(this.Address, reg, out value);
            if (result != 0)
            {
                this.Debug(string.Format("Error: ReadRegister16Bits result={0}", result));
            }
        }

        protected void ReadRegisterSigned16Bits(byte reg, out Int16 value)
        {
            UInt16 u16;

            var result = this.i2c.ReadRegister16Bits(this.Address, reg, out u16);
            value = (Int16)u16;
            if (result != 0)
            {
                this.Debug(string.Format("Error: ReadRegister16Bits result={0}", result));
            }
        }


        protected void SleepMilliSeconds(Int32 ms)
        {
            Thread.Sleep(ms);
        }

    }
}