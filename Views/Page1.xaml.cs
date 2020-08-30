using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using HiddenWatermark;

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

        private static string workdir;
        private static string fullPath; // full video path
        private static string filename;
        private static long fileSize;
        private static bool isUserDraggingSlider;

        private static Watermark _watermark;

        public Page1()
        {
            InitializeComponent();

            workdir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            isUserDraggingSlider = false;

            _watermark = new Watermark(true);

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

        private void ChooseVideoFileToAdd(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "All Files |*";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                fullPath = dlg.FileName;
                fileSize = new System.IO.FileInfo(fullPath).Length;
                filename = fullPath.Substring(fullPath.LastIndexOf('\\') + 1);

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

            var converter = new BrushConverter();
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

        private void ChangeStateText(string rsc, bool findFromResource)
        {
            if (findFromResource)
            {
                TextRegisterThisFile_Text.Text = Application.Current.FindResource(rsc) as String;
            }
            else
                TextRegisterThisFile_Text.Text = rsc;
        }

        private void ChangeStateText(string rsc, bool findFromResource, int val)
        {
            if (findFromResource)
                ChangeStateText((Application.Current.FindResource(rsc) as String) + " " + ((int)ProgressRegistering.Value) + " %", false);
            else
                ChangeStateText(rsc + " " + ((int)ProgressRegistering.Value) + " %", false);
        }

        // Start watermarking & registering a video
        private void BtnRegisterThisFile_Click(object sender, RoutedEventArgs e)
        {
            string outputPath = (fullPath.Substring(0, fullPath.LastIndexOf('.')) + "_watermarked" + fullPath.Substring(fullPath.LastIndexOf('.'))).Replace("\\", "/");
            string frameDir = workdir.Replace('\\', '/') + "/frames_watermarked/";
            double duration = GetVideoDuration(fullPath);
            string prevVid = fullPath;
            string targetVid = fullPath;
            bool isInitialEmbed = true;

            BtnRegisterThisFile.Background = Brushes.Transparent;
            BtnRegisterThisFile.IsEnabled = false;
            ProgressRegistering.Visibility = Visibility.Visible;

            // Extract all video frames
            this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                ChangeStateText("ExtractingFrames", false);
            }));
            ExtractFrames(fullPath, "frames");

            // iterate frame image files
            string[] files = Directory.GetFiles(workdir + "\\frames", "*.jpg", SearchOption.AllDirectories);

            if (Directory.Exists(workdir + "\\frames_watermarked"))
                Directory.Delete(workdir + "\\frames_watermarked", true);
            Directory.CreateDirectory(workdir + "\\frames_watermarked");

            var sw = Stopwatch.StartNew();

            // add watermark to each frames
            for (int i = 0; i < files.Length; ++i)
            {
                double progressPerc = ((double)i) / files.Length * 100;
                this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    ChangeStateText("WatermarkingFrames", true, (int)progressPerc);
                    UpdateProgressBar(progressPerc);
                }));

                string filename = files[i].Trim().Substring(files[i].LastIndexOf('\\') + 1);
                var fileBytes = File.ReadAllBytes(files[i]);
                var embeddedBytes = _watermark.EmbedWatermark(fileBytes);
                string frameLocation = workdir + "/frames_watermarked/" + filename;
                File.WriteAllBytes(frameLocation, embeddedBytes);
            }

            /*
            // replace watermarked frames to each video frames
            for (int i = 0; i < files.Length; ++i)
            {
                double progressedRate = ((double)i) / files.Length * 100;
                this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    ChangeStateText("ReplacingFrames", true, (int)progressedRate);
                    UpdateProgressBar(progressedRate);
                }));

                double timeReplace = duration * i / files.Length;
                targetVid = (fullPath.Substring(0, fullPath.LastIndexOf('.')) + $"_output_{i + 1}." + fullPath.Substring(fullPath.LastIndexOf('.') + 1)).Replace('\\', '/');
                string arg = "-i \"" + prevVid + "\" -i \"" + frameDir + "" + String.Format("{0:D5}", i + 1) + $".jpg\" -filter_complex \"[1]setpts={timeReplace}/TB[im];[0][im]overlay=eof_action=pass\" -codec:v libx264 -crf 18 -preset slow -pix_fmt yuv420p -c:a copy \"" + targetVid + "\"";
                Debug.WriteLine(arg);

                var process = new Process
                {
                    StartInfo =
                {
                    FileName = workdir + @"\ffmpeg\bin\ffmpeg.exe",
                    Arguments = arg,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                }
                };

                process.Start();
                process.StandardInput.Write("y\n");
                process.WaitForExit();

                if (isInitialEmbed)
                {
                    isInitialEmbed = false;
                }
                else
                {
                    File.Delete(prevVid);
                }

                prevVid = targetVid;
            }*/

            string arg = "-i \"" + frameDir + "%05d.jpg\" -codec:v libx264 -crf 18 -preset slow -pix_fmt yuv420p \"" + outputPath + "\"";

            this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                ChangeStateText("ReplacingFrames", true);
            }));

            var process = new Process
            {
                StartInfo =
                {
                    FileName = workdir + @"\ffmpeg\bin\ffmpeg.exe",
                    Arguments = arg,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                }
            };

            process.Start();
            process.StandardInput.Write("y\n");
            process.WaitForExit();

            sw.Stop();

            /*
            // rename last iterated video for being output file
            if (File.Exists(outputPath))
                File.Delete(outputPath);
            File.Move(targetVid, outputPath);*/

            // let user select thumbnail of his video
            Application.Current.Resources["thumbnail"] = selectThumbnail(outputPath);

            VideoPlayer.Visibility = Visibility.Collapsed;
            GridVideoDescription.Visibility = Visibility.Visible;
            TextTitleRegisterNew.Text = Application.Current.FindResource("TitleSubmitVideoInfo") as String;
            TextAddedFileName.Visibility = Visibility.Collapsed;
            TextAddedFileInfo.Visibility = Visibility.Collapsed;

            TextBox_Registerer.Text = Application.Current.Resources["username"].ToString();
            
            BtnRegisterThisFile.Background = (Brush)(new BrushConverter()).ConvertFromString("#B3000000");
            BtnRegisterThisFile.Click -= BtnRegisterThisFile_Click;
            BtnRegisterThisFile.Click += BtnRegisterThisFile_Click2;

            BtnRegisterThisFile.IsEnabled = true;
            ProgressRegistering.Visibility = Visibility.Collapsed;
            ProgressRegistering.Value = 0;

            this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                ChangeStateText("SaveRegisteredFile", true);
            }));
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

        private void UpdateProgressBar(double val)
        {
            ProgressRegistering.Value = val;
        }

        // Adding registered video information to the server
        private void BtnRegisterThisFile_Click2(object sender, RoutedEventArgs e)
        {
            if (TextBox_ContentTitle.Text == "" || TextBox_Keyword.Text == "" || TextBox_MadeAt.Text == "" || TextBox_OriginalLink.Text == "")
            {
                MessageBox.Show("Please fulfill the given format!");
                return;
            }

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

        private static Tuple<int, int, long> GetVideoDurationTuple(string fileName)
        {
            var inputFile = new MediaFile { Filename = @fileName };
            using (var engine = new Engine())
            {
                engine.GetMetadata(@inputFile);
            }

            var size = inputFile.Metadata.VideoData.FrameSize.Split(new[] { 'x' }).Select(o => int.Parse(o)).ToArray();

            return new Tuple<int, int, long>(size[0], size[1], inputFile.Metadata.Duration.Ticks);
        }


        private double GetVideoDuration(string filePath)
        {
            double duration = 0.0;
            string filename = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\ffmpeg\bin\ffprobe.exe";

            var ffprobe = new Process
            {
                StartInfo =
                {
                    FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\ffmpeg\bin\ffprobe.exe",
                    Arguments = "-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"" + filePath.Replace('\\', '/') + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            ffprobe.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                if (e.Data != null)
                {
                    Debug.WriteLine(e.Data);
                    try
                    {
                        duration = Convert.ToDouble(e.Data.Trim());
                    }
                    catch
                    {
                        Debug.WriteLine("Error getting duration of the video...");
                        if (ffprobe != null)
                            ffprobe.Dispose();
                    }
                }
            });

            ffprobe.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                if (e.Data != null)
                {
                    Debug.WriteLine(e.Data);
                    try
                    {
                        duration = Convert.ToDouble(e.Data.Trim());
                    }
                    catch
                    {
                        Debug.WriteLine("Error getting duration of the video...");
                        if (ffprobe != null)
                            ffprobe.Dispose();
                    }
                }
            });

            ffprobe.Start();
            ffprobe.BeginOutputReadLine();
            ffprobe.BeginErrorReadLine();
            ffprobe.WaitForExit();

            return duration;
        }

        private string GetVideoFPS(string filePath)
        {
            string fps = "0";
            string filename = workdir + @"\ffmpeg\bin\ffprobe.exe";

            var ffprobe = new Process
            {
                StartInfo =
                {
                    FileName = workdir + @"\ffmpeg\bin\ffprobe.exe",
                    Arguments = "-v error -select_streams v -of default=noprint_wrappers=1:nokey=1 -show_entries stream=r_frame_rate \"" + filePath.Replace('\\', '/') + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            ffprobe.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                if (e.Data != null)
                {
                    Debug.WriteLine(e.Data);
                    try
                    {
                        fps = e.Data.Trim();
                    }
                    catch
                    {
                        Debug.WriteLine("Error getting fps of the video...");
                        if (ffprobe != null)
                            ffprobe.Dispose();
                    }
                }
            });

            ffprobe.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                if (e.Data != null)
                {
                    Debug.WriteLine(e.Data);
                    try
                    {
                        fps = e.Data.Trim();
                    }
                    catch
                    {
                        Debug.WriteLine("Error getting fps of the video...");
                        if (ffprobe != null)
                            ffprobe.Dispose();
                    }
                }
            });

            ffprobe.Start();
            ffprobe.BeginOutputReadLine();
            ffprobe.BeginErrorReadLine();
            ffprobe.WaitForExit();

            return fps;
        }

        private int GetVideoFrameNum(string filePath)
        {
            int frames = 0;

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

            return frames;
        }

        private void ExtractFrames(string filePath, string folder)
        {
            Debug.WriteLine($"Extracting frame from: {filePath}, to directory: {folder}");

            string frameDir = workdir + "\\" + folder;
            string fps = GetVideoFPS(filePath);

            if (Directory.Exists(frameDir))
                Directory.Delete(frameDir, true);
            Directory.CreateDirectory(frameDir);

            string[] files = Directory.GetFiles(frameDir, "*", SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                DirectoryInfo di = new DirectoryInfo(workdir + "\\" + folder);
                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.EnumerateDirectories())
                {
                    dir.Delete(true);
                }
            }

            string arg = $"-i \"{filePath.Replace('\\', '/')}\" -vf fps={fps} -qscale:v 2 \"" + frameDir.Replace('\\', '/') + "/%05d.jpg\"";

            var process = new Process
            {
                StartInfo =
                {
                    FileName = workdir + @"\ffmpeg\bin\ffmpeg.exe",
                    Arguments = arg,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                }
            };

            process.Start();
            process.WaitForExit();
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
                duration = TimeSpan.FromTicks(GetVideoDurationTuple(fullPath).Item3).ToString(@"dd\:hh\:mm\:ss");
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

            var converter = new BrushConverter();
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

            BtnRegisterThisFile.Background = (Brush)(new BrushConverter()).ConvertFromString("#7F32A0AB");
            BtnRegisterThisFile.Click -= BtnRegisterThisFile_Click2;
            BtnRegisterThisFile.Click += BtnRegisterThisFile_Click;

            ChangeStateText("RegisterThisFile", true);
            TextTitleRegisterNew.Text = Application.Current.FindResource("TitleRegisterNew") as String;
        }
    }
}
