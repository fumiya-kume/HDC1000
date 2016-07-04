using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.UI.Xaml;

namespace HDC1000
{
    public sealed class HDC1000
    {
        public I2cDevice i2cdevice { get; set; } = null;
        public double Humidity { get; set; } = -1;
        public double Tempporary { get; set; } = -1;

        private DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(1000) };

        public HDC1000()
        {
            init().Wait();

            timer.Tick += async (s, e) =>
            {
                i2cdevice.Write(new byte[] { 0x00 });
                // センサーの測定待ち。測定にに13[ms]かかるので余裕を持って100[ms]待つ。
                await Task.Delay(100);
                var buf = new byte[4];
                i2cdevice.Read(buf);
                // 仕様書通りに変換
                this.Tempporary = (buf[0] * 256.0 + buf[1]) / 65536 * 165.0 - 40.0;
                this.Humidity = (buf[0] * 256.0 + buf[1]) / 65536 * 100.0;
            };
        }

        public async Task init()
        {
            byte HDC1000Adress = 0x40;

            var devices = await DeviceInformation.FindAllAsync("I2C1");
            var deviceID = devices[0].Id;

            var setting = new I2cConnectionSettings(HDC1000Adress)
            {
                BusSpeed = I2cBusSpeed.FastMode,
                SharingMode = I2cSharingMode.Shared
            };

             this.i2cdevice = await I2cDevice.FromIdAsync(deviceID, setting);
            //Research Humidity nad Temporary
            i2cdevice.Write(new byte[] { 0x02, 0x10, 0x00 });

        }
    }
}
