namespace FT232HTest
{
    using System;
    using System.Runtime.InteropServices;

    public class Mpsse
    {
        public enum I2C_CLOCKRATE : uint
        {
            I2C_CLOCK_STANDARD_MODE = 100000,
            I2C_CLOCK_FAST_MODE = 400000,
            I2C_CLOCK_FAST_MODE_PLUS = 1000000,
            I2C_CLOCK_HIGH_SPEED_MODE = 3400000
        }

        [Flags]
        public enum I2C_TRANSFER_OPTIONS : uint
        {
            NONE = 0,

            /* Options to I2C_DeviceWrite & I2C_DeviceRead */
            /*Generate start condition before transmitting */
            I2C_TRANSFER_OPTIONS_START_BIT = 0x00000001,

            /*Generate stop condition before transmitting */
            I2C_TRANSFER_OPTIONS_STOP_BIT = 0x00000002,

            /*Continue transmitting data in bulk without caring about Ack or nAck from device if this bit is 
            not set. If this bit is set then stop transitting the data in the buffer when the device nAcks*/
            I2C_TRANSFER_OPTIONS_BREAK_ON_NACK = 0x00000004,

            /* libMPSSE-I2C generates an ACKs for every byte read. Some I2C slaves require the I2C 
            master to generate a nACK for the last data byte read. Setting this bit enables working with such 
            I2C slaves */
            I2C_TRANSFER_OPTIONS_NACK_LAST_BYTE = 0x00000008,

            /* no address phase, no USB interframe delays */
            I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES = 0x00000010,
            I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BITS = 0x00000020,
            I2C_TRANSFER_OPTIONS_FAST_TRANSFER = 0x00000030,

            /* if I2C_TRANSFER_OPTION_FAST_TRANSFER is set then setting this bit would mean that the 
            address field should be ignored. The address is either a part of the data or this is a special I2C
            frame that doesn't require an address*/
            I2C_TRANSFER_OPTIONS_NO_ADDRESS = 0x00000040
        }

        [Flags]
        public enum I2C_INIT_OPTIONS : uint
        {
            NONE = 0,

            /// <summary>
            /// Please note that 3-phase-clocking is available only on the hi-speed devices and not on the FT2232D.
            /// </summary>
            I2C_DISABLE_3PHASE_CLOCKING = 1,

            /// <summary>
            /// Enabling Drive-Only-Zero ensures that the SDA line is driven by the I2C master only when it is supposed to be driven LOW, and tristates it when it is supposed to be driven HIGH. 
            /// This feature is available only in FT232H chip. 
            /// Trying to enable this feature using function I2C_Init will have no effect on chips other than FT232H.
            /// </summary>
            I2C_ENABLE_DRIVE_ONLY_ZERO = 2
        }

        public struct FTC_INPUT_OUTPUT_PINS
        {
            public bool bPin1InputOutputState;
            public bool bPin1LowHighState;
            public bool bPin2InputOutputState;
            public bool bPin2LowHighState;
            public bool bPin3InputOutputState;
            public bool bPin3LowHighState;
            public bool bPin4InputOutputState;
            public bool bPin4LowHighState;
        }

        public struct FTH_INPUT_OUTPUT_PINS
        {
            public bool bPin1InputOutputState;
            public bool bPin1LowHighState;
            public bool bPin2InputOutputState;
            public bool bPin2LowHighState;
            public bool bPin3InputOutputState;
            public bool bPin3LowHighState;
            public bool bPin4InputOutputState;
            public bool bPin4LowHighState;
            public bool bPin5InputOutputState;
            public bool bPin5LowHighState;
            public bool bPin6InputOutputState;
            public bool bPin6LowHighState;
            public bool bPin7InputOutputState;
            public bool bPin7LowHighState;
            public bool bPin8InputOutputState;
            public bool bPin8LowHighState;
        }

        public struct FTC_LOW_HIGH_PINS
        {
            public bool bPin1LowHighState;
            public bool bPin2LowHighState;
            public bool bPin3LowHighState;
            public bool bPin4LowHighState;
        }

        public struct FTC_PAGE_WRITE_DATA
        {
            public uint NumPages;
            public uint NumBytesPerPage;
        }

        public struct FTH_LOW_HIGH_PINS
        {
            public bool bPin1LowHighState;
            public bool bPin2LowHighState;
            public bool bPin3LowHighState;
            public bool bPin4LowHighState;
            public bool bPin5LowHighState;
            public bool bPin6LowHighState;
            public bool bPin7LowHighState;
            public bool bPin8LowHighState;
        }

        public struct FTC_CLOSE_FINAL_STATE_PINS
        {
            public bool bTCKPinState;
            public bool bTCKPinActiveState;
            public bool bTDIPinState;
            public bool bTDIPinActiveState;
            public bool bTMSPinState;
            public bool bTMSPinActiveState;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ChannelCfg
        {
            public UInt32 ClockRate;
            public byte LatencyTimer;
            public UInt32 Options;
        }

        [DllImport("libMPSSE.dll", EntryPoint = "Init_libMPSSE", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Init_libMPSSE();

        [DllImport("libMPSSE.dll", EntryPoint = "Cleanup_libMPSSE", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Cleanup_libMPSSE();

        [DllImport("libMPSSE.dll", EntryPoint = "I2C_GetNumChannels", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint I2C_GetNumChannels(ref uint numChannels);

        [DllImport("libMPSSE.dll", EntryPoint = "I2C_OpenChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint I2C_OpenChannel(uint index, ref IntPtr handler);

        [DllImport("libMPSSE.dll", EntryPoint = "I2C_CloseChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint I2C_CloseChannel(IntPtr handler);

        [DllImport("libMPSSE.dll", EntryPoint = "I2C_InitChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint I2C_InitChannel(IntPtr handler, ref ChannelCfg config);

        [DllImport("libMPSSE.dll", EntryPoint = "I2C_DeviceRead", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint I2C_DeviceRead(IntPtr handler, UInt32 deviceAddress, UInt32 sizeToTransfer, byte[] buffer, ref UInt32 sizeTransfered, UInt32 options);

        [DllImport("libMPSSE.dll", EntryPoint = "I2C_DeviceWrite", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint I2C_DeviceWrite(IntPtr handler, UInt32 deviceAddress, UInt32 sizeToTransfer, byte[] buffer, ref UInt32 sizeTransfered, UInt32 options);

        [DllImport("libMPSSE.dll", EntryPoint = "FT_WriteGPIO", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint FT_WriteGPIO(IntPtr handler, byte dir, byte value);

        [DllImport("libMPSSE.dll", EntryPoint = "FT_ReadGPIO", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint FT_ReadGPIO(IntPtr handler, ref byte value);

    }
}