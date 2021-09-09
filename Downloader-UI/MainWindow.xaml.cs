using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;
using Newtonsoft.Json.Linq;
using Hire_Hop_Interface.Requests;
using Hire_Hop_Interface.Management;

namespace Downloader_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Error.Visibility = Visibility.Hidden;

            if (GetLogin(out string uname, out string pword, out string code, out JObject login))
            {
                username.Text = uname;
                password.Text = pword;
                company.Text = code;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Error.Visibility = Visibility.Hidden;

            ClientConnection myHHConn = new ClientConnection();

            string uname = username.Text,
                pword = password.Text,
                code = company.Text;

            Login.Content = "Logging In ...";

            bool loggedin = await Authentication.Login(myHHConn, uname, pword, code);

            if (loggedin)
            {
                Login.Content = "Logged In!";
                GetLogin(out string u, out string p, out string w, out JObject login);
                File.WriteAllText("./login.json", login.ToString());
                Export expWindow = new Export(myHHConn);
                expWindow.Show();
                this.Hide();
            }
            else
            {
                Error.Visibility = Visibility.Visible;
                Error.Content = "Log In Failed!";
                Login.Content = "Log In Again";
            }
        }

        private bool GetLogin(out string uname, out string pword, out string word, out JObject login)
        {
            uname = ""; pword = ""; word = ""; login = new JObject();
            if (File.Exists("./login.json"))
            {
                string content = File.ReadAllText("./login.json");
                login = JObject.Parse(content);

                uname = login["username"].ToString();
                pword = login["password"].ToString();
                word = login["companyCode"].ToString();

                Console.WriteLine("Loaded details from file");
                return true;
            }
            else
            {
                login = new JObject();
                login["username"] = username.Text;
                login["password"] = password.Text;
                login["companyCode"] = company.Text;
            }
            return false;
        }
    }
}
