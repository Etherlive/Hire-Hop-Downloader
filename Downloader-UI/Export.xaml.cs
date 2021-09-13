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

        Thread data_export_thread;
        bool IsExporting = false;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!IsExporting)
            {
                Search.SearchParams @params = new Search.SearchParams()
                {
                    _open = search_open_jobs.IsChecked.Value,
                    _closed = search_closed_jobs.IsChecked.Value,
                    _is_late = search_late.IsChecked.Value,
                    _money_owed = search_owes.IsChecked.Value,

                    _search = !search_ignore_search.IsChecked.Value
                };

                switch (search_depot.SelectedValue)
                {
                    case "Any":
                        @params._depot = -1;
                        break;

                    case "EMEA":
                        @params._depot = 1;
                        break;

                    case "APAC":
                        @params._depot = 4;
                        break;

                    case "USA":
                        @params._depot = 5;
                        break;
                }

                data_export_thread = new Thread(() => ExportData(@params));
                data_export_thread.Start();
            }
            else
            {
                Console.WriteLine("Cannot Start, Already Running");
            }
        }

        private async void ExportData(Search.SearchParams @params)
        {
            if (IsExporting) return;
            IsExporting = true;
            Console.WriteLine("Pre Loading Data ...");

            await LabourData.Load(_client);

            Console.WriteLine("Fetching Search Fesults ...");

            var results = await Search.GetAllResults(_client, @params);

            BulkAdditionalData.LoadExtraDetail(ref results, _client);

            var jobs = BulkAdditionalData.SearchToJob(results);

            BulkAdditionalData.CalculateBilling(ref jobs, _client);

            BulkAdditionalData.SetLastModified(ref jobs, _client);

            BulkAdditionalData.CalculateCosts(ref jobs, _client);

            Console.WriteLine($"Finished Collecting {results.Length} Results");
            Console.WriteLine($"Writing Results To {f_name}");

            JSON_To_CSV.Converter.WriteConversion(f_name, JArray.FromObject(jobs));

            Console.WriteLine("Wrote Data Out! Finished!!");

            this.Dispatcher.Invoke(()=> { finished_export_page.Show(); });
            IsExporting = false;
        }

        private FinishedExport finished_export_page = new FinishedExport();

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