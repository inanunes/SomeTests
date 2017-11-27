using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CancellableTask
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private CancellationTokenSource ts = new CancellationTokenSource();
    private Task currentOpenFolderTask = null;

    public MainWindow()
    {
      InitializeComponent();
    }

    private async Task<List<string>> GetFolderResourcesAsync(CancellationToken ct)
    {
      List<string> folders = new List<string>();

      for (int i = 0; i < 5; i++)
      {
        if (ct.IsCancellationRequested)
        {
          ct.ThrowIfCancellationRequested();
        }
        folders.Add($"Folder {i}");
        await Task.Delay(TimeSpan.FromSeconds(1));
      }

      return folders;
    }

    private void PrintResources(List<string> folders, string approach)
    {
      Debug.WriteLine("=====================================================================");
      Debug.WriteLine($"{approach}");
      Debug.WriteLine("=====================================================================");

      foreach (var folder in folders)
      {
        Debug.WriteLine(folder);
      }

    }
    
    private async Task<bool> OpenFolderAsync(string approachNumber)
    {
      bool sucess = false;

      CancelPreviousOpenFolderTask();

      CancellationToken ct = ts.Token;

      await Task.Run(async () =>
      {
        try
        {
          currentOpenFolderTask = TryOpenFolderAsync(approachNumber, ct);
          await currentOpenFolderTask;
          sucess = true;
        }
        catch (OperationCanceledException ex)
        {
          Debug.WriteLine($"Approach {approachNumber} cancelled");
        }
      }, ct);

      return sucess;
    }

    private async Task TryOpenFolderAsync(string approachNumber, CancellationToken ct)
    {
      Debug.WriteLine($"Getting Folders - {approachNumber}");
      var resources = await GetFolderResourcesAsync(ct);

      Debug.WriteLine($"Preparing to print folders - {approachNumber}");
      await Task.Delay(TimeSpan.FromSeconds(3));

      if (!ct.IsCancellationRequested)
      {
        PrintResources(resources, approachNumber);
      }else
      {
        ct.ThrowIfCancellationRequested();
      }
    }

    private void CancelPreviousOpenFolderTask()
    {
      if (currentOpenFolderTask != null && TaskStatus.WaitingForActivation.Equals(currentOpenFolderTask.Status))
      {
        ts.Cancel();
        ts = new CancellationTokenSource();
      }
    }

    private async void button_Click(object sender, RoutedEventArgs e)
    {
      string approach = "0";
      bool result = await OpenFolderAsync(approach);
      Debug.WriteLine($"Open folder returned {result} to approach {approach}");
    }

    private async void button1_Click(object sender, RoutedEventArgs e)
    {
      string approach = "1";
      bool result = await OpenFolderAsync(approach);
      Debug.WriteLine($"Open folder returned {result} to approach {approach}");
    }

    private async void button2_Click(object sender, RoutedEventArgs e)
    {
      string approach = "2";
      bool result = await OpenFolderAsync(approach);
      Debug.WriteLine($"Open folder returned {result} to approach {approach}");
    }

    private async void button3_Click(object sender, RoutedEventArgs e)
    {
      string approach = "3";
      bool result = await OpenFolderAsync(approach);
      Debug.WriteLine($"Open folder returned {result} to approach {approach}");
    }
  }
}
