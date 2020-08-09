using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;

namespace C3.Views
{
    /// <summary>
    /// Page1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Page1 : Page
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        private static string fullPath;
        private string filename;
        private long fileSize;
        private static bool isUserDraggingSlider;
        private const int numThumbnail = 5;
        private int frames = 0;

        public Page1()
        {
            InitializeComponent();

            isUserDraggingSlider = false;

            TranslateEachText();

            Debug.WriteLine("--------------------");
            Debug.WriteLine(Application.Current.Resources["name"]);
            Debug.WriteLine(Application.Current.Resources["availableRegisterCount"]);
            Debug.WriteLine(Application.Current.Resources["isPremium"]);
            Debug.WriteLine(Application.Current.Resources["videoList"]);
            Debug.WriteLine(Application.Current.Resources["username"]);
            Debug.WriteLine("--------------------");
        }

        private void TranslateEachText()
        {
            TextTitleRegisterNew.Text = Application.Current.FindResource("TitleRegisterNew") as String;
            TextAddVideoFile.Text = Application.Current.FindResource("AddVideoFile") as String;
            TextRegisterThisFile_Text.Text = Application.Current.FindResource("RegisterThisFile") as String;
            TextContentTitle.Text = Application.Current.FindResource("ContentTitle") as String;
            TextRegisterer.Text = Application.Current.FindResource("ContentRegisterer") as String;
            TextMadeAt.Text = Application.Current.FindResource("ContentMadeAt") as String;
            TextKeyword.Text = Application.Current.FindResource("ContentKeyword") as String;
            TextDescription.Text = Application.Current.FindResource("ContentDescription") as String;
            TextOriginalLink.Text = Application.Current.FindResource("ContentOriginalLink") as String;
            TextKeywordHint.Text = Application.Current.FindResource("ContentKeywordHint") as String;
        }

        static string BytesToString(long byteCount)
        {
            string[] suf = { "b", "kb", "mb", "gb", "tb", "pb", "eb" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        static string SecToTimeFormat(double sec)
        {
            if (Convert.ToInt32(Math.Floor(sec)) / 3600 == 0)
            {
                return (Convert.ToInt32(Math.Floor(sec)) / 60).ToString() + ":" + Convert.ToInt32(Math.Floor(sec)).ToString();
            }
            return (Convert.ToInt32(Math.Floor(sec)) / 3600).ToString() + ":" + (Convert.ToInt32(Math.Floor(sec)) / 60).ToString() + ":" + Convert.ToInt32(Math.Floor(sec)).ToString();
        }

        private void saveUserInfo(JObject jo)
        {
            Application.Current.Resources["name"] = jo["data"]["name"];
            Application.Current.Resources["availableRegisterCount"] = jo["data"]["availableRegisterCount"];
            Application.Current.Resources["isPremium"] = jo["data"]["isPremium"];
            Application.Current.Resources["videoList"] = jo["data"]["videoList"];
            Application.Current.Resources["username"] = jo["data"]["username"];
        }

        private JObject requestUserJson()
        {
            var email = Application.Current.Resources["email"];
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

        private void ChooseVideoFileToAdd(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "All Files |*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                fullPath = dlg.FileName;
                fileSize = new System.IO.FileInfo(fullPath).Length;
                filename = fullPath.Substring(fullPath.LastIndexOf('\\') + 1);

                // if filename contains spaces, replace it to underline
                /*
                if (filename.Contains(' '))
                {
                    Debug.WriteLine(fullPath);
                    System.IO.File.Move(fullPath, fullPath.Replace(' ', '_'));
                    fullPath = fullPath.Replace(' ', '_');
                }

                filename = fullPath.Substring(fullPath.LastIndexOf('\\') + 1);*/

                VideoPlayer.Visibility = Visibility.Visible;
                VideoPlayer.BeginInit();
                VideoPlayer.Source = new Uri(fullPath, UriKind.RelativeOrAbsolute);
                VideoPlayer.EndInit();
            }
        }

        private void BtnVideoPlayerPlay_Click(object sender, RoutedEventArgs e)
        {
            if (VideoPlayer.Source != null)
            {
                VideoPlayer.Play();
                VideoPlayer.Opacity = 1.0;

                BtnVideoPlayerPlay.Visibility = Visibility.Collapsed;
                BtnVideoPlayerPause.Visibility = Visibility.Visible;

                TextAddedFileName.Visibility = Visibility.Collapsed;
                TextAddedFileInfo.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnVideoPlayerPause_Click(object sender, RoutedEventArgs e)
        {
            if (VideoPlayer.CanPause)
            {
                VideoPlayer.Pause();
                VideoPlayer.Opacity = 0.3;

                Slider.Value = VideoPlayer.Position.TotalSeconds;

                BtnVideoPlayerPause.Visibility = Visibility.Collapsed;
                BtnVideoPlayerPlay.Visibility = Visibility.Visible;

                TextAddedFileName.Visibility = Visibility.Visible;
                TextAddedFileInfo.Visibility = Visibility.Visible;
            }
        }

        private void BtnVideoPlayerStop_Click(object sender, RoutedEventArgs e)
        {
            if (VideoPlayer.Source != null)
            {
                VideoPlayer.Stop();
                VideoPlayer.Opacity = 0.3;

                Slider.Value = VideoPlayer.Position.TotalSeconds;

                if (BtnVideoPlayerPause.Visibility == Visibility.Visible)
                {
                    BtnVideoPlayerPlay.Visibility = Visibility.Visible;
                    BtnVideoPlayerPause.Visibility = Visibility.Collapsed;

                    TextAddedFileName.Visibility = Visibility.Visible;
                    TextAddedFileInfo.Visibility = Visibility.Visible;
                }
            }
        }

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            GridVideoPlayer.MouseLeftButtonDown -= ChooseVideoFileToAdd;

            BtnRegisterThisFile.IsEnabled = true;

            var converter = new System.Windows.Media.BrushConverter();
            var brush = (Brush)converter.ConvertFromString("#66FFFFFF");
            TextRegisterThisFile.Foreground = brush;

            VideoPlayer.Play();
            VideoPlayer.Stop();

            RectLargePlus1.Visibility = Visibility.Collapsed;
            RectLargePlus2.Visibility = Visibility.Collapsed;
            TextAddVideoFile.Visibility = Visibility.Collapsed;

            TextAddedFileName.Visibility = Visibility.Visible;
            TextAddedFileName_Text.Text = filename;

            TextAddedFileInfo.Visibility = Visibility.Visible;
            TextAddedFileInfo_Text.Text = BytesToString(fileSize) + " / " + SecToTimeFormat(VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds);
        }

        private void VideoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show("Failed to open the file as video. \n" + e.ErrorException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void VideoPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Play();
            VideoPlayer.Pause();

            Prepare_Ticker();
        }

        private void Prepare_Ticker()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.02);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if ((VideoPlayer.Source != null) && (VideoPlayer.NaturalDuration.HasTimeSpan) && (!isUserDraggingSlider))
            {
                Slider.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                Slider.Value = VideoPlayer.Position.TotalSeconds;
            }
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (BtnVideoPlayerPause.Visibility == Visibility.Visible)
            {
                BtnVideoPlayerPlay.Visibility = Visibility.Visible;
                BtnVideoPlayerPause.Visibility = Visibility.Collapsed;
            }

            VideoPlayer.Opacity = 0.3;
            VideoPlayer.Stop();

            TextAddedFileName.Visibility = Visibility.Visible;
            TextAddedFileInfo.Visibility = Visibility.Visible;
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isUserDraggingSlider = true;
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isUserDraggingSlider = false;
            VideoPlayer.Position = TimeSpan.FromSeconds(Slider.Value);
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            if (VideoPlayer.Source != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    var slider = (Slider)sender;
                    Point position = e.GetPosition(slider);
                    double d = 1.0d / slider.ActualWidth * position.X;
                    var p = slider.Maximum * d;

                    VideoPlayer.Position = TimeSpan.FromSeconds(p);
                    slider.Value = p;
                }
            }
        }

        private void BtnRegisterThisFile_Click(object sender, RoutedEventArgs e)
        {
            BtnRegisterThisFile.Background = Brushes.Transparent;
            BtnRegisterThisFile.IsEnabled = false;
            ProgressRegistering.Visibility = Visibility.Visible;

            // ffmpeg -i kakaotalk_1558771745380.mp4 -i watermark.png -qscale:v 0 -filter_complex "[1]lut=a=val*0.3[a];[0][a]overlay=x=(main_w-overlay_w):y=(main_h-overlay_h)" outputVideo.mp4
            string outputPath = (fullPath.Substring(0, fullPath.LastIndexOf('.')) + "_watermarked" + fullPath.Substring(fullPath.LastIndexOf('.'))).Replace("\\", "/");
            var markPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\Images\watermark.png";
            var inputArgs = ("-i \"" + fullPath + "\" -i \"" + markPath + "\" -qscale:v 0 -filter_complex \"[1]lut=a=val*0.3[a];[0][a]overlay=x=(main_w-overlay_w):y=(main_h-overlay_h)\" \"" + outputPath + "\"").Replace("\\", "/");

            Debug.WriteLine("Base Directory: " + System.AppDomain.CurrentDomain.BaseDirectory);
            Debug.WriteLine("markPath: " + markPath);
            Debug.WriteLine("ffmpeg Path: " + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\ffmpeg\bin\ffmpeg.exe");
            Debug.WriteLine("inputArgs: ffmpeg.exe " + inputArgs);
            Debug.WriteLine("outputPath: " + outputPath);

            var ffprobe = new Process
            {
                StartInfo =
                {
                    FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\ffmpeg\bin\ffprobe.exe",
                    Arguments = "-v error -select_streams v:0 -show_entries stream=nb_frames -of default=nokey=1:noprint_wrappers=1 \"" + fullPath + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            ffprobe.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                if (e.Data != null)
                {
                    try
                    {
                        frames = Convert.ToInt32(e.Data.Trim());
                    }
                    catch
                    {
                        Debug.WriteLine("Error getting duration of the video...");
                        if (ffprobe != null) ffprobe.Dispose();
                    }
                }
            });

            ffprobe.Start();
            ffprobe.BeginOutputReadLine();
            ffprobe.BeginErrorReadLine();
            ffprobe.WaitForExit();

            var process = new Process
            {
                StartInfo =
                {
                    FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\ffmpeg\bin\ffmpeg.exe",
                    Arguments = $"{inputArgs}",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true
                }
            };

            //AllocConsole();
            
            /*
            process.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                Console.WriteLine(e.Data);
            });*/
            process.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                Console.WriteLine(e.Data);

                if (e.Data != null && e.Data.Contains("Error", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("An Error occured while watermarking. Please check your video file and if there's still error, send an e-mail to us so that we can reach to help you.");
                    ResetToDefaultInputMode();
                    return;
                }

                //Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate { ProgressRegistering.Value = 100 * currentFrame / frames; }));
                //Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate { TextRegisterThisFile_Text.Text = e.Data; }));
            });

            process.Start();
            //process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var ffmpegIn = process.StandardInput;

            // Write Data
            ffmpegIn.Write("y\n");

            // Make sure ffmpeg has finished the work
            process.WaitForExit();
            //FreeConsole();

            // TODO: !!!!!!!!!!!!!!!!!!1
            Application.Current.Resources["thumbnail"] = selectThumbnail(outputPath);

            VideoPlayer.Visibility = Visibility.Collapsed;
            GridVideoDescription.Visibility = Visibility.Visible;
            TextTitleRegisterNew.Text = Application.Current.FindResource("TitleSubmitVideoInfo") as String;
            TextAddedFileName.Visibility = Visibility.Collapsed;
            TextAddedFileInfo.Visibility = Visibility.Collapsed;

            TextBox_Registerer.Text = Application.Current.Resources["username"].ToString();
            
            BtnRegisterThisFile.Background = (Brush)(new System.Windows.Media.BrushConverter()).ConvertFromString("#B3000000");
            BtnRegisterThisFile.Click -= BtnRegisterThisFile_Click;
            BtnRegisterThisFile.Click += BtnRegisterThisFile_Click2;

            BtnRegisterThisFile.IsEnabled = true;
            ProgressRegistering.Visibility = Visibility.Collapsed;
            ProgressRegistering.Value = 0;

            TextRegisterThisFile_Text.Text = "등록 완료파일 저장하기";
        }

        private string selectThumbnail(string path)
        {
            bool selected = false;
            Debug.WriteLine(path);
            var outputDir = path.Substring(0, path.LastIndexOf('/')).Replace('/', '\\');
            string pathThumbnail = "";
            Random r = new Random();

            using (var engine = new Engine())
            {
                var video = new MediaFile { Filename = @path };
                engine.GetMetadata(video);
                var len = video.Metadata.Duration.Seconds;
                int i = 0;

                while (!selected)
                {
                    int sec = r.Next(len);
                    Debug.WriteLine(sec);
                    var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(sec) };
                    pathThumbnail = string.Format("{0}\\thumbnail{1}.jpeg", outputDir, i);
                    var outputThumbnail = new MediaFile { Filename = @pathThumbnail };
                    engine.GetThumbnail(video, outputThumbnail, options);

                    var dialog = new ThumbnailWindow();
                    dialog.setImgSource(pathThumbnail);
                    if (dialog.ShowDialog() == true)
                    {
                        break;
                    }

                    ++i;
                }
            }

            return pathThumbnail;
        }

        private void UpdateProgressBar(int val)
        {
            ProgressRegistering.Value = val;
            TextRegisterThisFile_Text.Text = "이 파일로 영상 저작권 등록중 " + ProgressRegistering.Value + "%";
        }

        // TODO: Implement adding registered video to the real implemented user profile
        private void BtnRegisterThisFile_Click2(object sender, RoutedEventArgs e)
        {
            if (TextBox_ContentTitle.Text == "" || TextBox_Keyword.Text == "" || TextBox_MadeAt.Text == "" || TextBox_OriginalLink.Text == "")
            {
                MessageBox.Show("Please fulfill the given format!");
                return;
            }
            //var result = registerVideo(TextBox_ContentTitle.Text, TextBox_Keyword.Text, TextBox_MadeAt.Text, TextBox_Description.Text, TextBox_OriginalLink.Text);
            var result = registerVideo(TextBox_ContentTitle.Text, TextBox_Keyword.Text, TextBox_MadeAt.Text, TextBox_Description.Text, TextBox_OriginalLink.Text, Application.Current.Resources["thumbnail"].ToString());
            if (result == null)
            {
                MessageBox.Show("Sorry but somehow it failed to register your video. Try again and if it still does not work, please contact us.");
            }
            else if (result != "")
            {
                MessageBox.Show("Successfully registered your video! We will report you when it's on infringement.");
                MessageBox.Show("Check that \'*_watermark\' video file has been created on the same directory of the original video. Note that you have to use the watermarked version when uploading to online!!");
            }
            else
            {
                MessageBox.Show("You do not have available numbers for registration. Purchase more on our website! (Left buttons)");
            }

            ResetToDefaultInputMode();
        }

        private static Tuple<int, int, long> GetVideoDuration(string fileName)
        {
            var inputFile = new MediaToolkit.Model.MediaFile { Filename = @fileName };
            using (var engine = new Engine())
            {
                engine.GetMetadata(@inputFile);
            }

            var size = inputFile.Metadata.VideoData.FrameSize.Split(new[] { 'x' }).Select(o => int.Parse(o)).ToArray();

            return new Tuple<int, int, long>(size[0], size[1], inputFile.Metadata.Duration.Ticks);
        }

        private static string GetVideoResolution(string filename)
        {
            string res = "";

            var ffprobe = new Process
            {
                StartInfo =
                {
                    FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\ffmpeg\bin\ffprobe.exe",
                    Arguments = "-v error -select_streams v:0 -show_entries stream=width,height -of csv=s=x:p=0 \"" + filename + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            ffprobe.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                if (e.Data != null && e.Data.Contains("x"))
                {
                    try
                    {
                        res = e.Data.Trim();
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show("Error occurred: " + exc.Message);
                    }
                }
            });

            ffprobe.Start();
            ffprobe.BeginOutputReadLine();
            ffprobe.BeginErrorReadLine();
            ffprobe.WaitForExit();

            return res;
        }

        //private string registerVideo(string title, string keyword, string madeAt, string contentDescription, string originalLink)
        private string registerVideo(string title, string keyword, string madeAt, string contentDescription, string originalLink, string pathThumbnail)
        {
            string username = Application.Current.Resources["username"].ToString();
            var utcNow = (long)(DateTime.UtcNow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
            JObject jo;
            long fileSize;
            string duration;
            string resolution;

            try
            {
                fileSize = new FileInfo(fullPath).Length;
                duration = TimeSpan.FromTicks(GetVideoDuration(fullPath).Item3).ToString(@"dd\:hh\:mm\:ss");
                resolution = GetVideoResolution(fullPath);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error occurred while extracting video info: " + e.Message);
                return null;
            }

            try
            {
                var url = "http://c3.iptime.org:1485/api/users/registervideo/" + username;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers["x-access-token"] = Application.Current.Resources["token"].ToString();

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    var json = new JObject();
                    json.Add("title", title);
                    json.Add("keywords", JArray.FromObject(keyword.Trim().Split(',')));
                    json.Add("madeAt", madeAt);
                    json.Add("registeredAt", utcNow.ToString().Trim());
                    json.Add("contentDescription", contentDescription);
                    json.Add("platforms", JArray.FromObject(new[] { originalLink }));
                    json.Add("size", fileSize.ToString());
                    json.Add("duration", duration);
                    json.Add("resolution", resolution);

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
                    var keys = jo.Properties().Select(p => p.Name).ToList();

                    if (!keys.Exists(x => x == "title"))
                    {
                        return null;
                    }
                    if (jo["title"].ToString() == "")
                    {
                        return "";
                    }
                }

                var webClient = new WebClient();
                webClient.Headers.Add("x-access-token", Application.Current.Resources["token"].ToString());
                url = "http://c3.iptime.org:1485/api/users/image/" + username + "/" + utcNow.ToString().Trim();

                var response = webClient.UploadFile(url, pathThumbnail);
                Debug.WriteLine("++++++++++++++++++++++");
                Debug.WriteLine(response.ToString());
                Debug.WriteLine("++++++++++++++++++++++");

                /*
                var webClient = new WebClient();
                string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
                var fileData = webClient.Encoding.GetString(File.ReadAllBytes(pathThumbnail));
                Debug.WriteLine(fileData);
                var package = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n{3}\r\n--{0}--\r\n", boundary, System.IO.Path.GetFileName(pathThumbnail), System.IO.Path.GetExtension(pathThumbnail), fileData);
                var nfile = webClient.Encoding.GetBytes(package);

                url = "http://c3.iptime.org:1485/api/users/image/" + username + "/" + utcNow.ToString().Trim();

                webClient.Headers.Add("Content-Type", "multipart/form-data; boundary=" + boundary);
                webClient.Headers.Add("x-access-token", Application.Current.Resources["token"].ToString());

                var resp = webClient.UploadData(url, "POST", nfile);
                Debug.WriteLine("++++++++++++++++++++++");
                Debug.WriteLine(resp.ToString());
                Debug.WriteLine("++++++++++++++++++++++");*/
            }
            catch (Exception e)
            {
                MessageBox.Show("Error occurred while communication: " + e.Message);
                return null;
            }

            return jo["title"].ToString();
        }

        private void ResetToDefaultInputMode()
        {
            GridVideoPlayer.MouseLeftButtonDown += ChooseVideoFileToAdd;

            GridVideoDescription.Visibility = Visibility.Collapsed;

            BtnRegisterThisFile.IsEnabled = false;

            var converter = new System.Windows.Media.BrushConverter();
            var brush = (Brush)converter.ConvertFromString("#11FFFFFF");
            TextRegisterThisFile.Foreground = brush;

            VideoPlayer.Stop();
            VideoPlayer.Visibility = Visibility.Collapsed;
            VideoPlayer.Source = null;

            TextBox_ContentTitle.Text = "";
            TextBox_Description.Text = "";
            TextBox_Keyword.Text = "";
            TextBox_MadeAt.Text = "";
            TextBox_OriginalLink.Text = "";

            RectLargePlus1.Visibility = Visibility.Visible;
            RectLargePlus2.Visibility = Visibility.Visible;
            TextAddVideoFile.Visibility = Visibility.Visible;

            TextAddedFileName.Visibility = Visibility.Collapsed;
            TextAddedFileInfo.Visibility = Visibility.Collapsed;

            //BtnRegisterThisFile
            BtnRegisterThisFile.Background = (Brush)(new System.Windows.Media.BrushConverter()).ConvertFromString("#7F32A0AB");
            BtnRegisterThisFile.Click -= BtnRegisterThisFile_Click2;
            BtnRegisterThisFile.Click += BtnRegisterThisFile_Click;

            TextRegisterThisFile_Text.Text = Application.Current.FindResource("RegisterThisFile") as String;
            TextTitleRegisterNew.Text = Application.Current.FindResource("TitleRegisterNew") as String;
        }
    }
}
