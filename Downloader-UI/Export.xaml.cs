using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Hire_Hop_Interface.Requests;
using Hire_Hop_Interface.Objects;
using Hire_Hop_Interface.Management;
using System.Threading;
using System.IO;
using Hire_Hop_Interface;

namespace Downloader_UI
{
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class Export : Window
    {
        ClientConnection _client;
        ControlWriter _writer;

        public Export(ClientConnection client)
        {
            _client = client;
            InitializeComponent();

            _writer = new Downloader_UI.ControlWriter(LogText);

            Console.SetOut(_writer);
        }

        private async void ExportData()
        {
            Console.WriteLine("Pre Loading Data");

            await LabourData.Load(_client);

            Console.WriteLine("Fetching Search Fesults");

            var results = await Search.GetAllResults(_client, new Search.SearchParams() { _closed = false, _open = false, _money_owed = false, _search = false, _depot=-1, _status="" });

            BulkAdditionalData.LoadExtraDetail(ref results, _client);

            var jobs = BulkAdditionalData.SearchToJob(results);

            BulkAdditionalData.CalculateCosts(ref jobs, _client);

            BulkAdditionalData.CalculateBilling(ref jobs, _client);

            Console.WriteLine($"Finished Collecting {results.Length} Results");
            Console.WriteLine("Writing Results To data.csv");

            JSON_To_CSV.Converter.WriteConversion("../data.csv", JArray.FromObject(jobs));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() => ExportData()).Start();
        }
    }
}
