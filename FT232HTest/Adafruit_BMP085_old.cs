//https://github.com/adafruit/Adafruit-BMP085-Library/blob/master/Adafruit_BMP085.h

namespace FT232HTest
{
    using System;

    internal class Adafruit_BMP085_old : I2CDeviceBase
    {
        private const byte BMP085_I2CADDR = 0x77;

        private const byte BMP085_ULTRALOWPOWER = 0;
        private const byte BMP085_STANDARD = 1;
        private const byte BMP085_HIGHRES = 2;
        private const byte BMP085_ULTRAHIGHRES = 3;

        private const byte BMP085_CAL_AC1 = 0xAA; // R   Calibration data (16 bits)
        private const byte BMP085_CAL_AC2 = 0xAC; // R   Calibration data (16 bits)
        private const byte BMP085_CAL_AC3 = 0xAE; // R   Calibration data (16 bits)    
        private const byte BMP085_CAL_AC4 = 0xB0; // R   Calibration data (16 bits)
        private const byte BMP085_CAL_AC5 = 0xB2; // R   Calibration data (16 bits)
        private const byte BMP085_CAL_AC6 = 0xB4; // R   Calibration data (16 bits)
        private const byte BMP085_CAL_B1 = 0xB6; // R   Calibration data (16 bits)
        private const byte BMP085_CAL_B2 = 0xB8; // R   Calibration data (16 bits)
        private const byte BMP085_CAL_MB = 0xBA; // R   Calibration data (16 bits)
        private const byte BMP085_CAL_MC = 0xBC; // R   Calibration data (16 bits)
        private const byte BMP085_CAL_MD = 0xBE; // R   Calibration data (16 bits)

        private const byte BMP085_CONTROL = 0xF4;
        private const byte BMP085_TEMPDATA = 0xF6;
        private const byte BMP085_PRESSUREDATA = 0xF6;
        private const byte BMP085_READTEMPCMD = 0x2E;
        private const byte BMP085_READPRESSURECMD = 0x34;

        private byte oversampling;
        private Int16 ac1, ac2, ac3, b1, b2, mb, mc, md;
        private UInt16 ac4, ac5, ac6;

        public Adafruit_BMP085_old(I2CBus bus)
            : base(bus, BMP085_I2CADDR)
        {
        }

        public override bool Initialize()
        {
            return this.begin(BMP085_STANDARD);
        }

        private bool begin(byte mode)
        {
            if (mode > BMP085_ULTRAHIGHRES)
            {
                mode = BMP085_ULTRAHIGHRES;
            }
            this.oversampling = mode;

            if (this.ReadRegister8Bits(0xD0) != 0x55) return false;

            /* read calibration data */
            this.ac1 = Convert.ToInt16(this.ReadRegister16Bits(BMP085_CAL_AC1));
            this.ac2 = Convert.ToInt16(this.ReadRegister16Bits(BMP085_CAL_AC2));
            this.ac3 = Convert.ToInt16(this.ReadRegister16Bits(BMP085_CAL_AC3));
            this.ac4 = this.ReadRegister16Bits(BMP085_CAL_AC4);
            this.ac5 = this.ReadRegister16Bits(BMP085_CAL_AC5);
            this.ac6 = this.ReadRegister16Bits(BMP085_CAL_AC6);

            this.b1 = Convert.ToInt16(this.ReadRegister16Bits(BMP085_CAL_B1));
            this.b2 = Convert.ToInt16(this.ReadRegister16Bits(BMP085_CAL_B2));

            this.mb = Convert.ToInt16(this.ReadRegister16Bits(BMP085_CAL_MB));
            this.mc = Convert.ToInt16(this.ReadRegister16Bits(BMP085_CAL_MC));
            this.md = Convert.ToInt16(this.ReadRegister16Bits(BMP085_CAL_MD));

            //#if (BMP085_DEBUG == 1)
            //  Serial.print("ac1 = "); Serial.println(ac1, DEC);
            //  Serial.print("ac2 = "); Serial.println(ac2, DEC);
            //  Serial.print("ac3 = "); Serial.println(ac3, DEC);
            //  Serial.print("ac4 = "); Serial.println(ac4, DEC);
            //  Serial.print("ac5 = "); Serial.println(ac5, DEC);
            //  Serial.print("ac6 = "); Serial.println(ac6, DEC);

            //  Serial.print("b1 = "); Serial.println(b1, DEC);
            //  Serial.print("b2 = "); Serial.println(b2, DEC);

            //  Serial.print("mb = "); Serial.println(mb, DEC);
            //  Serial.print("mc = "); Serial.println(mc, DEC);
            //  Serial.print("md = "); Serial.println(md, DEC);
            //#endif

            return true;
        }

        private Int32 computeB5(Int32 UT)
        {
            Int32 X1 = (UT - (Int32)this.ac6) * ((Int32)this.ac5) >> 15;
            Int32 X2 = ((Int32)this.mc << 11) / (X1 + (Int32)this.md);
            return X1 + X2;
        }

        public UInt16 readRawTemperature()
        {
            this.WriteRegister8Bits(BMP085_CONTROL, BMP085_READTEMPCMD);
            this.SleepMilliSeconds(5);
            //#if BMP085_DEBUG == 1
            //  Serial.print("Raw temp: "); Serial.println(ReadRegister16Bits(BMP085_TEMPDATA));
            //#endif
            return this.ReadRegister16Bits(BMP085_TEMPDATA);
        }

        public UInt32 readRawPressure()
        {
            UInt32 raw;

            this.WriteRegister8Bits(BMP085_CONTROL, Convert.ToByte(BMP085_READPRESSURECMD + (this.oversampling << 6)));

            if (this.oversampling == BMP085_ULTRALOWPOWER)
            {
                this.SleepMilliSeconds(5);
            }
            else if (this.oversampling == BMP085_STANDARD)
            {
                this.SleepMilliSeconds(8);
            }
            else if (this.oversampling == BMP085_HIGHRES)
            {
                this.SleepMilliSeconds(14);
            }
            else
            {
                this.SleepMilliSeconds(26);
            }

            raw = this.ReadRegister16Bits(BMP085_PRESSUREDATA);

            raw <<= 8;
            raw |= this.ReadRegister8Bits(BMP085_PRESSUREDATA + 2);
            raw >>= (8 - this.oversampling);

            /* this pull broke stuff, look at it later?
             if (oversampling==0) {
               raw <<= 8;
               raw |= ReadRegister8Bits(BMP085_PRESSUREDATA+2);
               raw >>= (8 - oversampling);
             }
            */

            //#if BMP085_DEBUG == 1
            //  Serial.print("Raw pressure: "); Serial.println(raw);
            //#endif
            return raw;
        }


        public Int32 readPressure()
        {
            Int32 UT, UP, B3, B5, B6, X1, X2, X3, p;
            UInt32 B4, B7;

            UT = this.readRawTemperature();
            UP = Convert.ToInt32(this.readRawPressure());

            //#if BMP085_DEBUG == 1
            //  // use datasheet numbers!
            //  UT = 27898;
            //  UP = 23843;
            //  ac6 = 23153;
            //  ac5 = 32757;
            //  mc = -8711;
            //  md = 2868;
            //  b1 = 6190;
            //  b2 = 4;
            //  ac3 = -14383;
            //  ac2 = -72;
            //  ac1 = 408;
            //  ac4 = 32741;
            //  oversampling = 0;
            //#endif

            B5 = this.computeB5(UT);

            //#if BMP085_DEBUG == 1
            //  Serial.print("X1 = "); Serial.println(X1);
            //  Serial.print("X2 = "); Serial.println(X2);
            //  Serial.print("B5 = "); Serial.println(B5);
            //#endif

            // do pressure calcs
            B6 = B5 - 4000;
            X1 = ((Int32)this.b2 * ((B6 * B6) >> 12)) >> 11;
            X2 = ((Int32)this.ac2 * B6) >> 11;
            X3 = X1 + X2;
            B3 = ((((Int32)this.ac1 * 4 + X3) << this.oversampling) + 2) / 4;

            //#if BMP085_DEBUG == 1
            //  Serial.print("B6 = "); Serial.println(B6);
            //  Serial.print("X1 = "); Serial.println(X1);
            //  Serial.print("X2 = "); Serial.println(X2);
            //  Serial.print("B3 = "); Serial.println(B3);
            //#endif

            X1 = ((Int32)this.ac3 * B6) >> 13;
            X2 = ((Int32)this.b1 * ((B6 * B6) >> 12)) >> 16;
            X3 = ((X1 + X2) + 2) >> 2;
            B4 = ((UInt32)this.ac4 * (UInt32)(X3 + 32768)) >> 15;
            B7 = Convert.ToUInt32(((UInt32)UP - B3) * (UInt32)(50000UL >> this.oversampling));

            //#if BMP085_DEBUG == 1
            //  Serial.print("X1 = "); Serial.println(X1);
            //  Serial.print("X2 = "); Serial.println(X2);
            //  Serial.print("B4 = "); Serial.println(B4);
            //  Serial.print("B7 = "); Serial.println(B7);
            //#endif

            if (B7 < 0x80000000)
            {
                p = Convert.ToInt32((double)(B7 * 2) / B4);
            }
            else
            {
                p = Convert.ToInt32(((double)B7 / B4) * 2);
            }
            X1 = (p >> 8) * (p >> 8);
            X1 = (X1 * 3038) >> 16;
            X2 = (-7357 * p) >> 16;

            //#if BMP085_DEBUG == 1
            //  Serial.print("p = "); Serial.println(p);
            //  Serial.print("X1 = "); Serial.println(X1);
            //  Serial.print("X2 = "); Serial.println(X2);
            //#endif

            p = p + ((X1 + X2 + (Int32)3791) >> 4);
            //#if BMP085_DEBUG == 1
            //  Serial.print("p = "); Serial.println(p);
            //#endif
            return p;
        }

        public Int32 readSealevelPressure(float altitude_meters)
        {
            float pressure = this.readPressure();
            return (Int32)(pressure / Math.Pow(1.0 - altitude_meters / 44330, 5.255));
        }

        public float readTemperature()
        {
            Int32 UT, B5; // following ds convention
            float temp;

            UT = this.readRawTemperature();

            //#if BMP085_DEBUG == 1
            //  // use datasheet numbers!
            //  UT = 27898;
            //  ac6 = 23153;
            //  ac5 = 32757;
            //  mc = -8711;
            //  md = 2868;
            //#endif

            B5 = this.computeB5(UT);
            temp = (B5 + 8) >> 4;
            temp /= 10;

            return temp;
        }

        public double readAltitude(float sealevelPressure)
        {
            double pressure = this.readPressure();

            double altitude = 44330 * (1.0 - Math.Pow(pressure / sealevelPressure, 0.1903));

            return altitude;
        }
    }
}