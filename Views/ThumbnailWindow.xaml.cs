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

namespace C3.Views
{
    /// <summary>
    /// ThumbnailWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ThumbnailWindow : Window
    {
        public ThumbnailWindow()
        {
            InitializeComponent();
        }

        private void BtnThumbnailOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnThumbnailOther_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public void setImgSource(string fullPath)
        {
            ImageThumbnail.Source = (ImageSource)(new ImageSourceConverter()).ConvertFromString(fullPath);
        }
    }
}
