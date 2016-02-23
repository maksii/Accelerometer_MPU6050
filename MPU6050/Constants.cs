namespace MPU6050
{
    public class Constants
    {
        public const byte Address = 0x68;
        public const byte PwrMgmt1 = 0x6B;
        public const byte SmplrtDiv = 0x19;
        public const byte Config = 0x1A;
        public const byte GyroConfig = 0x1B;
        public const byte AccelConfig = 0x1C;
        public const byte FifoEn = 0x23;
        public const byte IntEnable = 0x38;
        public const byte IntStatus = 0x3A;
        public const byte UserCtrl = 0x6A;
        public const byte FifoCount = 0x72;
        public const byte FifoRW = 0x74;
        public const int SensorBytes = 12;
    }
}