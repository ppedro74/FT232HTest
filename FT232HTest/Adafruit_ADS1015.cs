//  Original "Code Logic" from:
//      @file     Adafruit_ADS1015.cpp
//      @author   K.Townsend (Adafruit Industries)
//      @license  BSD (see license.txt)
//      Driver for the ADS1015/ADS1115 ADC
//      https://github.com/adafruit/Adafruit_ADS1X15/blob/master/Adafruit_ADS1015.cpp

namespace FT232HTest
{
    using System;

    public class Adafruit_ADS1015 : I2CDeviceBase
    {
        protected byte m_conversionDelay;
        protected byte m_bitShift;
        protected adsGain_t m_gain;

        /*=========================================================================
        I2C ADDRESS/BITS
        -----------------------------------------------------------------------*/
        protected const byte ADS1015_ADDRESS = 0x48; // 1001 000 =ADDR = GND;
        /*=========================================================================*/

        /*=========================================================================
            CONVERSION DELAY =in mS;
            -----------------------------------------------------------------------*/
        protected const byte ADS1015_CONVERSIONDELAY = 1;
        protected const byte ADS1115_CONVERSIONDELAY = 8;
        /*=========================================================================*/

        /*=========================================================================
            POINTER REGISTER
            -----------------------------------------------------------------------*/
        protected const byte ADS1015_REG_POINTER_MASK = 0x03;
        protected const byte ADS1015_REG_POINTER_CONVERT = 0x00;
        protected const byte ADS1015_REG_POINTER_CONFIG = 0x01;
        protected const byte ADS1015_REG_POINTER_LOWTHRESH = 0x02;
        protected const byte ADS1015_REG_POINTER_HITHRESH = 0x03;
        /*=========================================================================*/

        /*=========================================================================
            CONFIG REGISTER
            -----------------------------------------------------------------------*/
        protected const UInt16 ADS1015_REG_CONFIG_OS_MASK = 0x8000;
        protected const UInt16 ADS1015_REG_CONFIG_OS_SINGLE = 0x8000; // Write: Set to start a single-conversion
        protected const UInt16 ADS1015_REG_CONFIG_OS_BUSY = 0x0000; // Read: Bit = 0 when conversion is in progress
        protected const UInt16 ADS1015_REG_CONFIG_OS_NOTBUSY = 0x8000; // Read: Bit = 1 when device is not performing a conversion

        protected const UInt16 ADS1015_REG_CONFIG_MUX_MASK = 0x7000;
        protected const UInt16 ADS1015_REG_CONFIG_MUX_DIFF_0_1 = 0x0000; // Differential P = AIN0, N = AIN1 =default;
        protected const UInt16 ADS1015_REG_CONFIG_MUX_DIFF_0_3 = 0x1000; // Differential P = AIN0, N = AIN3
        protected const UInt16 ADS1015_REG_CONFIG_MUX_DIFF_1_3 = 0x2000; // Differential P = AIN1, N = AIN3
        protected const UInt16 ADS1015_REG_CONFIG_MUX_DIFF_2_3 = 0x3000; // Differential P = AIN2, N = AIN3
        protected const UInt16 ADS1015_REG_CONFIG_MUX_SINGLE_0 = 0x4000; // Single-ended AIN0
        protected const UInt16 ADS1015_REG_CONFIG_MUX_SINGLE_1 = 0x5000; // Single-ended AIN1
        protected const UInt16 ADS1015_REG_CONFIG_MUX_SINGLE_2 = 0x6000; // Single-ended AIN2
        protected const UInt16 ADS1015_REG_CONFIG_MUX_SINGLE_3 = 0x7000; // Single-ended AIN3

        protected const UInt16 ADS1015_REG_CONFIG_PGA_MASK = 0x0E00;
        protected const UInt16 ADS1015_REG_CONFIG_PGA_6_144V = 0x0000; // +/-6.144V range = Gain 2/3
        protected const UInt16 ADS1015_REG_CONFIG_PGA_4_096V = 0x0200; // +/-4.096V range = Gain 1
        protected const UInt16 ADS1015_REG_CONFIG_PGA_2_048V = 0x0400; // +/-2.048V range = Gain 2 =default;
        protected const UInt16 ADS1015_REG_CONFIG_PGA_1_024V = 0x0600; // +/-1.024V range = Gain 4
        protected const UInt16 ADS1015_REG_CONFIG_PGA_0_512V = 0x0800; // +/-0.512V range = Gain 8
        protected const UInt16 ADS1015_REG_CONFIG_PGA_0_256V = 0x0A00; // +/-0.256V range = Gain 16

        protected const UInt16 ADS1015_REG_CONFIG_MODE_MASK = 0x0100;
        protected const UInt16 ADS1015_REG_CONFIG_MODE_CONTIN = 0x0000; // Continuous conversion mode
        protected const UInt16 ADS1015_REG_CONFIG_MODE_SINGLE = 0x0100; // Power-down single-shot mode =default;

        protected const UInt16 ADS1015_REG_CONFIG_DR_MASK = 0x00E0;
        protected const UInt16 ADS1015_REG_CONFIG_DR_128SPS = 0x0000; // 128 samples per second
        protected const UInt16 ADS1015_REG_CONFIG_DR_250SPS = 0x0020; // 250 samples per second
        protected const UInt16 ADS1015_REG_CONFIG_DR_490SPS = 0x0040; // 490 samples per second
        protected const UInt16 ADS1015_REG_CONFIG_DR_920SPS = 0x0060; // 920 samples per second
        protected const UInt16 ADS1015_REG_CONFIG_DR_1600SPS = 0x0080; // 1600 samples per second =default;
        protected const UInt16 ADS1015_REG_CONFIG_DR_2400SPS = 0x00A0; // 2400 samples per second
        protected const UInt16 ADS1015_REG_CONFIG_DR_3300SPS = 0x00C0; // 3300 samples per second

        protected const UInt16 ADS1015_REG_CONFIG_CMODE_MASK = 0x0010;
        protected const UInt16 ADS1015_REG_CONFIG_CMODE_TRAD = 0x0000; // Traditional comparator with hysteresis =default;
        protected const UInt16 ADS1015_REG_CONFIG_CMODE_WINDOW = 0x0010; // Window comparator

        protected const UInt16 ADS1015_REG_CONFIG_CPOL_MASK = 0x0008;
        protected const UInt16 ADS1015_REG_CONFIG_CPOL_ACTVLOW = 0x0000; // ALERT/RDY pin is low when active =default;
        protected const UInt16 ADS1015_REG_CONFIG_CPOL_ACTVHI = 0x0008; // ALERT/RDY pin is high when active

        protected const UInt16 ADS1015_REG_CONFIG_CLAT_MASK = 0x0004; // Determines if ALERT/RDY pin latches once asserted
        protected const UInt16 ADS1015_REG_CONFIG_CLAT_NONLAT = 0x0000; // Non-latching comparator =default;
        protected const UInt16 ADS1015_REG_CONFIG_CLAT_LATCH = 0x0004; // Latching comparator

        protected const UInt16 ADS1015_REG_CONFIG_CQUE_MASK = 0x0003;
        protected const UInt16 ADS1015_REG_CONFIG_CQUE_1CONV = 0x0000; // Assert ALERT/RDY after one conversions
        protected const UInt16 ADS1015_REG_CONFIG_CQUE_2CONV = 0x0001; // Assert ALERT/RDY after two conversions
        protected const UInt16 ADS1015_REG_CONFIG_CQUE_4CONV = 0x0002; // Assert ALERT/RDY after four conversions
        protected const UInt16 ADS1015_REG_CONFIG_CQUE_NONE = 0x0003; // Disable the comparator and put ALERT/RDY in high state =default;
        /*=========================================================================*/

        protected enum adsGain_t : ushort
        {
            GAIN_TWOTHIRDS = ADS1015_REG_CONFIG_PGA_6_144V,
            GAIN_ONE = ADS1015_REG_CONFIG_PGA_4_096V,
            GAIN_TWO = ADS1015_REG_CONFIG_PGA_2_048V,
            GAIN_FOUR = ADS1015_REG_CONFIG_PGA_1_024V,
            GAIN_EIGHT = ADS1015_REG_CONFIG_PGA_0_512V,
            GAIN_SIXTEEN = ADS1015_REG_CONFIG_PGA_0_256V
        }

        /**************************************************************************/
        /*!
            @brief  Instantiates a new ADS1015 class w/appropriate properties
        */
        /**************************************************************************/

        public Adafruit_ADS1015(I2CBus i2c)
            : base(i2c, ADS1015_ADDRESS)
        {
            this.m_conversionDelay = ADS1015_CONVERSIONDELAY;
            this.m_bitShift = 4;
            this.m_gain = adsGain_t.GAIN_TWOTHIRDS; /* +/- 6.144V range (limited to VDD +0.3V max!) */
        }

        public Adafruit_ADS1015(I2CBus i2c, byte address)
            : base(i2c, address)
        {
            this.m_conversionDelay = ADS1015_CONVERSIONDELAY;
            this.m_bitShift = 4;
            this.m_gain = adsGain_t.GAIN_TWOTHIRDS; /* +/- 6.144V range (limited to VDD +0.3V max!) */
        }

        public override bool Initialize()
        {
            return true;
        }

        /**************************************************************************/
        /*!
            @brief  Sets the gain and input voltage range
        */
        /**************************************************************************/

        protected void setGain(adsGain_t gain)
        {
            this.m_gain = gain;
        }

        /**************************************************************************/
        /*!
            @brief  Gets a gain and input voltage range
        */
        /**************************************************************************/

        protected adsGain_t getGain()
        {
            return this.m_gain;
        }

        /**************************************************************************/
        /*!
            @brief  Gets a single-ended ADC reading from the specified channel
        */
        /**************************************************************************/

        public Int16 readADC_SingleEnded(byte channel)
        {
            if (channel > 3)
            {
                return 0;
            }

            // Start with default values
            UInt16 config = ADS1015_REG_CONFIG_CQUE_NONE | // Disable the comparator (default val)
                            ADS1015_REG_CONFIG_CLAT_NONLAT | // Non-latching (default val)
                            ADS1015_REG_CONFIG_CPOL_ACTVLOW | // Alert/Rdy active low   (default val)
                            ADS1015_REG_CONFIG_CMODE_TRAD | // Traditional comparator (default val)
                            ADS1015_REG_CONFIG_DR_1600SPS | // 1600 samples per second (default)
                            ADS1015_REG_CONFIG_MODE_SINGLE; // Single-shot mode (default)

            // Set PGA/voltage range
            config |= (UInt16)this.m_gain;

            // Set single-ended input channel
            switch (channel)
            {
                case (0):
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_0;
                    break;
                case (1):
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_1;
                    break;
                case (2):
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_2;
                    break;
                case (3):
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_3;
                    break;
            }

            // Set 'start single-conversion' bit
            config |= ADS1015_REG_CONFIG_OS_SINGLE;

            // Write config register to the ADC
            this.WriteRegister16Bits(ADS1015_REG_POINTER_CONFIG, config);

            // Wait for the conversion to complete
            this.SleepMilliSeconds(this.m_conversionDelay);

            // Read the conversion results
            var u16 = this.ReadRegister16Bits(ADS1015_REG_POINTER_CONVERT);

            //Ones' complement
            var signed = (u16 > 0x7fff);
            if (signed)
            {
                u16 = Convert.ToUInt16(~u16 & 0xffff);
            }

            // Shift 12-bit results right 4 bits for the ADS1015
            var s16 = Convert.ToInt16(u16 >> this.m_bitShift);

            //ptp - fixed the signed bit - one's complement
            return Convert.ToInt16((signed ? -1 : 1) * s16);
        }

        /**************************************************************************/
        /*! 
            @brief  Reads the conversion results, measuring the voltage
                    difference between the P (AIN0) and N (AIN1) input.  Generates
                    a signed value since the difference can be either
                    positive or negative.
        */
        /**************************************************************************/

        protected Int16 readADC_Differential_0_1()
        {
            // Start with default values
            UInt16 config = ADS1015_REG_CONFIG_CQUE_NONE | // Disable the comparator (default val)
                            ADS1015_REG_CONFIG_CLAT_NONLAT | // Non-latching (default val)
                            ADS1015_REG_CONFIG_CPOL_ACTVLOW | // Alert/Rdy active low   (default val)
                            ADS1015_REG_CONFIG_CMODE_TRAD | // Traditional comparator (default val)
                            ADS1015_REG_CONFIG_DR_1600SPS | // 1600 samples per second (default)
                            ADS1015_REG_CONFIG_MODE_SINGLE; // Single-shot mode (default)

            // Set PGA/voltage range
            config |= (UInt16)this.m_gain;

            // Set channels
            config |= ADS1015_REG_CONFIG_MUX_DIFF_0_1; // AIN0 = P, AIN1 = N

            // Set 'start single-conversion' bit
            config |= ADS1015_REG_CONFIG_OS_SINGLE;

            // Write config register to the ADC
            this.WriteRegister16Bits(ADS1015_REG_POINTER_CONFIG, config);

            // Wait for the conversion to complete
            this.SleepMilliSeconds(this.m_conversionDelay);

            // Read the conversion results
            UInt16 res = Convert.ToUInt16(this.ReadRegister16Bits(ADS1015_REG_POINTER_CONVERT) >> this.m_bitShift);
            if (this.m_bitShift == 0)
            {
                return (Int16)res;
            }
            else
            {
                // Shift 12-bit results right 4 bits for the ADS1015,
                // making sure we keep the sign bit intact
                if (res > 0x07FF)
                {
                    // negative number - extend the sign to 16th bit
                    res |= 0xF000;
                }
                return (Int16)res;
            }
        }

        /**************************************************************************/
        /*! 
            @brief  Reads the conversion results, measuring the voltage
                    difference between the P (AIN2) and N (AIN3) input.  Generates
                    a signed value since the difference can be either
                    positive or negative.
        */
        /**************************************************************************/

        protected Int16 readADC_Differential_2_3()
        {
            // Start with default values
            UInt16 config = ADS1015_REG_CONFIG_CQUE_NONE | // Disable the comparator (default val)
                            ADS1015_REG_CONFIG_CLAT_NONLAT | // Non-latching (default val)
                            ADS1015_REG_CONFIG_CPOL_ACTVLOW | // Alert/Rdy active low   (default val)
                            ADS1015_REG_CONFIG_CMODE_TRAD | // Traditional comparator (default val)
                            ADS1015_REG_CONFIG_DR_1600SPS | // 1600 samples per second (default)
                            ADS1015_REG_CONFIG_MODE_SINGLE; // Single-shot mode (default)

            // Set PGA/voltage range
            config |= (UInt16)this.m_gain;

            // Set channels
            config |= ADS1015_REG_CONFIG_MUX_DIFF_2_3; // AIN2 = P, AIN3 = N

            // Set 'start single-conversion' bit
            config |= ADS1015_REG_CONFIG_OS_SINGLE;

            // Write config register to the ADC
            this.WriteRegister16Bits(ADS1015_REG_POINTER_CONFIG, config);

            // Wait for the conversion to complete
            this.SleepMilliSeconds(this.m_conversionDelay);

            // Read the conversion results
            UInt16 res = Convert.ToUInt16(this.ReadRegister16Bits(ADS1015_REG_POINTER_CONVERT) >> this.m_bitShift);
            if (this.m_bitShift == 0)
            {
                return (Int16)res;
            }
            else
            {
                // Shift 12-bit results right 4 bits for the ADS1015,
                // making sure we keep the sign bit intact
                if (res > 0x07FF)
                {
                    // negative number - extend the sign to 16th bit
                    res |= 0xF000;
                }
                return (Int16)res;
            }
        }

        /**************************************************************************/
        /*!
            @brief  Sets up the comparator to operate in basic mode, causing the
                    ALERT/RDY pin to assert (go from high to low) when the ADC
                    value exceeds the specified threshold.
                    This will also set the ADC in continuous conversion mode.
        */
        /**************************************************************************/

        protected void startComparator_SingleEnded(byte channel, Int16 threshold)
        {
            // Start with default values
            UInt16 config = ADS1015_REG_CONFIG_CQUE_1CONV | // Comparator enabled and asserts on 1 match
                            ADS1015_REG_CONFIG_CLAT_LATCH | // Latching mode
                            ADS1015_REG_CONFIG_CPOL_ACTVLOW | // Alert/Rdy active low   (default val)
                            ADS1015_REG_CONFIG_CMODE_TRAD | // Traditional comparator (default val)
                            ADS1015_REG_CONFIG_DR_1600SPS | // 1600 samples per second (default)
                            ADS1015_REG_CONFIG_MODE_CONTIN;  // Continuous conversion mode

            // Set PGA/voltage range
            config |= (UInt16)this.m_gain;

            // Set single-ended input channel
            switch (channel)
            {
                case (0):
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_0;
                    break;
                case (1):
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_1;
                    break;
                case (2):
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_2;
                    break;
                case (3):
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_3;
                    break;
            }

            // Set the high threshold register
            // Shift 12-bit results left 4 bits for the ADS1015
            this.WriteRegister16Bits(ADS1015_REG_POINTER_HITHRESH, Convert.ToUInt16(threshold << this.m_bitShift));

            // Write config register to the ADC
            this.WriteRegister16Bits(ADS1015_REG_POINTER_CONFIG, config);
        }

        /**************************************************************************/
        /*!
            @brief  In order to clear the comparator, we need to read the
                    conversion results.  This function reads the last conversion
                    results without changing the config value.
        */
        /**************************************************************************/

        protected Int16 getLastConversionResults()
        {
            // Wait for the conversion to complete
            this.SleepMilliSeconds(this.m_conversionDelay);

            // Read the conversion results
            UInt16 res = Convert.ToUInt16(this.ReadRegister16Bits(ADS1015_REG_POINTER_CONVERT) >> this.m_bitShift);
            if (this.m_bitShift == 0)
            {
                return (Int16)res;
            }
            else
            {
                // Shift 12-bit results right 4 bits for the ADS1015,
                // making sure we keep the sign bit intact
                if (res > 0x07FF)
                {
                    // negative number - extend the sign to 16th bit
                    res |= 0xF000;
                }
                return (Int16)res;
            }
        }

        public double convertToVoltage(Int16 value)
        {
            double vref;

            switch (this.m_gain)
            {
                default:
                case adsGain_t.GAIN_TWOTHIRDS:
                    {
                        vref = 6.144;
                        break;
                    }
                case adsGain_t.GAIN_ONE:
                    {
                        vref = 4.096;
                        break;
                    }
                case adsGain_t.GAIN_TWO:
                    {
                        vref = 2.048;
                        break;
                    }
                case adsGain_t.GAIN_FOUR:
                    {
                        vref = 1.024;
                        break;
                    }
                case adsGain_t.GAIN_EIGHT:
                    {
                        vref = 0.512;
                        break;
                    }
                case adsGain_t.GAIN_SIXTEEN:
                    {
                        vref = 0.256;
                        break;
                    }
            }

            return (value * vref) / 32767;
        }

    }
}