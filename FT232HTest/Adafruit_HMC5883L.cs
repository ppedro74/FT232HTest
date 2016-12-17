namespace FT232HTest
{
    using System;
    using System.Threading;

    public class Adafruit_HMC5883L : I2CDeviceBase
    {
        /*=========================================================================
        I2C ADDRESS/BITS
        -----------------------------------------------------------------------*/
        private const byte HMC5883_ADDRESS_MAG = (0x3C >> 1); // 0011110x
        /*=========================================================================*/

        /*=========================================================================
            REGISTERS
            -----------------------------------------------------------------------*/

        private enum hmc5883MagRegisters : byte
        {
            HMC5883_REGISTER_MAG_CRA_REG_M = 0x00,
            HMC5883_REGISTER_MAG_CRB_REG_M = 0x01,
            HMC5883_REGISTER_MAG_MR_REG_M = 0x02,
            HMC5883_REGISTER_MAG_OUT_X_H_M = 0x03,
            HMC5883_REGISTER_MAG_OUT_X_L_M = 0x04,
            HMC5883_REGISTER_MAG_OUT_Z_H_M = 0x05,
            HMC5883_REGISTER_MAG_OUT_Z_L_M = 0x06,
            HMC5883_REGISTER_MAG_OUT_Y_H_M = 0x07,
            HMC5883_REGISTER_MAG_OUT_Y_L_M = 0x08,
            HMC5883_REGISTER_MAG_SR_REG_Mg = 0x09,
            HMC5883_REGISTER_MAG_IRA_REG_M = 0x0A,
            HMC5883_REGISTER_MAG_IRB_REG_M = 0x0B,
            HMC5883_REGISTER_MAG_IRC_REG_M = 0x0C,
            HMC5883_REGISTER_MAG_TEMP_OUT_H_M = 0x31,
            HMC5883_REGISTER_MAG_TEMP_OUT_L_M = 0x32
        }

        [Flags]
        private enum craFlags
        {
            Bit7_Reserved = 1 << 7,
            Bit6_Sample = 1 << 6,
            Bit5_Sample = 1 << 5,
            Bit4_Data_Output_Rate = 1 << 4,
            Bit3_Data_Output_Rate = 1 << 3,
            Bit2_Data_Output_Rate = 1 << 2,
            Bit1_Measurement_Configuration = 1 << 1,
            Bit0_Measurement_Configuration = 1 << 0,
        }

        [Flags]
        private enum crbFlags
        {
            Bit7_Gain_Configuration = 1 << 7,
            Bit6_Gain_Configuration = 1 << 6,
            Bit5_Gain_Configuration = 1 << 5,
            Bit4 = 1 << 4,
            Bit3 = 1 << 3,
            Bit2 = 1 << 2,
            Bit1 = 1 << 1,
            Bit0 = 1 << 0,
        }

        [Flags]
        private enum crmFlags
        {
            Bit7_HS = 1 << 7,
            Bit6_HS = 1 << 6,
            Bit5_HS = 1 << 5,
            Bit4_HS = 1 << 4,
            Bit3_HS = 1 << 3,
            Bit2_HS = 1 << 2,
            Bit1_Operation_Mode = 1 << 1,
            Bit0_Operation_Mode = 1 << 0,
        }

        /*=========================================================================*/

        /*=========================================================================
            MAGNETOMETER GAIN SETTINGS
            -----------------------------------------------------------------------*/

        private enum hmc5883l_samples
        {
            Sample8 = 64 + 32,
            Sample4 = 64,
            Sample2 = 32,
            Sample1 = 0,
        }

        private enum hmc5883l_data_rate
        {
            Reserved = 4 + 8 + 16,
            DataRate_75HZ = 16 + 8,
            DataRate_30HZ = 16 + 4,
            DataRate_15HZ = 16,
            DataRate_7_5HZ = 4 + 8,
            DataRate_3HZ = 8,
            DataRate_1_5HZ = 4,
            DataRate_0_75_HZ = 0
        }

        private enum hmc5883l_measurement
        {
            Normal = 0,
            Positive = 1,
            Negative = 2,
            Reserved = 3
        }

        private enum hmc5883l_mode
        {
            Continuous = 0,
            Single = 1,
            Idle = 2,
        }


        private enum hmc5883l_gain : byte
        {
            Gain_1_3 = 0x20, // +/- 1.3
            Gain_1_9 = 0x40, // +/- 1.9
            Gain_2_5 = 0x60, // +/- 2.5
            Gain_4_0 = 0x80, // +/- 4.0
            Gain_4_7 = 0xA0, // +/- 4.7
            Gain_5_6 = 0xC0, // +/- 5.6
            Gain_8_1 = 0xE0 // +/- 8.1
        }

        /*=========================================================================*/

        /*=========================================================================
            INTERNAL MAGNETOMETER DATA TYPE
            -----------------------------------------------------------------------*/

        public struct hmc5883MagData
        {
            public double x;
            public double y;
            public double z;
            public double orientation;
        };

        /*=========================================================================*/

        /*=========================================================================
            CHIP ID
            -----------------------------------------------------------------------*/
        private const byte HMC5883_ID = 0xc4;
        /*=========================================================================*/

        private float _hmc5883_Gauss_LSB_XY = 1100.0F; // Varies with gain
        private float _hmc5883_Gauss_LSB_Z = 980.0F; // Varies with gain
        private const int SENSORS_GAUSS_TO_MICROTESLA = 100; /**< Gauss to micro-Tesla multiplier */

        private hmc5883l_gain _magGain;
        private hmc5883MagData _magData; // Last read magnetometer data will be available here

        public Adafruit_HMC5883L(I2CBus bus)
            : base(bus, HMC5883_ADDRESS_MAG)
        {
        }

        public override bool Initialize()
        {
            return this.begin();
        }

        /***************************************************************************
         MAGNETOMETER
         ***************************************************************************/

        /**************************************************************************/
        /*!
            @brief  Reads the raw data from the sensor
        */
        /**************************************************************************/

        public hmc5883MagData read()
        {
            // Read the magnetometer
            //this._magData.x = this.ReadRegister16Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_OUT_X_H_M);
            //this._magData.z = this.ReadRegister16Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_OUT_Z_H_M);
            //this._magData.y = this.ReadRegister16Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_OUT_Y_H_M);

            //var bytes0 = new byte[6];
            //this.ReadRegisterBytes((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_OUT_X_H_M, bytes0);
            //Thread.Sleep(500);
            //var bytes1 = new byte[5];
            //this.ReadRegisterBytes((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_OUT_X_L_M, bytes1);
            //Thread.Sleep(500);

            var bytes = new byte[6];
            byte reg = (byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_OUT_X_H_M;
            for (int ix = 0; ix < 6; ix++)
            {
                bytes[ix] = this.ReadRegister8Bits(reg++);
            }

            // Note high before low (different than accel)  
            byte xhi = bytes[0];
            byte xlo = bytes[1];
            byte zhi = bytes[2];
            byte zlo = bytes[3];
            byte yhi = bytes[4];
            byte ylo = bytes[5];

            // Shift values to create properly formed integer (low byte first)
            this._magData.x = (Int16)(xlo | ((Int16)xhi << 8));
            this._magData.y = (Int16)(ylo | ((Int16)yhi << 8));
            this._magData.z = (Int16)(zlo | ((Int16)zhi << 8));


            this._magData.x = this._magData.x / this._hmc5883_Gauss_LSB_XY * SENSORS_GAUSS_TO_MICROTESLA;
            this._magData.y = this._magData.y / this._hmc5883_Gauss_LSB_XY * SENSORS_GAUSS_TO_MICROTESLA;
            this._magData.z = this._magData.z / this._hmc5883_Gauss_LSB_Z * SENSORS_GAUSS_TO_MICROTESLA;


            // ToDo: Calculate orientation
            //this._magData.orientation = 0.0f;

            /* Display the results (magnetic vector values are in micro-Tesla (uT)) */
            //Serial.print("X: "); Serial.print(event.magnetic.x); Serial.print("  ");
            //Serial.print("Y: "); Serial.print(event.magnetic.y); Serial.print("  ");
            //Serial.print("Z: "); Serial.print(event.magnetic.z); Serial.print("  ");Serial.println("uT");

            // Hold the module so that Z is pointing 'up' and you can measure the heading with x&y
            // Calculate heading when the magnetometer is level, then correct for signs of axis.
            double heading = Math.Atan2(this._magData.y, this._magData.x);

            // Once you have your heading, you must then add your 'Declination Angle', which is the 'Error' of the magnetic field in your location.
            // Find yours here: http://www.magnetic-declination.com/
            // Mine is: -13* 2' W, which is ~13 Degrees, or (which we need) 0.22 radians
            // If you cannot find your Declination, comment out these two lines, your compass will be slightly off.
            float declinationAngle = 0.22f;
            heading += declinationAngle;

            // Correct for when signs are reversed.
            if (heading < 0)
            {
                heading += 2 * Math.PI;
            }

            // Check for wrap due to addition of declination.
            if (heading > 2 * Math.PI)
            {
                heading -= 2 * Math.PI;
            }

            // Convert radians to degrees for readability.
            double headingDegrees = heading * 180 / Math.PI;

            //Serial.print("Heading (degrees): "); Serial.println(headingDegrees);
            this._magData.orientation = headingDegrees;

            return this._magData;
        }

        /**************************************************************************/
        /*!
            @brief  Setups the HW
        */
        /**************************************************************************/

        private bool begin()
        {
            if ((this.ReadRegister8Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_IRA_REG_M) != 0x48)
                || (this.ReadRegister8Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_IRB_REG_M) != 0x34)
                || (this.ReadRegister8Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_IRC_REG_M) != 0x33))
            {
                return false;
            }

            //var ca0 = (craFlags)this.ReadRegister8Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_CRA_REG_M);
            //var cb0 = (crbFlags)this.ReadRegister8Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_CRB_REG_M);
            //var cm0 = (crmFlags)this.ReadRegister8Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_MR_REG_M);

            this.WriteRegister8Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_CRA_REG_M, Convert.ToByte((byte)hmc5883l_samples.Sample1 | (byte)hmc5883l_data_rate.DataRate_15HZ | (byte)hmc5883l_measurement.Normal));

            // Set the gain to a known level
            this.SetConfigurationRegisterB(hmc5883l_gain.Gain_1_3);

            // Enable the magnetometer
            this.WriteRegister8Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_MR_REG_M, (byte)hmc5883l_mode.Continuous);

            //var ca1 = (craFlags)this.ReadRegister8Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_CRA_REG_M);
            //var cb1 = (crbFlags)this.ReadRegister8Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_CRB_REG_M);
            //var cm1 = (crmFlags)this.ReadRegister8Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_MR_REG_M);

            return true;
        }


        /**************************************************************************/
        /*!
            @brief  Sets the magnetometer's gain
        */
        /**************************************************************************/

        private void SetConfigurationRegisterB(hmc5883l_gain gain)
        {
            this.WriteRegister8Bits((byte)hmc5883MagRegisters.HMC5883_REGISTER_MAG_CRB_REG_M, (byte)gain);

            this._magGain = gain;

            switch (gain)
            {
                case hmc5883l_gain.Gain_1_3:
                    this._hmc5883_Gauss_LSB_XY = 1100;
                    this._hmc5883_Gauss_LSB_Z = 980;
                    break;
                case hmc5883l_gain.Gain_1_9:
                    this._hmc5883_Gauss_LSB_XY = 855;
                    this._hmc5883_Gauss_LSB_Z = 760;
                    break;
                case hmc5883l_gain.Gain_2_5:
                    this._hmc5883_Gauss_LSB_XY = 670;
                    this._hmc5883_Gauss_LSB_Z = 600;
                    break;
                case hmc5883l_gain.Gain_4_0:
                    this._hmc5883_Gauss_LSB_XY = 450;
                    this._hmc5883_Gauss_LSB_Z = 400;
                    break;
                case hmc5883l_gain.Gain_4_7:
                    this._hmc5883_Gauss_LSB_XY = 400;
                    this._hmc5883_Gauss_LSB_Z = 255;
                    break;
                case hmc5883l_gain.Gain_5_6:
                    this._hmc5883_Gauss_LSB_XY = 330;
                    this._hmc5883_Gauss_LSB_Z = 295;
                    break;
                case hmc5883l_gain.Gain_8_1:
                    this._hmc5883_Gauss_LSB_XY = 230;
                    this._hmc5883_Gauss_LSB_Z = 205;
                    break;
            }
        }

        ///**************************************************************************/
        ///*! 
        //    @brief  Gets the most recent sensor event
        //*/
        ///**************************************************************************/
        //bool getEvent(sensors_event_t *event) {
        //  /* Clear the event */
        //  memset(event, 0, sizeof(sensors_event_t));

        //  /* Read new data */
        //  read();

        //  event->version   = sizeof(sensors_event_t);
        //  event->sensor_id = _sensorID;
        //  event->type      = SENSOR_TYPE_MAGNETIC_FIELD;
        //  event->timestamp = 0;
        //  event->magnetic.x = _magData.x / _hmc5883_Gauss_LSB_XY * SENSORS_GAUSS_TO_MICROTESLA;
        //  event->magnetic.y = _magData.y / _hmc5883_Gauss_LSB_XY * SENSORS_GAUSS_TO_MICROTESLA;
        //  event->magnetic.z = _magData.z / _hmc5883_Gauss_LSB_Z * SENSORS_GAUSS_TO_MICROTESLA;

        //  return true;
        //}

        ///**************************************************************************/
        ///*! 
        //    @brief  Gets the sensor_t data
        //*/
        ///**************************************************************************/
        //void getSensor(sensor_t *sensor) {
        //  /* Clear the sensor_t object */
        //  memset(sensor, 0, sizeof(sensor_t));

        //  /* Insert the sensor name in the fixed length char array */
        //  strncpy (sensor->name, "HMC5883", sizeof(sensor->name) - 1);
        //  sensor->name[sizeof(sensor->name)- 1] = 0;
        //  sensor->version     = 1;
        //  sensor->sensor_id   = _sensorID;
        //  sensor->type        = SENSOR_TYPE_MAGNETIC_FIELD;
        //  sensor->min_delay   = 0;
        //  sensor->max_value   = 800; // 8 gauss == 800 microTesla
        //  sensor->min_value   = -800; // -8 gauss == -800 microTesla
        //  sensor->resolution  = 0.2; // 2 milligauss == 0.2 microTesla
        //}
    }
}