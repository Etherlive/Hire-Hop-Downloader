using Hire_Hop_Interface.Management;
using Hire_Hop_Interface.Objects;
using Hire_Hop_Interface.Requests;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace Downloader_UI
{
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class Export : Window
    {
        #region Fields

        private ClientConnection _client;
        private ControlWriter _writer;

        private string f_name = "../data.csv";

        #endregion Fields

        #region Methods

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() => ExportData()).Start();
        }

        private async void ExportData()
        {
            Console.WriteLine("Pre Loading Data");

            await LabourData.Load(_client);

            Console.WriteLine("Fetching Search Fesults");

            var results = await Search.GetAllResults(_client, new Search.SearchParams() { _closed = false, _open = false, _money_owed = false, _search = false, _depot = -1, _status = "" });

            BulkAdditionalData.LoadExtraDetail(ref results, _client);

            var jobs = BulkAdditionalData.SearchToJob(results);

            BulkAdditionalData.CalculateCosts(ref jobs, _client);

            BulkAdditionalData.CalculateBilling(ref jobs, _client);

            Console.WriteLine($"Finished Collecting {results.Length} Results");
            Console.WriteLine($"Writing Results To {f_name}");

            JSON_To_CSV.Converter.WriteConversion(f_name, JArray.FromObject(jobs));

            Console.WriteLine("Wrote Data Out! Finished!!");
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.SaveFileDialog FileDlg = new Microsoft.Win32.SaveFileDialog();

            FileDlg.AddExtension = true;
            FileDlg.DefaultExt = ".csv";
            FileDlg.FileName = "data";
            FileDlg.InitialDirectory = Directory.GetCurrentDirectory();

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = FileDlg.ShowDialog();
            // Get the selected file name and display in a TextBox. Load content of file in a TextBlock
            if (result == true)
            {
                f_name = FileDlg.FileName;
                Console.WriteLine($"Selected File Location {f_name}");
            }
        }

        #endregion Methods

        #region Constructors

        public Export(ClientConnection client)
        {
            _client = client;
            InitializeComponent();

            _writer = new Downloader_UI.ControlWriter(LogText);
            LogText.Width -= 15;

            Console.SetOut(_writer);
            Console.WriteLine($"Current File Location {f_name}");
        }

        #endregion Constructors
    }
}