using System;
using System.Threading;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class MainPage : Page
  {
    private const int DISTANCE_THRESHOLD = -45;
    private const int CONNECTION_TIME_IN_SECONDS = 3;
    private int count = 1;
    private int ammount = 0;
    private string msg;
    private Color msgColor;
    private DistanceState currentDistanceState = DistanceState.FAR;
    private Timer timer;
    private int seconds;
    private DistanceState oldDistanceState;
    private ConnectionState connectionState = ConnectionState.DISCONNECTED;
    private ContentDialog dialog = new ContentDialog();


    public MainPage()
    {
      this.InitializeComponent();
    }

    private void btnDiscover_Click(object sender, RoutedEventArgs e)
    {
      BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();
      watcher.Received += OnAdvertisementReceived;
      watcher.ScanningMode = BluetoothLEScanningMode.Active;
      watcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(Guid.Parse("CDB7950D-73F1-4D4D-8E47-C090502DBD63"));

      //var manufacturerData = new BluetoothLEManufacturerData();
      //manufacturerData.CompanyId = 0x0075;

      //// Make sure that the buffer length can fit within an advertisement payload (~20 bytes). 
      //// Otherwise you will get an exception.
      //var writer = new DataWriter();
      //writer.WriteString("Data");
      //manufacturerData.Data = writer.DetachBuffer();
      //watcher.AdvertisementFilter.Advertisement.ManufacturerData.Add(manufacturerData);

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
      StartConnectionTimer();
    }

    private void StartConnectionTimer()
    {
      timer = new Timer((e) => { NotifyConnection(); }, null, 0, 1000);
    }

    private async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
    {
      await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
     () =>
      {

        txtSignal.Text = string.Format("[{0}]", args.RawSignalStrengthInDBm.ToString()); 

        count++;
        ammount += args.RawSignalStrengthInDBm;
                
        if (count == 5)
        {
          int rssiAVG = ammount / count;

          if (rssiAVG < DISTANCE_THRESHOLD)
          {
            msg = "It's far!!!";
            msgColor = Color.FromArgb(255, 255, 0, 0);
            currentDistanceState = DistanceState.FAR;
          }
          else
          {
            msg = "It's close!!!";
            msgColor = Color.FromArgb(255, 0, 255, 0);
            currentDistanceState = DistanceState.CLOSE;
          }

          txtDistance.Foreground = new SolidColorBrush(msgColor);
          txtDistance.Text = msg;

          count = 0;
          ammount = 0;
        }
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

    private async void Publisher_StatusChanged(BluetoothLEAdvertisementPublisher sender, BluetoothLEAdvertisementPublisherStatusChangedEventArgs args)
    {
      await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
      () =>
      {
        txtStatus.Text = args.Status.ToString() + " " + args.Error.ToString();
      }
      );
    }

    private async void NotifyConnection()
    {
      await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
      () =>
      {
        if (oldDistanceState == currentDistanceState)
        {
          seconds++;
          if (seconds == CONNECTION_TIME_IN_SECONDS)
          {
            if (ConnectionState.DISCONNECTED.Equals(connectionState))
            {
              if (DistanceState.CLOSE.Equals(currentDistanceState))
              {
                connectionState = ConnectionState.CONNECTED;
                txtConnectionState.Text = "Connected!!!";
              }
            }
            else
            {
              if (DistanceState.FAR.Equals(currentDistanceState))
              {
                connectionState = ConnectionState.DISCONNECTED;
                txtConnectionState.Text = "Disconnected!!!";
              }
            }
            seconds = 0;
          }
        }
        else
        {
          seconds = 0;
        }
        oldDistanceState = currentDistanceState;
      });
    }

    enum DistanceState
    {
      CLOSE,
      FAR
    }

    enum ConnectionState
    {
      CONNECTED,
      DISCONNECTED
    }
  }
}
