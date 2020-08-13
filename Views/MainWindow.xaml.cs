using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace C3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool isMaximized;
        private string currentLanguage;
        private int numAvailable;
        private int maxAvailable;
        private double prevTop;
        private double prevLeft;
        private double prevWidth;
        private double prevHeight;
        private int currentMenu;

        public MainWindow()
        {
            InitializeComponent();

            isMaximized = false;
            numAvailable = 0;
            maxAvailable = 10;

            saveLastWindowSize();

            LocalizeProgram();
            TranslateEachText();
        }

        private void saveLastWindowSize()
        {
            prevTop = Top;
            prevLeft = Left;
            prevWidth = Width;
            prevHeight = Height;
        }

        private void restoreLastWindowSize()
        {
            Top = prevTop;
            Left = prevLeft;
            Width = prevWidth;
            Height = prevHeight;
        }

        private void LocalizeProgram()
        {
            currentLanguage = System.Threading.Thread.CurrentThread.CurrentUICulture.ToString();
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(currentLanguage);

            List<ResourceDictionary> dicList = Application.Current.Resources.MergedDictionaries.ToList();

            string currentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture.ToString();
            string requestedCulture = "Resources/" + string.Format("Localization.{0}.xaml", currentCulture);

            ResourceDictionary resourceDictionary = dicList.FirstOrDefault(d => d.Source.OriginalString == requestedCulture);
            if (resourceDictionary == null)
            {
                requestedCulture = "Localization.en-US.xaml";
                resourceDictionary = dicList.FirstOrDefault(d => d.Source.OriginalString == requestedCulture);
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
        }

        private void TranslateEachText()
        {
            TextWelcomeUsername_Welcome.Text = Application.Current.FindResource("Welcome") + ", ";
            TextWelcomeUsername_Username.Text = "";

            TextRegisterAvailableMonth_Text.Text = Application.Current.FindResource("RegisterAvailableMonth") as String;

            TextNumberAvailable_Text.Text = numAvailable + "/" + maxAvailable;

            BtnPurchaseAdditionalRegistration_Text.Text = Application.Current.FindResource("PurchaseRegistrations") as String;
            BtnUpgradeToPremium_Text.Text = Application.Current.FindResource("UpgradeToPremium") as String;

            TabRegisterVideo_Text.Text = Application.Current.FindResource("RegisterCopyrights") as String;
            TabListCopyrights_Text.Text = Application.Current.FindResource("ListMyCopyrights") as String;

            BtnLogout_Text.Text = Application.Current.FindResource("Logout") as String;
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnMaximizeButtonClick(object sender, RoutedEventArgs e)
        {
            if (isMaximized)
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }
                else
                {
                    isMaximized = false;
                    restoreLastWindowSize();
                }
            }
            else
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }
                else
                {
                    isMaximized = true;
                    saveLastWindowSize();

                    Top = 0;
                    Left = 0;
                    Width = SystemParameters.WorkArea.Width;
                    Height = SystemParameters.WorkArea.Height;
                }
            }
        }

        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void TitleBarMain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void TabRegisterVideo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentMenu == 1)
            {
                currentMenu = 0;

                TabOn_RegisterVideo.Visibility = Visibility.Visible;
                TabOn_ListCopyrights.Visibility = Visibility.Hidden;

                Frame_Tab.Source = new Uri("Page1.xaml", UriKind.Relative);
            }
        }

        private void TabRegisterVideo_MouseEnter(object sender, MouseEventArgs e)
        {
            TabRegisterVideo.Background = (Brush)(new System.Windows.Media.BrushConverter()).ConvertFromString("#30153262");
        }

        private void TabRegisterVideo_MouseLeave(object sender, MouseEventArgs e)
        {
            TabRegisterVideo.Background = Brushes.Transparent;
        }

        private void TabListCopyrights_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentMenu == 0)
            {
                currentMenu = 1;

                TabOn_RegisterVideo.Visibility = Visibility.Hidden;
                TabOn_ListCopyrights.Visibility = Visibility.Visible;
            }

            Frame_Tab.Source = new Uri("Page2.xaml", UriKind.Relative);
        }

        private void TabListCopyrights_MouseEnter(object sender, MouseEventArgs e)
        {
            TabListCopyrights.Background = (Brush)(new System.Windows.Media.BrushConverter()).ConvertFromString("#30153262");
        }

        private void TabListCopyrights_MouseLeave(object sender, MouseEventArgs e)
        {
            TabListCopyrights.Background = Brushes.Transparent;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var reqEmail = TextBox_LoginID.Text;
            var result = requestLogin(reqEmail, PasswordBox_LoginPW.Password);

            if (result != "")
            {
                saveToken(result);

                JObject joUser = requestUserJson(reqEmail);
                if (joUser == null)
                {
                    MessageBox.Show("Somehow failed to get user information. Please email to us and we will fix it as soon as possible.");
                    return;
                }
                saveUserInfo(joUser);

                TextWelcomeUsername_Username.Text = Application.Current.Resources["name"].ToString();
                updateAvailableCount();

                GridLoginPage.Visibility = Visibility.Collapsed;

                checkVideoReports();
            }
            else
            {
                MessageBox.Show("Failed to login. Please check your ID/PW again.");
            }
        }

        public void updateAvailableCount()
        {
            string availableCount = Application.Current.Resources["availableRegisterCount"].ToString();
            Debug.WriteLine(Application.Current.Resources["isPremium"].ToString());
            TextNumberAvailable_Text.Text = (Application.Current.Resources["isPremium"].ToString() == "True") ? availableCount : availableCount + "/10";
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private string requestLogin(string email, string password)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://c3.iptime.org:1485/api/auth/login");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            string encodedPassword = Base64Encode(Uri.EscapeDataString(Uri.EscapeDataString(Uri.EscapeDataString(Reverse(Base64Encode(password))))));

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"email\":\"" + email + "\"," +
                    "\"password\":\"" + encodedPassword + "\"}";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    JObject jo = JObject.Parse(result);
                    Debug.WriteLine(jo.ToString());
                    if (jo["success"].Value<bool>())
                    {
                        return jo["data"].ToString();
                    }
                    else
                    {
                        return "";
                    }
                }
            } 
            catch
            {
                MessageBox.Show("Failed to login. It seems like server is down, please contact us and we will fix as soon as possible.");
            }

            return "";
        }

        private JObject requestUserJson(string email)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://c3.iptime.org:1485/api/users/" + email);
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers["x-access-token"] = Application.Current.Resources["token"].ToString();

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Debug.WriteLine(result);
                JObject jo = JObject.Parse(result);
                if (jo["success"].Value<bool>())
                {
                    return jo;
                }
                else
                {
                    return null;
                }
            }
        }

        private void saveToken(string token)
        {
            Application.Current.Resources["token"] = token;
        }

        private void saveUserInfo(JObject jo)
        {
            Application.Current.Resources["name"] = jo["data"]["name"];
            Application.Current.Resources["email"] = jo["data"]["email"];
            Application.Current.Resources["availableRegisterCount"] = jo["data"]["availableRegisterCount"];
            Application.Current.Resources["isPremium"] = jo["data"]["isPremium"];
            Application.Current.Resources["videoList"] = jo["data"]["videoList"];
            Application.Current.Resources["username"] = jo["data"]["username"];
            Application.Current.Resources["reportList"] = jo["data"]["reportList"];
        }

        private void checkVideoReports()
        {
            JArray reportList = Application.Current.Resources["reportList"] as JArray;

            for (int i = 0; i < reportList.Count; ++i)
            {
                if (!reportList[i]["reported"].Value<bool>())
                {
                    try
                    {
                        // Mark as reported
                        string username = Application.Current.Resources["username"].ToString();
                        string registeredAt = reportList[i]["registeredAt"].ToString();
                        var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://c3.iptime.org:1485/api/users/report/" + username + "/" + registeredAt);
                        httpWebRequest.Method = "GET";
                        httpWebRequest.Headers["x-access-token"] = Application.Current.Resources["token"].ToString();

                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            Debug.WriteLine(result);
                            JObject jo = JObject.Parse(result);
                            if (!jo["success"].Value<bool>())
                            {
                                MessageBox.Show("Error occurred while marking as reported... Response: " + jo.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error occurred while marking as reported. Error Msg: " + e.Message);
                    }

                    if (MessageBox.Show("One link is detected for infringement, Click OK button to connect to that link and check out.") == MessageBoxResult.OK)
                    {
                        string linkToReported = "";
                        if (reportList[i]["url"].ToString().Contains("https"))
                        {
                            linkToReported = reportList[i]["url"].ToString();
                        }
                        else
                        {
                            linkToReported = "https://" + reportList[i]["url"].ToString();
                        }
                        Process proc = new Process();
                        proc.StartInfo.UseShellExecute = true;
                        proc.StartInfo.FileName = linkToReported;
                        proc.Start();
                    }
                }
            }
        }

        private void BtnPurchaseAdditionalRegistration_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = "http://175.125.94.153";
            proc.Start();
        }

        private void BtnUpgradeToPremium_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = "http://175.125.94.153";
            proc.Start();
        }

        private void TextSignUp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = "http://175.125.94.153";
            proc.Start();
        }

        private void TextForgotIDPW_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = "http://175.125.94.153";
            proc.Start();
        }

        private void BtnLogout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GridLoginPage.Visibility = Visibility.Visible;
        }
    }
}
