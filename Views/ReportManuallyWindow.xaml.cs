using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace C3.Views
{
    /// <summary>
    /// ReportManuallyWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ReportManuallyWindow : Window
    {
        private string registeredAt = "";

        public ReportManuallyWindow()
        {
            InitializeComponent();
        }

        public void setRegisteredAt(string value)
        {
            registeredAt = value;
        }

        private void BtnReportManually_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_ReportManually.Text == "")
            {
                MessageBox.Show("Please enter a url to manually report!");
            }
            if (!TextBox_ReportManually.Text.Contains("youtube.com"))
            {
                MessageBox.Show("Please enter a full youtube url! (ex. youtube.com/xxx, not the format of youtu.be/xxx");
            }
            else
            {
                var reportUrl = TextBox_ReportManually.Text;
                JObject jo;

                try
                {
                    var url = "http://c3.iptime.org:1485/api/users/report";
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    httpWebRequest.Headers["x-access-token"] = Application.Current.Resources["token"].ToString();

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        var json = new JObject();
                        json.Add("username", Application.Current.Resources["username"].ToString());
                        json.Add("url", reportUrl);
                        json.Add("registeredAt", registeredAt);

                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        jo = JObject.Parse(result);
                        Debug.WriteLine(jo.ToString());

                        if (jo["success"].Value<bool>())
                        {
                            MessageBox.Show("Successfully sent an report to the link. It may take some time to get result for the report.");
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Failed to add an report manually. ");
                        }
                    }
                } 
                catch (Exception exception)
                {
                    MessageBox.Show("Failed to add an report manually. Error message: " + exception.Message);
                }
            }
        }
    }
}
