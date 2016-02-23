using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MPU6050;

namespace Accelerometer_MPU6050
{
    public sealed partial class MainPage : Page
    {
        readonly Mpu6050 _mpu6050 = new Mpu6050();
        int _interruptCount = 0;
        readonly DateTime _startTime;

        public MainPage()
        {
            this.InitializeComponent();
            _mpu6050.InitHardware();
            _mpu6050.SensorInterruptEvent += _mpu6050_SensorInterruptEvent;
            _startTime = DateTime.Now;
        }

        private void _mpu6050_SensorInterruptEvent(object sender, MpuSensorEventArgs e)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _interruptCount += e.Values.Length;
                float samplesPerSecond = (float) _interruptCount/(float) ((DateTime.Now - _startTime).Seconds);

                textBoxStatus.Text = String.Format("{0} {1} {2}", e.Status, e.SamplePeriod, samplesPerSecond);
                textBoxAccel.Text = String.Format("{0}, {1}, {2}", e.Values[0].AccelerationX, e.Values[0].AccelerationY,
                    e.Values[0].AccelerationZ);
                textBoxGyro.Text = String.Format("{0}, {1}, {2}", e.Values[0].GyroX, e.Values[0].GyroY,
                    e.Values[0].GyroZ);
            });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _mpu6050.Dispose();
            Application.Current.Exit();
        }
    }
}
