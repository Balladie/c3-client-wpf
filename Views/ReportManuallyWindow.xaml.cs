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
using VideoLibrary;
using HiddenWatermark;
using System.Windows.Threading;
using System.Threading;

namespace C3.Views
{
    /// <summary>
    /// ReportManuallyWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ReportManuallyWindow : Window
    {
        private static string workdir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        private static string registeredAt = "";

        private static Watermark _watermark;

        public ReportManuallyWindow()
        {
            InitializeComponent();

            _watermark = new Watermark(true);
        }

        public void setRegisteredAt(string value)
        {
            registeredAt = value;
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

            string arg = $"-i \"{filePath.Replace('\\', '/')}\" -vf fps={fps} -qscale:v 2 \"" + frameDir.Replace('\\', '/') + "/%05d.png\"";

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
                string url = TextBox_ReportManually.Text;
                string videoPath = "";
                double similarity = 0;
                int successCount = 0;
                double maxScore = 0;
                List<byte[]> detects = new List<byte[]>();
                byte[] hardestDetected = new byte[0];
                bool detectSuccess = false;

                var youtube = YouTube.Default;

                if (!(url.Contains("http://") || url.Contains("https://")))
                    url = "https://" + url;

                Debug.WriteLine(url);

                // download youtube video from url
                try
                {
                    var videos = youtube.GetAllVideos(url);
                    var video = videos.FirstOrDefault(v => v.Resolution == 1080);
                    string videoName = video.FullName;
                    videoPath = workdir + "\\" + videoName;
                    File.WriteAllBytes(videoPath, video.GetBytes());
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                    Close();
                }

                // Extract frames
                ExtractFrames(videoPath, "frames_retrieve");
                string[] files = Directory.GetFiles(workdir + "\\frames_retrieve", "*.png", SearchOption.AllDirectories);

                var sw = Stopwatch.StartNew();

                // retrieves watermark for each frame
                for (int i = 0; i < files.Length; ++i)
                {
                    string filename = files[i].Trim().Substring(files[i].LastIndexOf('\\') + 1);
                    var fileBytes = File.ReadAllBytes(files[i]);
                    var result = _watermark.RetrieveWatermark(fileBytes);

                    if (result.WatermarkDetected)
                    {
                        Debug.WriteLine("!!! DETECTED !!!");

                        successCount++;
                        similarity += result.Similarity;

                        detectSuccess = true;

                        if (result.Similarity > maxScore)
                        {
                            maxScore = result.Similarity;
                            hardestDetected = result.RecoveredWatermark;
                        }

                        detects.Add(result.RecoveredWatermark);
                    }

                    string retrievePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\frames_retrieve\\wm_" + filename;
                    File.WriteAllBytes(retrievePath, result.RecoveredWatermark);
                }

                sw.Stop();

                if (successCount > 0)
                    similarity /= successCount;

                if (detectSuccess)
                    MessageBox.Show(String.Format("Similarity: {0}%", similarity));
                else
                    MessageBox.Show("Watermark not detected.");

                RenderImageBytes(RetrievedWatermark, hardestDetected);
            }
        }

        private void RenderImageBytes(Image control, byte[] bytes)
        {
            MemoryStream byteStream = new MemoryStream(bytes);
            BitmapImage imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = byteStream;
            imageSource.EndInit();

            control.Source = imageSource;
        }
    }
}
