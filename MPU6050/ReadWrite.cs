using System;

namespace MPU6050
{
    public class ReadWrite
    {
        private Mpu6050 _mpu6050;

        public ReadWrite(Mpu6050 mpu6050)
        {
            _mpu6050 = mpu6050;
        }

        public byte ReadByte(byte regAddr)
        {
            byte[] buffer = new byte[1];
            buffer[0] = regAddr;
            byte[] value = new byte[1];
            _mpu6050.Mpu6050Device.WriteRead(buffer, value);
            return value[0];
        }

        public byte[] ReadBytes(byte regAddr, int length)
        {
            byte[] values = new byte[length];
            byte[] buffer = new byte[1];
            buffer[0] = regAddr;
            _mpu6050.Mpu6050Device.WriteRead(buffer, values);
            return values;
        }

        public ushort ReadWord(byte address)
        {
            byte[] buffer = ReadBytes(Constants.FifoCount, 2);
            return (ushort)(((int)buffer[0] << 8) | (int)buffer[1]);
        }

        public void WriteByte(byte regAddr, byte data)
        {
            byte[] buffer = new byte[2];
            buffer[0] = regAddr;
            buffer[1] = data;
            _mpu6050.Mpu6050Device.Write(buffer);
        }

        private void writeBytes(byte regAddr, byte[] values)
        {
            byte[] buffer = new byte[1 + values.Length];
            buffer[0] = regAddr;
            Array.Copy(values, 0, buffer, 1, values.Length);
            _mpu6050.Mpu6050Device.Write(buffer);
        }
    }
}