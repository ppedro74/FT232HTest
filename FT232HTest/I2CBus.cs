namespace FT232HTest
{
    using System;

    public interface I2CBus
    {
        uint ReadBytes(byte deviceAddress, UInt32 sizeToTransfer, byte[] buffer, ref UInt32 sizeTransfered, bool sendStart = true, bool sendStop = true, uint options = 0);
        uint WriteBytes(byte deviceAddress, UInt32 sizeToTransfer, byte[] buffer, ref UInt32 sizeTransfered, bool sendStart = true, bool sendStop = true, uint options = 0);

        uint WriteRegister8Bits(byte deviceAddress, byte registerAddress, byte value);
        uint WriteRegister16Bits(byte deviceAddress, byte registerAddress, UInt16 value);
        uint WriteRegister32Bits(byte deviceAddress, byte registerAddress, UInt32 value);

        uint ReadRegister8Bits(byte deviceAddress, byte registerAddress, out byte value);
        uint ReadRegister16Bits(byte deviceAddress, byte registerAddress, out UInt16 value);
        uint ReadRegister32Bits(byte deviceAddress, byte registerAddress, out UInt32 value);
        uint ReadRegisterBytes(byte deviceAddress, byte registerAddress, byte[] buffer);
    }
}