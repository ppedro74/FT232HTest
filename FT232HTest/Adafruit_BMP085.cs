namespace FT232HTest
{
    using System;

    internal class Adafruit_BMP085 : I2CDeviceBase
    {
        public const float PRESSURE_SEALEVELHPA = 1013.25F; /**< Average sea level pressure is 1013.25 hPa */

        /*=========================================================================
            I2C ADDRESS/BITS
            -----------------------------------------------------------------------*/
        private const byte BMP085_ADDRESS = 0x77;
        /*=========================================================================*/

        /*=========================================================================
            REGISTERS
            -----------------------------------------------------------------------*/

        internal enum Registers
        {
            BMP085_REGISTER_CAL_AC1 = 0xAA, // R   Calibration data (16 bits)
            BMP085_REGISTER_CAL_AC2 = 0xAC, // R   Calibration data (16 bits)
            BMP085_REGISTER_CAL_AC3 = 0xAE, // R   Calibration data (16 bits)
            BMP085_REGISTER_CAL_AC4 = 0xB0, // R   Calibration data (16 bits)
            BMP085_REGISTER_CAL_AC5 = 0xB2, // R   Calibration data (16 bits)
            BMP085_REGISTER_CAL_AC6 = 0xB4, // R   Calibration data (16 bits)
            BMP085_REGISTER_CAL_B1 = 0xB6, // R   Calibration data (16 bits)
            BMP085_REGISTER_CAL_B2 = 0xB8, // R   Calibration data (16 bits)
            BMP085_REGISTER_CAL_MB = 0xBA, // R   Calibration data (16 bits)
            BMP085_REGISTER_CAL_MC = 0xBC, // R   Calibration data (16 bits)
            BMP085_REGISTER_CAL_MD = 0xBE, // R   Calibration data (16 bits)
            BMP085_REGISTER_CHIPID = 0xD0,
            BMP085_REGISTER_VERSION = 0xD1,
            BMP085_REGISTER_SOFTRESET = 0xE0,
            BMP085_REGISTER_CONTROL = 0xF4,
            BMP085_REGISTER_TEMPDATA = 0xF6,
            BMP085_REGISTER_PRESSUREDATA = 0xF6,
            BMP085_REGISTER_READTEMPCMD = 0x2E,
            BMP085_REGISTER_READPRESSURECMD = 0x34
        };

        /*=========================================================================*/

        /*=========================================================================
            MODE SETTINGS
            -----------------------------------------------------------------------*/

        internal enum bmp085_mode_t : byte
        {
            BMP085_MODE_ULTRALOWPOWER = 0,
            BMP085_MODE_STANDARD = 1,
            BMP085_MODE_HIGHRES = 2,
            BMP085_MODE_ULTRAHIGHRES = 3
        }

        /*=========================================================================*/

        /*=========================================================================
            CALIBRATION DATA
            -----------------------------------------------------------------------*/

        internal struct bmp085_calib_data
        {
            internal Int16 ac1;
            internal Int16 ac2;
            internal Int16 ac3;
            internal UInt16 ac4;
            internal UInt16 ac5;
            internal UInt16 ac6;
            internal Int16 b1;
            internal Int16 b2;
            internal Int16 mb;
            internal Int16 mc;
            internal Int16 md;
        }

        /*=========================================================================*/

        private bmp085_calib_data _bmp085_coeffs; // Last read accelerometer data will be available here
        private bmp085_mode_t _bmp085Mode;

        public Adafruit_BMP085(I2CBus bus)
            : base(bus, BMP085_ADDRESS)
        {
        }

        public override bool Initialize()
        {
            return this.begin(bmp085_mode_t.BMP085_MODE_HIGHRES);
        }


        /**************************************************************************/
        /*!
            @brief  Reads the factory-set coefficients
        */
        /**************************************************************************/

        private void readCoefficients()
        {
#if BMP085_USE_DATASHEET_VALS
            _bmp085_coeffs.ac1 = 408;
            _bmp085_coeffs.ac2 = -72;
            _bmp085_coeffs.ac3 = -14383;
            _bmp085_coeffs.ac4 = 32741;
            _bmp085_coeffs.ac5 = 32757;
            _bmp085_coeffs.ac6 = 23153;
            _bmp085_coeffs.b1 = 6190;
            _bmp085_coeffs.b2 = 4;
            _bmp085_coeffs.mb = -32768;
            _bmp085_coeffs.mc = -8711;
            _bmp085_coeffs.md = 2868;
            _bmp085Mode = 0;
#else

            //var buffer = new byte[2];
            //var result = this.i2c.ReadRegisterBytes(this.Address, (byte)Registers.BMP085_REGISTER_CAL_AC1, buffer);

            this.ReadRegisterSigned16Bits((byte)Registers.BMP085_REGISTER_CAL_AC1, out this._bmp085_coeffs.ac1); //error
            this.ReadRegisterSigned16Bits((byte)Registers.BMP085_REGISTER_CAL_AC2, out this._bmp085_coeffs.ac2);
            this.ReadRegisterSigned16Bits((byte)Registers.BMP085_REGISTER_CAL_AC3, out this._bmp085_coeffs.ac3);
            this.ReadRegister16Bits((byte)Registers.BMP085_REGISTER_CAL_AC4, out this._bmp085_coeffs.ac4);
            this.ReadRegister16Bits((byte)Registers.BMP085_REGISTER_CAL_AC5, out this._bmp085_coeffs.ac5); //error
            this.ReadRegister16Bits((byte)Registers.BMP085_REGISTER_CAL_AC6, out this._bmp085_coeffs.ac6);
            this.ReadRegisterSigned16Bits((byte)Registers.BMP085_REGISTER_CAL_B1, out this._bmp085_coeffs.b1);//error
            this.ReadRegisterSigned16Bits((byte)Registers.BMP085_REGISTER_CAL_B2, out this._bmp085_coeffs.b2);
            this.ReadRegisterSigned16Bits((byte)Registers.BMP085_REGISTER_CAL_MB, out this._bmp085_coeffs.mb);
            this.ReadRegisterSigned16Bits((byte)Registers.BMP085_REGISTER_CAL_MC, out this._bmp085_coeffs.mc);
            this.ReadRegisterSigned16Bits((byte)Registers.BMP085_REGISTER_CAL_MD, out this._bmp085_coeffs.md);//error
#endif
        }

        /**************************************************************************/
        /*!
        */
        /**************************************************************************/

        public void readRawTemperature(out Int32 temperature)
        {
#if BMP085_USE_DATASHEET_VALS
            temperature = 27898;
#else
            UInt16 t;
            this.WriteRegister8Bits((byte)Registers.BMP085_REGISTER_CONTROL, (byte)Registers.BMP085_REGISTER_READTEMPCMD);
            this.SleepMilliSeconds(5);
            this.ReadRegister16Bits((byte)Registers.BMP085_REGISTER_TEMPDATA, out t);
            temperature = t;
#endif
        }

        /**************************************************************************/
        /*!
        */
        /**************************************************************************/

        public void readRawPressure(out Int32 pressure)
        {
#if BMP085_USE_DATASHEET_VALS
            pressure = 23843;
#else
            byte p8;
            UInt16 p16;
            Int32 p32;

            this.WriteRegister8Bits((byte)Registers.BMP085_REGISTER_CONTROL, Convert.ToByte((byte)Registers.BMP085_REGISTER_READPRESSURECMD + ((byte)this._bmp085Mode << 6)));
            switch (this._bmp085Mode)
            {
                case bmp085_mode_t.BMP085_MODE_ULTRALOWPOWER:
                    this.SleepMilliSeconds(5);
                    break;
                case bmp085_mode_t.BMP085_MODE_STANDARD:
                    this.SleepMilliSeconds(8);
                    break;
                case bmp085_mode_t.BMP085_MODE_HIGHRES:
                    this.SleepMilliSeconds(14);
                    break;
                case bmp085_mode_t.BMP085_MODE_ULTRAHIGHRES:
                default:
                    this.SleepMilliSeconds(26);
                    break;
            }

            this.ReadRegister16Bits((byte)Registers.BMP085_REGISTER_PRESSUREDATA, out p16);
            p32 = (Int32)((UInt32)p16 << 8);

            this.ReadRegister8Bits((byte)Registers.BMP085_REGISTER_PRESSUREDATA + 2, out p8);
            p32 += p8;
            p32 >>= (8 - (byte)this._bmp085Mode);

            pressure = p32;
#endif
        }

        /**************************************************************************/
        /*!
            @brief  Compute B5 coefficient used in temperature & pressure calcs.
        */
        /**************************************************************************/

        private Int32 computeB5(Int32 ut)
        {
            Int32 X1 = (ut - (Int32)this._bmp085_coeffs.ac6) * ((Int32)this._bmp085_coeffs.ac5) >> 15;
            Int32 X2 = ((Int32)this._bmp085_coeffs.mc << 11) / (X1 + (Int32)this._bmp085_coeffs.md);
            return X1 + X2;
        }


        /**************************************************************************/
        /*!
            @brief  Setups the HW
        */
        /**************************************************************************/

        private bool begin(bmp085_mode_t mode)
        {
            this.Debug("Initializing BMP180");

            /* Mode boundary check */
            if ((mode > bmp085_mode_t.BMP085_MODE_ULTRAHIGHRES) || (mode < 0))
            {
                mode = bmp085_mode_t.BMP085_MODE_ULTRAHIGHRES;
            }

            /* Make sure we have the right device */
            byte id;
            this.ReadRegister8Bits((byte)Registers.BMP085_REGISTER_CHIPID, out id);
            if (id != 0x55)
            {
                this.Debug("Error: BMP180 not found.");
                return false;
            }

            /* Set the mode indicator */
            this._bmp085Mode = mode;

            /* Coefficients need to be read once */
            this.readCoefficients();

            this.Debug("BMP180 Initialized");

            return true;
        }

        /**************************************************************************/
        /*!
            @brief  Gets the compensated pressure level in kPa
        */
        /**************************************************************************/

        public void getPressure(out float pressure)
        {
            Int32 ut = 0, up = 0, compp = 0;
            Int32 x1, x2, b5, b6, x3, b3, p;
            UInt32 b4, b7;

            /* Get the raw pressure and temperature values */
            this.readRawTemperature(out ut);
            this.readRawPressure(out up);

            /* Temperature compensation */
            b5 = this.computeB5(ut);

            /* Pressure compensation */
            b6 = b5 - 4000;
            x1 = (this._bmp085_coeffs.b2 * ((b6 * b6) >> 12)) >> 11;
            x2 = (this._bmp085_coeffs.ac2 * b6) >> 11;
            x3 = x1 + x2;
            b3 = (((((Int32)this._bmp085_coeffs.ac1) * 4 + x3) << (byte)this._bmp085Mode) + 2) >> 2;
            x1 = (this._bmp085_coeffs.ac3 * b6) >> 13;
            x2 = (this._bmp085_coeffs.b1 * ((b6 * b6) >> 12)) >> 16;
            x3 = ((x1 + x2) + 2) >> 2;
            b4 = (this._bmp085_coeffs.ac4 * (UInt32)(x3 + 32768)) >> 15;
            b7 = Convert.ToUInt32(((UInt32)(up - b3) * (50000 >> (byte)this._bmp085Mode)));

            if (b7 < 0x80000000)
            {
                p = (int)((b7 << 1) / b4);
            }
            else
            {
                p = (int)((b7 / b4) << 1);
            }

            x1 = (p >> 8) * (p >> 8);
            x1 = (x1 * 3038) >> 16;
            x2 = (-7357 * p) >> 16;
            compp = p + ((x1 + x2 + 3791) >> 4);

            /* Assign compensated pressure value */
            pressure = compp;
        }

        /**************************************************************************/
        /*!
            @brief  Reads the temperatures in degrees Celsius
        */
        /**************************************************************************/

        public void getTemperature(out float temp)
        {
            Int32 UT, B5; // following ds convention
            float t;

            this.readRawTemperature(out UT);

#if BMP085_USE_DATASHEET_VALS
            // use datasheet numbers!
            UT = 27898;
            _bmp085_coeffs.ac6 = 23153;
            _bmp085_coeffs.ac5 = 32757;
            _bmp085_coeffs.mc = -8711;
            _bmp085_coeffs.md = 2868;
#endif

            B5 = this.computeB5(UT);
            t = (B5 + 8) >> 4;
            t /= 10;

            temp = t;
        }

        /**************************************************************************/
        /*!
            Calculates the altitude (in meters) from the specified atmospheric
            pressure (in hPa), and sea-level pressure (in hPa).
            @param  seaLevel      Sea-level pressure in hPa
            @param  atmospheric   Atmospheric pressure in hPa
        */
        /**************************************************************************/

        public double pressureToAltitude(float seaLevel, float atmospheric)
        {
            // Equation taken from BMP180 datasheet (page 16):
            //  http://www.adafruit.com/datasheets/BST-BMP180-DS000-09.pdf

            // Note that using the equation from wikipedia can give bad results
            // at high altitude.  See this thread for more information:
            //  http://forums.adafruit.com/viewtopic.php?f=22&t=58064

            return 44330.0 * (1.0 - Math.Pow(atmospheric / seaLevel, 0.1903));
        }

        /**************************************************************************/
        /*!
            Calculates the altitude (in meters) from the specified atmospheric
            pressure (in hPa), and sea-level pressure (in hPa).  Note that this
            function just calls the overload of pressureToAltitude which takes
            seaLevel and atmospheric pressure--temperature is ignored.  The original
            implementation of this function was based on calculations from Wikipedia
            which are not accurate at higher altitudes.  To keep compatibility with
            old code this function remains with the same interface, but it calls the
            more accurate calculation.
            @param  seaLevel      Sea-level pressure in hPa
            @param  atmospheric   Atmospheric pressure in hPa
            @param  temp          Temperature in degrees Celsius
        */
        /**************************************************************************/

        public double pressureToAltitude(float seaLevel, float atmospheric, float temp)
        {
            return this.pressureToAltitude(seaLevel, atmospheric);
        }

        /**************************************************************************/
        /*!
            Calculates the pressure at sea level (in hPa) from the specified altitude 
            (in meters), and atmospheric pressure (in hPa).  
            @param  altitude      Altitude in meters
            @param  atmospheric   Atmospheric pressure in hPa
        */
        /**************************************************************************/

        public double seaLevelForAltitude(float altitude, float atmospheric)
        {
            // Equation taken from BMP180 datasheet (page 17):
            //  http://www.adafruit.com/datasheets/BST-BMP180-DS000-09.pdf

            // Note that using the equation from wikipedia can give bad results
            // at high altitude.  See this thread for more information:
            //  http://forums.adafruit.com/viewtopic.php?f=22&t=58064

            return atmospheric / Math.Pow(1.0 - (altitude / 44330.0), 5.255);
        }

        /**************************************************************************/
        /*!
            Calculates the pressure at sea level (in hPa) from the specified altitude 
            (in meters), and atmospheric pressure (in hPa).  Note that this
            function just calls the overload of seaLevelForAltitude which takes
            altitude and atmospheric pressure--temperature is ignored.  The original
            implementation of this function was based on calculations from Wikipedia
            which are not accurate at higher altitudes.  To keep compatibility with
            old code this function remains with the same interface, but it calls the
            more accurate calculation.
            @param  altitude      Altitude in meters
            @param  atmospheric   Atmospheric pressure in hPa
            @param  temp          Temperature in degrees Celsius
        */
        /**************************************************************************/

        public double seaLevelForAltitude(float altitude, float atmospheric, float temp)
        {
            return this.seaLevelForAltitude(altitude, atmospheric);
        }
    }
}