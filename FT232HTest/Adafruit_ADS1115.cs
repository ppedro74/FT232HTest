namespace FT232HTest
{
    public class Adafruit_ADS1115 : Adafruit_ADS1015
    {
        /**************************************************************************/
        /*!
            @brief  Instantiates a new ADS1115 class w/appropriate properties
        */
        /**************************************************************************/

        public Adafruit_ADS1115(I2CBus i2c)
            : base(i2c)
        {
            this.m_conversionDelay = ADS1115_CONVERSIONDELAY;
            this.m_bitShift = 0;
            this.m_gain = adsGain_t.GAIN_TWOTHIRDS; /* +/- 6.144V range (limited to VDD +0.3V max!) */
        }

        public Adafruit_ADS1115(I2CBus i2c, byte address)
            : base(i2c, address)
        {
            this.m_conversionDelay = ADS1115_CONVERSIONDELAY;
            this.m_bitShift = 0;
            this.m_gain = adsGain_t.GAIN_TWOTHIRDS; /* +/- 6.144V range (limited to VDD +0.3V max!) */
        }
    }
}