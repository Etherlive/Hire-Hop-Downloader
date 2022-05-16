using Hire_Hop_Interface.Objects;
using Hire_Hop_Interface.Interface.Connections;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows;
using System.Collections.Generic;
using Hire_Hop_Interface.Interface.Connections;
using Newtonsoft.Json.Linq;
using System;
using Hire_Hop_Interface.Interface;
using Hire_Hop_Interface.Objects;
using Hire_Hop_Interface.Objects.JobProject;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Downloader_UI
{
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class Export : Window
    {
        #region Fields

        private CookieConnection _client;
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
                SearchResult.SearchOptions @params = new SearchResult.SearchOptions()
                {
                    open = search_open_jobs.IsChecked.Value,
                    closed = search_closed_jobs.IsChecked.Value,
                    is_late = search_late.IsChecked.Value,
                    money_owed = search_owes.IsChecked.Value,

                    search = !search_ignore_search.IsChecked.Value
                };

                List<string> status = new List<string>();

                for (int i = 0; i < search_status.Items.Count; i++)
                {
                    if (((CheckBox)search_status.Items[i]).IsChecked.Value)
                    {
                        status.Add(i.ToString());
                    }
                }

                @params.status = String.Join(',', status);

                switch (((ComboBoxItem)search_depot.SelectedValue).Content)
                {
                    case "Any":
                        @params.depot = -1;
                        break;

                    case "EMEA":
                        @params.depot = 1;
                        break;

                    case "APAC":
                        @params.depot = 4;
                        break;

                    case "USA":
                        @params.depot = 5;
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

        private async void ExportData(SearchResult.SearchOptions @params)
        {
            if (IsExporting) return;
            IsExporting = true;
            Console.WriteLine("Pre Loading Data ...");

            var labour_costs = await DefaultCost.GetAllLabourCosts(_client);

            Console.WriteLine("Fetching Jobs ...");

            var results = await SearchResult.SearchForAll(@params, _client);

            var t_jobs = results.results.Select(x => x.GetJob(_client)).ToArray();
            Task.WaitAll(t_jobs);
            var jobs = t_jobs.Select(x => new JobWithMisc(x.Result)).ToArray();

            Console.WriteLine($"Finished Collecting {jobs.Length} Jobs");
            Console.WriteLine("Now Loading Associated Data");

            var load_misc_t = jobs.Select(x => x.LoadMisc(_client, labour_costs.results)).ToArray();
            Task.WaitAll(load_misc_t);

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

        public Export(CookieConnection client)
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