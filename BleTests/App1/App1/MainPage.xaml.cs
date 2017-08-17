using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void btnDiscover_Click(object sender, RoutedEventArgs e)
        {
            BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();
            watcher.Received += OnAdvertisementReceived;
            watcher.ScanningMode = BluetoothLEScanningMode.Active;


            //// Set the in-range threshold to -70dBm. This means advertisements with RSSI >= -70dBm 
            //// will start to be considered "in-range" (callbacks will start in this range).
            //watcher.SignalStrengthFilter.InRangeThresholdInDBm = -70;

            //// Set the out-of-range threshold to -75dBm (give some buffer). Used in conjunction 
            //// with OutOfRangeTimeout to determine when an advertisement is no longer 
            //// considered "in-range".
            //watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -75;

            //// Set the out-of-range timeout to be 2 seconds. Used in conjunction with 
            //// OutOfRangeThresholdInDBm to determine when an advertisement is no longer 
            //// considered "in-range"
            //watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(2000);

            watcher.Start();
        }

        private async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
           () =>
            {
            //    IList<BluetoothLEManufacturerData> manufacturerDataList = args.Advertisement.ManufacturerData;

            //    List< BluetoothLEManufacturerData> list = manufacturerDataList.ToList<BluetoothLEManufacturerData>();

            //    txtDistance.Text = string.Format("Company ID: {0} , Data: {1} , Distance: {2}", list[0].CompanyId, list[0].Data, args.RawSignalStrengthInDBm);

            txtDistance.Text = args.RawSignalStrengthInDBm.ToString();

            }
           );
        }

        private void btnAdvertise_Click(object sender, RoutedEventArgs e)
        {
            BluetoothLEAdvertisementPublisher publisher = new BluetoothLEAdvertisementPublisher();

            // Add custom data to the advertisement
            var manufacturerData = new BluetoothLEManufacturerData();
            manufacturerData.CompanyId = 0xFFFE;

            var writer = new DataWriter();
            writer.WriteString("Hello World");

            // Make sure that the buffer length can fit within an advertisement payload (~20 bytes). 
            // Otherwise you will get an exception.
            manufacturerData.Data = writer.DetachBuffer();

            // Add the manufacturer data to the advertisement publisher:
            publisher.Advertisement.ManufacturerData.Add(manufacturerData);
            publisher.StatusChanged += Publisher_StatusChanged;

            publisher.Start();

        }

        private void Publisher_StatusChanged(BluetoothLEAdvertisementPublisher sender, BluetoothLEAdvertisementPublisherStatusChangedEventArgs args)
        {
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                txtStatus.Text = args.Status.ToString() + " " + args.Error.ToString();
            }
            );
        }
    }
}
