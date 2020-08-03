using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace C3.Views
{
    /// <summary>
    /// Page2.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Page2 : Page
    {
        List<Rectangle> listTitleRect = new List<Rectangle>();
        List<Rectangle> listDateRect = new List<Rectangle>();
        private static int length = 0;
        private static int currentIdx = -1;
        private string currentRegisteredAt = "";

        public Page2()
        {
            InitializeComponent();

            ImageVideoInfo.Source = null;

            JObject jo = requestUserJson();
            Debug.WriteLine(jo);
            JArray videoList = jo["data"]["videoList"] as JArray;
            Application.Current.Resources["videoList"] = videoList;
            List<TextBlock> listTitleText = new List<TextBlock>();
            List<TextBlock> listDateText = new List<TextBlock>();
            Grid gridFirstLeft;
            Grid gridSecondLeft;
            Grid gridFirstRight;
            Grid gridSecondRight;
            length = videoList.Count;

            for (int i = 0; i < (length + 1) / 2; ++i)
            {
                TextBlock titleLeft = new TextBlock
                {
                    Name = "VideoList_Text_Title" + (2 * i).ToString(),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(19.6, 0, 0, 0),
                    Foreground = (Brush)(new BrushConverter().ConvertFromString("#FFFFFFFF")),
                    FontFamily = new FontFamily(".. / Resources /#AppleSDGothicNeoB00"),
                    FontSize = 18
                };
                TextBlock dateLeft = new TextBlock
                {
                    Name = "VideoList_Text_Date" + (2 * i).ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = (Brush)(new BrushConverter().ConvertFromString("#FFFFFFFF")),
                    FontFamily = new FontFamily(".. / Resources /#AppleSDGothicNeoB00"),
                    FontSize = 18
                };
                TextBlock titleRight = new TextBlock
                {
                    Name = "VideoList_Text_Title" + (2 * i + 1).ToString(),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(19.6, 0, 0, 0),
                    Foreground = (Brush)(new BrushConverter().ConvertFromString("#FFFFFFFF")),
                    FontFamily = new FontFamily(".. / Resources /#AppleSDGothicNeoB00"),
                    FontSize = 18
                };
                TextBlock dateRight = new TextBlock
                {
                    Name = "VideoList_Text_Date" + (2 * i + 1).ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = (Brush)(new BrushConverter().ConvertFromString("#FFFFFFFF")),
                    FontFamily = new FontFamily(".. / Resources /#AppleSDGothicNeoB00"),
                    FontSize = 18
                };

                listTitleText.Add(titleLeft);
                listTitleText.Add(titleRight);
                listDateText.Add(dateLeft);
                listDateText.Add(dateRight);
            }

            for (int i = 0; i < length; ++i)
            {
                TextBlock textTitle = listTitleText[i];
                TextBlock textDate = listDateText[i];

                textTitle.Text = videoList[i]["title"].ToString();
                textDate.Text = new DateTime(Convert.ToInt64(videoList[i]["registeredAt"].ToString())).ToString("yyyy.MM.dd");
            }

            for (int i = 0; i < (length + 1) / 2; ++i)
            { 
                Rectangle rectFirstLeft = new Rectangle
                {
                    Name = "VideoList_Rect_Title" + (2 * i).ToString(),
                    Height = 36.28,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Fill = (Brush)(new BrushConverter().ConvertFromString("#FF32A0AB"))
                };
                Rectangle rectSecondLeft = new Rectangle
                {
                    Name = "VideoList_Rect_Thumbnail" + (2 * i).ToString(),
                    Height = 164.74,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Fill = (Brush)(new BrushConverter().ConvertFromString("#3B32A0AB"))
                };
                Rectangle rectThirdLeft = new Rectangle
                {
                    Height = 14,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Fill = (Brush)(new BrushConverter().ConvertFromString("#3B32A0AB"))
                };
                Rectangle rectFirstRight = new Rectangle
                {
                    Name = "VideoList_Rect_Title" + (2 * i + 1).ToString(),
                    Height = 36.28,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Fill = (Brush)(new BrushConverter().ConvertFromString("#FF32A0AB"))
                };
                Rectangle rectSecondRight = new Rectangle
                {
                    Name = "VideoList_Rect_Thumbnail" + (2 * i + 1).ToString(),
                    Height = 164.74,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Fill = (Brush)(new BrushConverter().ConvertFromString("#3B32A0AB"))
                };
                Rectangle rectThirdRight = new Rectangle
                {
                    Height = 14,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Fill = (Brush)(new BrushConverter().ConvertFromString("#3B32A0AB"))
                };

                gridFirstLeft = new Grid
                {
                    Name = "VideoList_Grid_Title" + (2 * i).ToString(),
                    Height = 36.28,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                gridSecondLeft = new Grid
                {
                    Name = "VideoList_Grid_Date" + (2 * i).ToString(),
                    Height = 164.74,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                gridFirstRight = new Grid
                {
                    Name = "VideoList_Grid_Title" + (2 * i + 1).ToString(),
                    Height = 36.28,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                gridSecondRight = new Grid
                {
                    Name = "VideoList_Grid_Date" + (2 * i + 1).ToString(),
                    Height = 164.74,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };

                listTitleRect.Add(rectFirstLeft);
                listTitleRect.Add(rectFirstRight);
                listDateRect.Add(rectSecondLeft);
                listDateRect.Add(rectSecondRight);

                gridFirstLeft.Children.Add(rectFirstLeft);
                gridFirstLeft.Children.Add(listTitleText[2 * i]);
                gridSecondLeft.Children.Add(rectSecondLeft);
                gridSecondLeft.Children.Add(listDateText[2 * i]);
                gridFirstRight.Children.Add(rectFirstRight);
                gridFirstRight.Children.Add(listTitleText[2 * i + 1]);
                gridSecondRight.Children.Add(rectSecondRight);
                gridSecondRight.Children.Add(listDateText[2 * i + 1]);

                Page2_StackLeft.Children.Add(gridFirstLeft);
                Page2_StackLeft.Children.Add(gridSecondLeft);
                Page2_StackLeft.Children.Add(rectThirdLeft);
                Page2_StackRight.Children.Add(gridFirstRight);
                Page2_StackRight.Children.Add(gridSecondRight);
                Page2_StackRight.Children.Add(rectThirdRight);
            }

            LinkToVideoInfo();
        }

        private void LinkToVideoInfo()
        {
            var videoList = Application.Current.Resources["videoList"] as JArray;

            for (int i = 0; i < length; ++i)
            {
                listTitleRect[i].MouseLeftButtonDown += new MouseButtonEventHandler(ShowVideoInfo);
                listDateRect[i].MouseLeftButtonDown += new MouseButtonEventHandler(ShowVideoInfo);
            }
        }

        private void ShowVideoInfo(object sender, MouseButtonEventArgs e)
        {
            string rectName = (sender as Rectangle).Name;
            for (int i = 0; i < length; ++i)
            {
                if (Convert.ToInt32(rectName.Substring(rectName.IndexOf("Thumbnail") + 9)) == i)
                {
                    currentIdx = i;
                    break;
                }
            }

            if (!Directory.Exists("./thumbnail"))
            {
                Directory.CreateDirectory("./thumbnail");
            }

            Debug.WriteLine("++++++++++++++");
            Debug.WriteLine(Application.Current.Resources["name"]);
            Debug.WriteLine(Application.Current.Resources["availableRegisterCount"]);
            Debug.WriteLine(Application.Current.Resources["isPremium"]);
            Debug.WriteLine(Application.Current.Resources["videoList"]);
            Debug.WriteLine(Application.Current.Resources["username"]);
            Debug.WriteLine("++++++++++++++");

            int idx = currentIdx;
            var videoList = Application.Current.Resources["videoList"] as JArray;
            string localeCode = GetLocaleCode();
            string dateNow = DateTime.UtcNow.Ticks.ToString();
            string thumbnailFilename = "./thumbnail/thumbnail_" + dateNow + ".jpeg";

            string title = videoList[idx]["title"].ToString();
            string registeredAt = new DateTime(Convert.ToInt64(videoList[idx]["registeredAt"].ToString())).ToString("yyyy. MM. dd.");
            currentRegisteredAt = videoList[idx]["registeredAt"].ToString();
            string registerer = Application.Current.Resources["username"].ToString();
            string platform = videoList[idx]["platforms"][0].ToString();
            long fileSize = Convert.ToInt64(videoList[idx]["size"].ToString());
            string duration = videoList[idx]["duration"].ToString();
            string resolution = videoList[idx]["resolution"].ToString();

            // Get thumbnail image
            //var webClient = new WebClient();
            string url = "http://c3.iptime.org:1485/api/users/thumbnail/" + registerer + "/" + videoList[idx]["registeredAt"].ToString();
            //string url = "http://c3.iptime.org:1485/images/thumbnails/" + registerer + "_" + videoList[idx]["registeredAt"].ToString() + ".jpeg";
            Debug.WriteLine("URL: " + url);
            //webClient.Headers.Add("x-access-token", Application.Current.Resources["token"].ToString());
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Method = "GET";
                httpWebRequest.Headers["x-access-token"] = Application.Current.Resources["token"].ToString();

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                long received = 0;
                using (Stream stream = httpResponse.GetResponseStream())
                {
                    using (FileStream fileStream = File.OpenWrite(thumbnailFilename))
                    {
                        int len = (int)httpResponse.ContentLength;
                        byte[] buffer = new byte[len];
                        int size = stream.Read(buffer, 0, len);
                        while (size > 0)
                        {
                            fileStream.Write(buffer, 0, size);
                            received += size;

                            size = stream.Read(buffer, 0, len);
                        }

                        fileStream.Flush();
                        fileStream.Close();
                    }
                }
            }
            catch (Exception exc)
            {
                //if (!exc.Message.Contains("process cannot access the file"))
                MessageBox.Show("Failed to get video informations from server. Check the internet and if it stil does not work, please contact us with this error msg: " + exc.ToString() + " " + exc.Message);
            }

            Page2_StackLeft.Visibility = Visibility.Collapsed;
            Page2_StackRight.Visibility = Visibility.Collapsed;

            if (localeCode == "ko-KR")
            {
                TextVideoInfo_Text.Text = "제목: " + title + "\n등록일: " + registeredAt + "\n저작권자: " + registerer + "\n업로드 주소: " + platform + "\n저작 원본 파일 크기: " + GetFormattedFileSize(fileSize) + "\n저작 원본 영상 길이: " + duration + "\n저작 원본 해상도: " + resolution;
            }
            else
            {
                TextVideoInfo_Text.Text = "Title: " + title + "\nRegistered at: " + registeredAt + "\nRegisterer: " + registerer + "\nUploaded on: " + platform + "\nOriginal File Size: " + GetFormattedFileSize(fileSize) + "\nOriginal File Duration: " + duration + "\nOriginal Resolution: " + resolution;
            }

            ImageVideoInfo.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(thumbnailFilename);

            TextVideoInfo.Visibility = Visibility.Visible;
            ImageVideoInfo.Visibility = Visibility.Visible;
            BtnReportManually.Visibility = Visibility.Visible;
        }

        private JObject requestUserJson()
        {
            var username = Application.Current.Resources["username"];
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://c3.iptime.org:1485/api/users/" + username);
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

        private string GetLocaleCode()
        {
            string currentLanguage = System.Threading.Thread.CurrentThread.CurrentUICulture.ToString();
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(currentLanguage);

            List<ResourceDictionary> dicList = Application.Current.Resources.MergedDictionaries.ToList();

            return System.Threading.Thread.CurrentThread.CurrentUICulture.ToString();
        }

        private string GetFormattedFileSize(long len)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        private void BtnReportManually_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportManuallyWindow();
            dialog.setRegisteredAt(currentRegisteredAt);
            dialog.ShowDialog();
        }
    }
}
