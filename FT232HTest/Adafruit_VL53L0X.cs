//TODO: Output is not correct

namespace FT232HTest
{
    using System;
    using System.Diagnostics;

    public class Adafruit_VL53L0X : I2CDeviceBase
    {
        private const bool io_2v8 = true;
        private byte stop_variable; // read by init and used when starting measurement; is StopVariable field of VL53L0X_DevData_t structure in API
        private UInt32 measurement_timing_budget_us;
        private UInt16 io_timeout;
        private bool did_timeout;
        private UInt16 timeout_start_ms;
        private readonly Stopwatch sw = new Stopwatch();

        // reg addresses from API vl53l0x_device.h (ordered as listed there)
        private enum Register : byte
        {
            SYSRANGE_START = 0x00,

            SYSTEM_THRESH_HIGH = 0x0C,
            SYSTEM_THRESH_LOW = 0x0E,

            SYSTEM_SEQUENCE_CONFIG = 0x01,
            SYSTEM_RANGE_CONFIG = 0x09,
            SYSTEM_INTERMEASUREMENT_PERIOD = 0x04,

            SYSTEM_INTERRUPT_CONFIG_GPIO = 0x0A,

            GPIO_HV_MUX_ACTIVE_HIGH = 0x84,

            SYSTEM_INTERRUPT_CLEAR = 0x0B,

            RESULT_INTERRUPT_STATUS = 0x13,
            RESULT_RANGE_STATUS = 0x14,

            RESULT_CORE_AMBIENT_WINDOW_EVENTS_RTN = 0xBC,
            RESULT_CORE_RANGING_TOTAL_EVENTS_RTN = 0xC0,
            RESULT_CORE_AMBIENT_WINDOW_EVENTS_REF = 0xD0,
            RESULT_CORE_RANGING_TOTAL_EVENTS_REF = 0xD4,
            RESULT_PEAK_SIGNAL_RATE_REF = 0xB6,

            ALGO_PART_TO_PART_RANGE_OFFSET_MM = 0x28,

            I2C_SLAVE_DEVICE_ADDRESS = 0x8A,

            MSRC_CONFIG_CONTROL = 0x60,

            PRE_RANGE_CONFIG_MIN_SNR = 0x27,
            PRE_RANGE_CONFIG_VALID_PHASE_LOW = 0x56,
            PRE_RANGE_CONFIG_VALID_PHASE_HIGH = 0x57,
            PRE_RANGE_MIN_COUNT_RATE_RTN_LIMIT = 0x64,

            FINAL_RANGE_CONFIG_MIN_SNR = 0x67,
            FINAL_RANGE_CONFIG_VALID_PHASE_LOW = 0x47,
            FINAL_RANGE_CONFIG_VALID_PHASE_HIGH = 0x48,
            FINAL_RANGE_CONFIG_MIN_COUNT_RATE_RTN_LIMIT = 0x44,

            PRE_RANGE_CONFIG_SIGMA_THRESH_HI = 0x61,
            PRE_RANGE_CONFIG_SIGMA_THRESH_LO = 0x62,

            PRE_RANGE_CONFIG_VCSEL_PERIOD = 0x50,
            PRE_RANGE_CONFIG_TIMEOUT_MACROP_HI = 0x51,
            PRE_RANGE_CONFIG_TIMEOUT_MACROP_LO = 0x52,

            SYSTEM_HISTOGRAM_BIN = 0x81,
            HISTOGRAM_CONFIG_INITIAL_PHASE_SELECT = 0x33,
            HISTOGRAM_CONFIG_READOUT_CTRL = 0x55,

            FINAL_RANGE_CONFIG_VCSEL_PERIOD = 0x70,
            FINAL_RANGE_CONFIG_TIMEOUT_MACROP_HI = 0x71,
            FINAL_RANGE_CONFIG_TIMEOUT_MACROP_LO = 0x72,
            CROSSTALK_COMPENSATION_PEAK_RATE_MCPS = 0x20,

            MSRC_CONFIG_TIMEOUT_MACROP = 0x46,

            SOFT_RESET_GO2_SOFT_RESET_N = 0xBF,
            IDENTIFICATION_MODEL_ID = 0xC0,
            IDENTIFICATION_REVISION_ID = 0xC2,

            OSC_CALIBRATE_VAL = 0xF8,

            GLOBAL_CONFIG_VCSEL_WIDTH = 0x32,
            GLOBAL_CONFIG_SPAD_ENABLES_REF_0 = 0xB0,
            GLOBAL_CONFIG_SPAD_ENABLES_REF_1 = 0xB1,
            GLOBAL_CONFIG_SPAD_ENABLES_REF_2 = 0xB2,
            GLOBAL_CONFIG_SPAD_ENABLES_REF_3 = 0xB3,
            GLOBAL_CONFIG_SPAD_ENABLES_REF_4 = 0xB4,
            GLOBAL_CONFIG_SPAD_ENABLES_REF_5 = 0xB5,

            GLOBAL_CONFIG_REF_EN_START_SELECT = 0xB6,
            DYNAMIC_SPAD_NUM_REQUESTED_REF_SPAD = 0x4E,
            DYNAMIC_SPAD_REF_EN_START_OFFSET = 0x4F,
            POWER_MANAGEMENT_GO1_POWER_FORCE = 0x80,

            VHV_CONFIG_PAD_SCL_SDA__EXTSUP_HV = 0x89,

            ALGO_PHASECAL_LIM = 0x30,
            ALGO_PHASECAL_CONFIG_TIMEOUT = 0x30,
        };

        private enum VcselPeriodType
        {
            VcselPeriodPreRange,
            VcselPeriodFinalRange
        };

        private struct SequenceStepEnables
        {
            public bool tcc, msrc, dss, pre_range, final_range;
        };

        private struct SequenceStepTimeouts
        {
            public UInt16 pre_range_vcsel_period_pclks, final_range_vcsel_period_pclks;
            public UInt16 msrc_dss_tcc_mclks, pre_range_mclks, final_range_mclks;
            public UInt32 msrc_dss_tcc_us, pre_range_us, final_range_us;
        };

        public Adafruit_VL53L0X(I2CBus bus)
            : base(bus, 0x29)
        {
        }

        public override bool Initialize()
        {
            this.Debug("VL53L0X Initializing");

            this.sw.Start();

            ////sensor uses 1V8 mode for I/O by default; switch to 2V8 mode
            //byte b;
            //var result = i2c.ReadRegister8Bits(this.Address, (byte)Register.VHV_CONFIG_PAD_SCL_SDA__EXTSUP_HV, out b);
            //result = i2c.WriteRegister8Bits(this.Address, (byte)Register.VHV_CONFIG_PAD_SCL_SDA__EXTSUP_HV, Convert.ToByte(b | 0x1));

            // VL53L0X_DataInit() begin

            // sensor uses 1V8 mode for I/O by default; switch to 2V8 mode if necessary
            if (io_2v8)
            {
                this.WriteRegister8Bits((byte)Register.VHV_CONFIG_PAD_SCL_SDA__EXTSUP_HV, Convert.ToByte(this.ReadRegister8Bits((byte)Register.VHV_CONFIG_PAD_SCL_SDA__EXTSUP_HV) | 0x01)); // set bit 0
            }

            // "Set I2C standard mode"
            this.WriteRegister8Bits(0x88, 0x00);

            this.WriteRegister8Bits(0x80, 0x01);
            this.WriteRegister8Bits(0xFF, 0x01);
            this.WriteRegister8Bits(0x00, 0x00);
            this.stop_variable = this.ReadRegister8Bits(0x91);
            this.WriteRegister8Bits(0x00, 0x01);
            this.WriteRegister8Bits(0xFF, 0x00);
            this.WriteRegister8Bits(0x80, 0x00);

            // disable SIGNAL_RATE_MSRC (bit 1) and SIGNAL_RATE_PRE_RANGE (bit 4) limit checks
            this.WriteRegister8Bits((byte)Register.MSRC_CONFIG_CONTROL, Convert.ToByte(this.ReadRegister8Bits((byte)Register.MSRC_CONFIG_CONTROL) | 0x12));

            // set final range signal rate limit to 0.25 MCPS (million counts per second)
            this.setSignalRateLimit(0.25f);

            this.WriteRegister8Bits((byte)Register.SYSTEM_SEQUENCE_CONFIG, 0xFF);

            // VL53L0X_DataInit() end

            // VL53L0X_StaticInit() begin

            byte spad_count = 0;
            bool spad_type_is_aperture = false;
            if (!this.getSpadInfo(ref spad_count, ref spad_type_is_aperture))
            {
                return false;
            }

            // The SPAD map (RefGoodSpadMap) is read by VL53L0X_get_info_from_device() in
            // the API, but the same data seems to be more easily readable from
            // GLOBAL_CONFIG_SPAD_ENABLES_REF_0 through _6, so read it from there
            var ref_spad_map = new byte[6];

            this.readMulti((byte)Register.GLOBAL_CONFIG_SPAD_ENABLES_REF_0, ref_spad_map, 6);

            // -- VL53L0X_set_reference_spads() begin (assume NVM values are valid)

            this.WriteRegister8Bits(0xFF, 0x01);
            this.WriteRegister8Bits((byte)Register.DYNAMIC_SPAD_REF_EN_START_OFFSET, 0x00);
            this.WriteRegister8Bits((byte)Register.DYNAMIC_SPAD_NUM_REQUESTED_REF_SPAD, 0x2C);
            this.WriteRegister8Bits(0xFF, 0x00);
            this.WriteRegister8Bits((byte)Register.GLOBAL_CONFIG_REF_EN_START_SELECT, 0xB4);

            byte first_spad_to_enable = Convert.ToByte(spad_type_is_aperture ? 12 : 0); // 12 is the first aperture spad
            byte spads_enabled = 0;

            for (byte i = 0; i < 48; i++)
            {
                if (i < first_spad_to_enable || spads_enabled == spad_count)
                {
                    // This bit is lower than the first one that should be enabled, or
                    // (reference_spad_count) bits have already been enabled, so zero this bit
                    ref_spad_map[i / 8] &= Convert.ToByte((~Convert.ToByte(1 << (i % 8))) & 0xff);
                }
                else if (((ref_spad_map[i / 8] >> (i % 8)) & 0x1) == 0x01)
                {
                    spads_enabled++;
                }
            }

            this.writeMulti((byte)Register.GLOBAL_CONFIG_SPAD_ENABLES_REF_0, ref_spad_map, 6);

            // -- VL53L0X_set_reference_spads() end

            // -- VL53L0X_load_tuning_settings() begin
            // DefaultTuningSettings from vl53l0x_tuning.h

            this.WriteRegister8Bits(0xFF, 0x01);
            this.WriteRegister8Bits(0x00, 0x00);

            this.WriteRegister8Bits(0xFF, 0x00);
            this.WriteRegister8Bits(0x09, 0x00);
            this.WriteRegister8Bits(0x10, 0x00);
            this.WriteRegister8Bits(0x11, 0x00);

            this.WriteRegister8Bits(0x24, 0x01);
            this.WriteRegister8Bits(0x25, 0xFF);
            this.WriteRegister8Bits(0x75, 0x00);

            this.WriteRegister8Bits(0xFF, 0x01);
            this.WriteRegister8Bits(0x4E, 0x2C);
            this.WriteRegister8Bits(0x48, 0x00);
            this.WriteRegister8Bits(0x30, 0x20);

            this.WriteRegister8Bits(0xFF, 0x00);
            this.WriteRegister8Bits(0x30, 0x09);
            this.WriteRegister8Bits(0x54, 0x00);
            this.WriteRegister8Bits(0x31, 0x04);
            this.WriteRegister8Bits(0x32, 0x03);
            this.WriteRegister8Bits(0x40, 0x83);
            this.WriteRegister8Bits(0x46, 0x25);
            this.WriteRegister8Bits(0x60, 0x00);
            this.WriteRegister8Bits(0x27, 0x00);
            this.WriteRegister8Bits(0x50, 0x06);
            this.WriteRegister8Bits(0x51, 0x00);
            this.WriteRegister8Bits(0x52, 0x96);
            this.WriteRegister8Bits(0x56, 0x08);
            this.WriteRegister8Bits(0x57, 0x30);
            this.WriteRegister8Bits(0x61, 0x00);
            this.WriteRegister8Bits(0x62, 0x00);
            this.WriteRegister8Bits(0x64, 0x00);
            this.WriteRegister8Bits(0x65, 0x00);
            this.WriteRegister8Bits(0x66, 0xA0);

            this.WriteRegister8Bits(0xFF, 0x01);
            this.WriteRegister8Bits(0x22, 0x32);
            this.WriteRegister8Bits(0x47, 0x14);
            this.WriteRegister8Bits(0x49, 0xFF);
            this.WriteRegister8Bits(0x4A, 0x00);

            this.WriteRegister8Bits(0xFF, 0x00);
            this.WriteRegister8Bits(0x7A, 0x0A);
            this.WriteRegister8Bits(0x7B, 0x00);
            this.WriteRegister8Bits(0x78, 0x21);

            this.WriteRegister8Bits(0xFF, 0x01);
            this.WriteRegister8Bits(0x23, 0x34);
            this.WriteRegister8Bits(0x42, 0x00);
            this.WriteRegister8Bits(0x44, 0xFF);
            this.WriteRegister8Bits(0x45, 0x26);
            this.WriteRegister8Bits(0x46, 0x05);
            this.WriteRegister8Bits(0x40, 0x40);
            this.WriteRegister8Bits(0x0E, 0x06);
            this.WriteRegister8Bits(0x20, 0x1A);
            this.WriteRegister8Bits(0x43, 0x40);

            this.WriteRegister8Bits(0xFF, 0x00);
            this.WriteRegister8Bits(0x34, 0x03);
            this.WriteRegister8Bits(0x35, 0x44);

            this.WriteRegister8Bits(0xFF, 0x01);
            this.WriteRegister8Bits(0x31, 0x04);
            this.WriteRegister8Bits(0x4B, 0x09);
            this.WriteRegister8Bits(0x4C, 0x05);
            this.WriteRegister8Bits(0x4D, 0x04);

            this.WriteRegister8Bits(0xFF, 0x00);
            this.WriteRegister8Bits(0x44, 0x00);
            this.WriteRegister8Bits(0x45, 0x20);
            this.WriteRegister8Bits(0x47, 0x08);
            this.WriteRegister8Bits(0x48, 0x28);
            this.WriteRegister8Bits(0x67, 0x00);
            this.WriteRegister8Bits(0x70, 0x04);
            this.WriteRegister8Bits(0x71, 0x01);
            this.WriteRegister8Bits(0x72, 0xFE);
            this.WriteRegister8Bits(0x76, 0x00);
            this.WriteRegister8Bits(0x77, 0x00);

            this.WriteRegister8Bits(0xFF, 0x01);
            this.WriteRegister8Bits(0x0D, 0x01);

            this.WriteRegister8Bits(0xFF, 0x00);
            this.WriteRegister8Bits(0x80, 0x01);
            this.WriteRegister8Bits(0x01, 0xF8);

            this.WriteRegister8Bits(0xFF, 0x01);
            this.WriteRegister8Bits(0x8E, 0x01);
            this.WriteRegister8Bits(0x00, 0x01);
            this.WriteRegister8Bits(0xFF, 0x00);
            this.WriteRegister8Bits(0x80, 0x00);

            // -- VL53L0X_load_tuning_settings() end

            // "Set interrupt config to new sample ready"
            // -- VL53L0X_SetGpioConfig() begin

            this.WriteRegister8Bits((byte)Register.SYSTEM_INTERRUPT_CONFIG_GPIO, 0x04);
            this.WriteRegister8Bits((byte)Register.GPIO_HV_MUX_ACTIVE_HIGH, Convert.ToByte(this.ReadRegister8Bits((byte)Register.GPIO_HV_MUX_ACTIVE_HIGH) & ~0x10 & 0xff)); // active low
            this.WriteRegister8Bits((byte)Register.SYSTEM_INTERRUPT_CLEAR, 0x01);

            // -- VL53L0X_SetGpioConfig() end

            this.measurement_timing_budget_us = this.getMeasurementTimingBudget();

            // "Disable MSRC and TCC by default"
            // MSRC = Minimum Signal Rate Check
            // TCC = Target CentreCheck
            // -- VL53L0X_SetSequenceStepEnable() begin

            this.WriteRegister8Bits((byte)Register.SYSTEM_SEQUENCE_CONFIG, 0xE8);

            // -- VL53L0X_SetSequenceStepEnable() end

            // "Recalculate timing budget"
            this.setMeasurementTimingBudget(this.measurement_timing_budget_us);

            // VL53L0X_StaticInit() end

            // VL53L0X_PerformRefCalibration() begin (VL53L0X_perform_ref_calibration())

            // -- VL53L0X_perform_vhv_calibration() begin

            this.WriteRegister8Bits((byte)Register.SYSTEM_SEQUENCE_CONFIG, 0x01);
            if (!this.performSingleRefCalibration(0x40))
            {
                return false;
            }

            // -- VL53L0X_perform_vhv_calibration() end

            // -- VL53L0X_perform_phase_calibration() begin

            this.WriteRegister8Bits((byte)Register.SYSTEM_SEQUENCE_CONFIG, 0x02);
            if (!this.performSingleRefCalibration(0x00))
            {
                return false;
            }

            // -- VL53L0X_perform_phase_calibration() end

            // "restore the previous Sequence Config"
            this.WriteRegister8Bits((byte)Register.SYSTEM_SEQUENCE_CONFIG, 0xE8);

            // VL53L0X_PerformRefCalibration() end

            this.Debug("VL53L0X Initialized");

            return true;
        }


        // Returns a range reading in millimeters when continuous mode is active
        // (readRangeSingleMillimeters() also calls this function after starting a
        // single-shot range measurement)
        private UInt16 readRangeContinuousMillimeters()
        {
            this.startTimeout();
            while ((this.ReadRegister8Bits((byte)Register.RESULT_INTERRUPT_STATUS) & 0x07) == 0)
            {
                if (this.checkTimeoutExpired())
                {
                    this.did_timeout = true;
                    return 65535;
                }
            }

            // assumptions: Linearity Corrective Gain is 1000 (default);
            // fractional ranging is not enabled
            UInt16 range = this.ReadRegister16Bits((byte)Register.RESULT_RANGE_STATUS + 10);

            this.WriteRegister8Bits((byte)Register.SYSTEM_INTERRUPT_CLEAR, 0x01);

            return range;
        }


        // Performs a single-shot range measurement and returns the reading in
        // millimeters
        // based on VL53L0X_PerformSingleRangingMeasurement()
        public UInt16 readRangeSingleMillimeters()
        {
            this.WriteRegister8Bits(0x80, 0x01);
            this.WriteRegister8Bits(0xFF, 0x01);
            this.WriteRegister8Bits(0x00, 0x00);
            this.WriteRegister8Bits(0x91, this.stop_variable);
            this.WriteRegister8Bits(0x00, 0x01);
            this.WriteRegister8Bits(0xFF, 0x00);
            this.WriteRegister8Bits(0x80, 0x00);

            this.WriteRegister8Bits((byte)Register.SYSRANGE_START, 0x01);

            // "Wait until start bit has been cleared"
            this.startTimeout();
            while (Convert.ToBoolean(this.ReadRegister8Bits((byte)Register.SYSRANGE_START) & 0x01))
            {
                if (this.checkTimeoutExpired())
                {
                    this.did_timeout = true;
                    return 65535;
                }
            }

            return this.readRangeContinuousMillimeters();
        }


        // Set the measurement timing budget in microseconds, which is the time allowed
        // for one measurement; the ST API and this library take care of splitting the
        // timing budget among the sub-steps in the ranging sequence. A longer timing
        // budget allows for more accurate measurements. Increasing the budget by a
        // factor of N decreases the range measurement standard deviation by a factor of
        // sqrt(N). Defaults to about 33 milliseconds; the minimum is 20 ms.
        // based on VL53L0X_set_measurement_timing_budget_micro_seconds()
        private bool setMeasurementTimingBudget(UInt32 budget_us)
        {
            SequenceStepEnables enables;
            SequenceStepTimeouts timeouts;

            const UInt16 StartOverhead = 1320; // note that this is different than the value in get_
            const UInt16 EndOverhead = 960;
            const UInt16 MsrcOverhead = 660;
            const UInt16 TccOverhead = 590;
            const UInt16 DssOverhead = 690;
            const UInt16 PreRangeOverhead = 660;
            const UInt16 FinalRangeOverhead = 550;

            const UInt32 MinTimingBudget = 20000;

            if (budget_us < MinTimingBudget)
            {
                return false;
            }

            UInt32 used_budget_us = StartOverhead + EndOverhead;

            this.getSequenceStepEnables(out enables);
            this.getSequenceStepTimeouts(ref enables, out timeouts);

            if (enables.tcc)
            {
                used_budget_us += (timeouts.msrc_dss_tcc_us + TccOverhead);
            }

            if (enables.dss)
            {
                used_budget_us += 2 * (timeouts.msrc_dss_tcc_us + DssOverhead);
            }
            else if (enables.msrc)
            {
                used_budget_us += (timeouts.msrc_dss_tcc_us + MsrcOverhead);
            }

            if (enables.pre_range)
            {
                used_budget_us += (timeouts.pre_range_us + PreRangeOverhead);
            }

            if (enables.final_range)
            {
                used_budget_us += FinalRangeOverhead;

                // "Note that the final range timeout is determined by the timing
                // budget and the sum of all other timeouts within the sequence.
                // If there is no room for the final range timeout, then an error
                // will be set. Otherwise the remaining time will be applied to
                // the final range."

                if (used_budget_us > budget_us)
                {
                    // "Requested timeout too big."
                    return false;
                }

                UInt32 final_range_timeout_us = budget_us - used_budget_us;

                // set_sequence_step_timeout() begin
                // (SequenceStepId == VL53L0X_SEQUENCESTEP_FINAL_RANGE)

                // "For the final range timeout, the pre-range timeout
                //  must be added. To do this both final and pre-range
                //  timeouts must be expressed in macro periods MClks
                //  because they have different vcsel periods."

                UInt16 final_range_timeout_mclks = Convert.ToUInt16(this.timeoutMicrosecondsToMclks(final_range_timeout_us, Convert.ToByte(timeouts.final_range_vcsel_period_pclks)));

                if (enables.pre_range)
                {
                    final_range_timeout_mclks += timeouts.pre_range_mclks;
                }

                this.WriteRegister16Bits((byte)Register.FINAL_RANGE_CONFIG_TIMEOUT_MACROP_HI, this.encodeTimeout(final_range_timeout_mclks));

                // set_sequence_step_timeout() end

                this.measurement_timing_budget_us = budget_us; // store for internal reuse
            }
            return true;
        }

        // Get the measurement timing budget in microseconds
        // based on VL53L0X_get_measurement_timing_budget_micro_seconds()
        // in us
        private UInt32 getMeasurementTimingBudget()
        {
            SequenceStepEnables enables;
            SequenceStepTimeouts timeouts;

            const UInt16 StartOverhead = 1910; // note that this is different than the value in set_
            const UInt16 EndOverhead = 960;
            const UInt16 MsrcOverhead = 660;
            const UInt16 TccOverhead = 590;
            const UInt16 DssOverhead = 690;
            const UInt16 PreRangeOverhead = 660;
            const UInt16 FinalRangeOverhead = 550;

            // "Start and end overhead times always present"
            UInt32 budget_us = StartOverhead + EndOverhead;

            this.getSequenceStepEnables(out enables);
            this.getSequenceStepTimeouts(ref enables, out timeouts);

            if (enables.tcc)
            {
                budget_us += (timeouts.msrc_dss_tcc_us + TccOverhead);
            }

            if (enables.dss)
            {
                budget_us += 2 * (timeouts.msrc_dss_tcc_us + DssOverhead);
            }
            else if (enables.msrc)
            {
                budget_us += (timeouts.msrc_dss_tcc_us + MsrcOverhead);
            }

            if (enables.pre_range)
            {
                budget_us += (timeouts.pre_range_us + PreRangeOverhead);
            }

            if (enables.final_range)
            {
                budget_us += (timeouts.final_range_us + FinalRangeOverhead);
            }

            this.measurement_timing_budget_us = budget_us; // store for internal reuse
            return budget_us;
        }

        // Get sequence step timeouts
        // based on get_sequence_step_timeout(),
        // but gets all timeouts instead of just the requested one, and also stores
        // intermediate values
        private void getSequenceStepTimeouts(ref SequenceStepEnables enables, out SequenceStepTimeouts timeouts)
        {
            timeouts.pre_range_vcsel_period_pclks = this.getVcselPulsePeriod(VcselPeriodType.VcselPeriodPreRange);

            timeouts.msrc_dss_tcc_mclks = Convert.ToUInt16(this.ReadRegister8Bits((byte)Register.MSRC_CONFIG_TIMEOUT_MACROP) + 1);
            timeouts.msrc_dss_tcc_us = this.timeoutMclksToMicroseconds(timeouts.msrc_dss_tcc_mclks, Convert.ToByte(timeouts.pre_range_vcsel_period_pclks));
            timeouts.pre_range_mclks = this.decodeTimeout(this.ReadRegister16Bits((byte)Register.PRE_RANGE_CONFIG_TIMEOUT_MACROP_HI));
            timeouts.pre_range_us = this.timeoutMclksToMicroseconds(timeouts.pre_range_mclks, Convert.ToByte(timeouts.pre_range_vcsel_period_pclks));
            timeouts.final_range_vcsel_period_pclks = this.getVcselPulsePeriod(VcselPeriodType.VcselPeriodFinalRange);
            timeouts.final_range_mclks = this.decodeTimeout(this.ReadRegister16Bits((byte)Register.FINAL_RANGE_CONFIG_TIMEOUT_MACROP_HI));
            if (enables.pre_range)
            {
                timeouts.final_range_mclks -= timeouts.pre_range_mclks;
            }
            timeouts.final_range_us = this.timeoutMclksToMicroseconds(timeouts.final_range_mclks, Convert.ToByte(timeouts.final_range_vcsel_period_pclks));
        }


        // Get the VCSEL pulse period in PCLKs for the given period type.
        // based on VL53L0X_get_vcsel_pulse_period()
        private byte getVcselPulsePeriod(VcselPeriodType type)
        {
            if (type == VcselPeriodType.VcselPeriodPreRange)
            {
                return this.decodeVcselPeriod(this.ReadRegister8Bits((byte)Register.PRE_RANGE_CONFIG_VCSEL_PERIOD));
            }
            else if (type == VcselPeriodType.VcselPeriodFinalRange)
            {
                return this.decodeVcselPeriod(this.ReadRegister8Bits((byte)Register.FINAL_RANGE_CONFIG_VCSEL_PERIOD));
            }
            else
            {
                return 255;
            }
        }


        // Get reference SPAD (single photon avalanche diode) count and type
        // based on VL53L0X_get_info_from_device(),
        // but only gets reference SPAD count and type
        private bool getSpadInfo(ref byte count, ref bool type_is_aperture)
        {
            byte tmp;

            this.WriteRegister8Bits(0x80, 0x01);
            this.WriteRegister8Bits(0xFF, 0x01);
            this.WriteRegister8Bits(0x00, 0x00);

            this.WriteRegister8Bits(0xFF, 0x06);
            this.WriteRegister8Bits(0x83, Convert.ToByte(this.ReadRegister8Bits(0x83) | 0x04));
            this.WriteRegister8Bits(0xFF, 0x07);
            this.WriteRegister8Bits(0x81, 0x01);

            this.WriteRegister8Bits(0x80, 0x01);

            this.WriteRegister8Bits(0x94, 0x6b);
            this.WriteRegister8Bits(0x83, 0x00);
            this.startTimeout();
            while (this.ReadRegister8Bits(0x83) == 0x00)
            {
                if (this.checkTimeoutExpired())
                {
                    return false;
                }
            }
            this.WriteRegister8Bits(0x83, 0x01);
            tmp = this.ReadRegister8Bits(0x92);

            count = Convert.ToByte(tmp & 0x7f);
            type_is_aperture = (((tmp >> 7) & 0x01) == 0x01);

            this.WriteRegister8Bits(0x81, 0x00);
            this.WriteRegister8Bits(0xFF, 0x06);
            this.WriteRegister8Bits(0x83, this.ReadRegister8Bits(0x83 & ~0x04));
            this.WriteRegister8Bits(0xFF, 0x01);
            this.WriteRegister8Bits(0x00, 0x01);

            this.WriteRegister8Bits(0xFF, 0x00);
            this.WriteRegister8Bits(0x80, 0x00);

            return true;
        }

        // Set the return signal rate limit check value in units of MCPS (mega counts
        // per second). "This represents the amplitude of the signal reflected from the
        // target and detected by the device"; setting this limit presumably determines
        // the minimum measurement necessary for the sensor to report a valid reading.
        // Setting a lower limit increases the potential range of the sensor but also
        // seems to increase the likelihood of getting an inaccurate reading because of
        // unwanted reflections from objects other than the intended target.
        // Defaults to 0.25 MCPS as initialized by the ST API and this library.
        private bool setSignalRateLimit(float limit_Mcps)
        {
            if (limit_Mcps < 0 || limit_Mcps > 511.99)
            {
                return false;
            }

            // Q9.7 fixed point format (9 integer bits, 7 fractional bits)
            this.WriteRegister16Bits((byte)Register.FINAL_RANGE_CONFIG_MIN_COUNT_RATE_RTN_LIMIT, Convert.ToUInt16(limit_Mcps * (1 << 7)));
            return true;
        }

        // Get sequence step enables
        // based on VL53L0X_GetSequenceStepEnables()
        private void getSequenceStepEnables(out SequenceStepEnables enables)
        {
            byte sequence_config = this.ReadRegister8Bits((byte)Register.SYSTEM_SEQUENCE_CONFIG);

            enables.tcc = Convert.ToBoolean((sequence_config >> 4) & 0x1);
            enables.dss = Convert.ToBoolean((sequence_config >> 3) & 0x1);
            enables.msrc = Convert.ToBoolean((sequence_config >> 2) & 0x1);
            enables.pre_range = Convert.ToBoolean((sequence_config >> 6) & 0x1);
            enables.final_range = Convert.ToBoolean((sequence_config >> 7) & 0x1);
        }

        // based on VL53L0X_perform_single_ref_calibration()
        private bool performSingleRefCalibration(byte vhv_init_byte)
        {
            this.WriteRegister8Bits((byte)Register.SYSRANGE_START, Convert.ToByte(0x01 | vhv_init_byte)); // VL53L0X_REG_SYSRANGE_MODE_START_STOP

            this.startTimeout();
            while ((this.ReadRegister8Bits((byte)Register.RESULT_INTERRUPT_STATUS) & 0x07) == 0)
            {
                if (this.checkTimeoutExpired())
                {
                    return false;
                }
            }

            this.WriteRegister8Bits((byte)Register.SYSTEM_INTERRUPT_CLEAR, 0x01);

            this.WriteRegister8Bits((byte)Register.SYSRANGE_START, 0x00);

            return true;
        }

        // Decode sequence step timeout in MCLKs from register value
        // based on VL53L0X_decode_timeout()
        // Note: the original function returned a uint32_t, but the return value is
        // always stored in a uint16_t.
        private UInt16 decodeTimeout(UInt16 reg_val)
        {
            // format: "(LSByte * 2^MSByte) + 1"
            return Convert.ToUInt16((UInt16)((reg_val & 0x00FF) << (UInt16)((reg_val & 0xFF00) >> 8)) + 1);
        }

        // Encode sequence step timeout register value from timeout in MCLKs
        // based on VL53L0X_encode_timeout()
        // Note: the original function took a uint16_t, but the argument passed to it
        // is always a uint16_t.
        private UInt16 encodeTimeout(UInt16 timeout_mclks)
        {
            // format: "(LSByte * 2^MSByte) + 1"
            UInt32 ls_byte = 0;
            UInt16 ms_byte = 0;

            if (timeout_mclks > 0)
            {
                ls_byte = Convert.ToUInt32(timeout_mclks - 1);

                while ((ls_byte & 0xFFFFFF00) > 0)
                {
                    ls_byte >>= 1;
                    ms_byte++;
                }

                return Convert.ToUInt16((ms_byte << 8) | (ls_byte & 0xFF));
            }
            else
            {
                return 0;
            }
        }

        // Convert sequence step timeout from MCLKs to microseconds with given VCSEL period in PCLKs
        // based on VL53L0X_calc_timeout_us()
        private UInt32 timeoutMclksToMicroseconds(UInt16 timeout_period_mclks, byte vcsel_period_pclks)
        {
            UInt32 macro_period_ns = this.calcMacroPeriod(vcsel_period_pclks);
            return ((timeout_period_mclks * macro_period_ns) + (macro_period_ns / 2)) / 1000;
        }

        // Convert sequence step timeout from microseconds to MCLKs with given VCSEL period in PCLKs
        // based on VL53L0X_calc_timeout_mclks()
        private UInt32 timeoutMicrosecondsToMclks(UInt32 timeout_period_us, byte vcsel_period_pclks)
        {
            UInt32 macro_period_ns = this.calcMacroPeriod(vcsel_period_pclks);
            return (((timeout_period_us * 1000) + (macro_period_ns / 2)) / macro_period_ns);
        }

        // Record the current time to check an upcoming timeout against
        private void startTimeout()
        {
            this.timeout_start_ms = this.millis();
        }

        // Check if timeout is enabled (set to nonzero value) and has expired
        private bool checkTimeoutExpired()
        {
            return this.io_timeout > 0 && ((UInt16)this.millis() - this.timeout_start_ms) > this.io_timeout;
        }

        // Decode VCSEL (vertical cavity surface emitting laser) pulse period in PCLKs
        // from register value
        // based on VL53L0X_decode_vcsel_period()
        private byte decodeVcselPeriod(byte reg_val)
        {
            return Convert.ToByte(((reg_val) + 1) << 1);
        }

        // Encode VCSEL pulse period register value from period in PCLKs
        // based on VL53L0X_encode_vcsel_period()
        private byte encodeVcselPeriod(byte period_pclks)
        {
            return Convert.ToByte(((period_pclks) >> 1) - 1);
        }

        // Calculate macro period in *nanoseconds* from VCSEL period in PCLKs
        // based on VL53L0X_calc_macro_period_ps()
        // PLL_period_ps = 1655; macro_period_vclks = 2304
        private UInt32 calcMacroPeriod(byte vcsel_period_pclks)
        {
            return ((((UInt32)2304 * (vcsel_period_pclks) * 1655) + 500) / 1000);
        }


        private uint writeMulti(byte reg, byte[] src, byte count)
        {
            var buffer = new byte[count + 1];
            buffer[1] = reg;
            Buffer.BlockCopy(src, 0, buffer, 1, count);

            uint sizeTransfered = 0;
            var result = this.i2c.WriteBytes(this.Address, (uint)buffer.Length, buffer, ref sizeTransfered);
            if (result != 0)
            {
            }

            return result;
        }

        private uint readMulti(byte reg, byte[] src, byte count)
        {
            uint sizeTransfered1 = 0;
            var result1 = this.i2c.WriteBytes(this.Address, 1, new[] { reg }, ref sizeTransfered1);
            if (result1 != 0)
            {
            }

            uint sizeTransfered2 = 0;
            var result2 = this.i2c.ReadBytes(this.Address, count, src, ref sizeTransfered2);
            if (result2 != 0)
            {
            }

            return result2;
        }


        private UInt16 millis()
        {
            return Convert.ToUInt16(this.sw.ElapsedMilliseconds & 0xffff);
        }
    }
}