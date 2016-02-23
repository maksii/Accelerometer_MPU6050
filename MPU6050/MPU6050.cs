using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace MPU6050
{
    public class Mpu6050 : IDisposable
    {
        public event EventHandler<MpuSensorEventArgs> SensorInterruptEvent;

        private const int InterruptPin = 18;
        public I2cDevice Mpu6050Device;
        private GpioController _ioController;
        private GpioPin _interruptPin;

        public ReadWrite ReadWrite { get; }

        public async void InitHardware()
        {
            try
            {
                _ioController = GpioController.GetDefault();
                _interruptPin = _ioController.OpenPin(InterruptPin);
                _interruptPin.Write(GpioPinValue.Low);
                _interruptPin.SetDriveMode(GpioPinDriveMode.Input);
                _interruptPin.ValueChanged += Interrupt;

                var aqs = I2cDevice.GetDeviceSelector();
                var collection = await DeviceInformation.FindAllAsync(aqs);

                var settings = new I2cConnectionSettings(Constants.Address)
                {
                    BusSpeed = I2cBusSpeed.FastMode,
                    SharingMode = I2cSharingMode.Exclusive
                };
                Mpu6050Device = await I2cDevice.FromIdAsync(collection[0].Id, settings);

                await Task.Delay(3); // wait power up sequence

                ReadWrite.WriteByte(Constants.PwrMgmt1, 0x80); // reset the device
                await Task.Delay(100);
                ReadWrite.WriteByte(Constants.PwrMgmt1, 0x2);
                ReadWrite.WriteByte(Constants.UserCtrl, 0x04); //reset fifo

                ReadWrite.WriteByte(Constants.PwrMgmt1, 1); // clock source = gyro x
                ReadWrite.WriteByte(Constants.GyroConfig, 0); // +/- 250 degrees sec
                ReadWrite.WriteByte(Constants.AccelConfig, 0); // +/- 2g

                ReadWrite.WriteByte(Constants.Config, 1); // 184 Hz, 2ms delay
                ReadWrite.WriteByte(Constants.SmplrtDiv, 19); // set rate 50Hz
                ReadWrite.WriteByte(Constants.FifoEn, 0x78); // enable accel and gyro to read into fifo
                ReadWrite.WriteByte(Constants.UserCtrl, 0x40); // reset and enable fifo
                ReadWrite.WriteByte(Constants.IntEnable, 0x1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void Interrupt(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (Mpu6050Device == null) return;

            int interruptStatus = ReadWrite.ReadByte(Constants.IntStatus);
            if ((interruptStatus & 0x10) != 0)
            {
                ReadWrite.WriteByte(Constants.UserCtrl, 0x44); // reset and enable fifo
            }
            if ((interruptStatus & 0x1) == 0) return;
            var ea = new MpuSensorEventArgs
            {
                Status = (byte) interruptStatus,
                SamplePeriod = 0.02f
            };
            var l = new List<MpuSensorValue>();

            int count = ReadWrite.ReadWord(Constants.FifoCount);

            while (count >= Constants.SensorBytes)
            {
                var data = ReadWrite.ReadBytes(Constants.FifoRW, Constants.SensorBytes);
                count -= Constants.SensorBytes;

                var xa = (short) (data[0] << 8 | data[1]);
                var ya = (short) (data[2] << 8 | data[3]);
                var za = (short) (data[4] << 8 | data[5]);

                var xg = (short) (data[6] << 8 | data[7]);
                var yg = (short) (data[8] << 8 | data[9]);
                var zg = (short) (data[10] << 8 | data[11]);

                var sv = new MpuSensorValue
                {
                    AccelerationX = xa/(float) 16384,
                    AccelerationY = ya/(float) 16384,
                    AccelerationZ = za/(float) 16384,
                    GyroX = xg/(float) 131,
                    GyroY = yg/(float) 131,
                    GyroZ = zg/(float) 131
                };
                l.Add(sv);
            }
            ea.Values = l.ToArray();

            if (SensorInterruptEvent == null) return;

            if (ea.Values.Length > 0)
            {
                SensorInterruptEvent(this, ea);
            }
        }

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        public Mpu6050()
        {
            ReadWrite = new ReadWrite(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;

            _interruptPin.Dispose();
            if (Mpu6050Device != null)
            {
                Mpu6050Device.Dispose();
                Mpu6050Device = null;
            }
            disposedValue = true;
        }

        ~Mpu6050()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
