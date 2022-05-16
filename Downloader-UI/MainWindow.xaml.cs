using Hire_Hop_Interface.Interface;
using Hire_Hop_Interface.Interface.Connections;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;

namespace Downloader_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Methods

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Error.Visibility = Visibility.Hidden;

            CookieConnection myHHConn = new CookieConnection();

            string uname = username.Text,
                pword = password.Text,
                code = company.Text;

            Login.Content = "Logging In ...";

            bool loggedin = await Authentication.Login(myHHConn, uname, pword, code);

            if (loggedin)
            {
                Login.Content = "Logged In!";
                GetLoginFromInput(out JObject login);
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
            return false;
        }

        private void GetLoginFromInput(out JObject login)
        {
            login = new JObject();
            login["username"] = username.Text;
            login["password"] = RememberPassword.IsChecked.Value ? password.Text : "";
            login["companyCode"] = company.Text;
        }

        #endregion Methods

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            Error.Visibility = Visibility.Hidden;

            if (GetLogin(out string uname, out string pword, out string code, out JObject login))
            {
                username.Text = uname;
                password.Text = pword;
                company.Text = code;

                if (pword.Length > 0) RememberPassword.IsChecked = true;
            }
        }

        #endregion Constructors
    }
}