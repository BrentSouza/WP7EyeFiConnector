using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;

namespace EyeFiConnector
{
    public class ImageItem
    {
        public string FullImagePath { get; set; }
        public BitmapImage ThumbSource { get; set; }
        public DateTime FileDate { get; set; }
        public bool IsFavorite { get; set; }

        public BitmapImage GetFullImage()
        {
            BitmapImage bm = new BitmapImage();

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream imageStream = store.OpenFile(FullImagePath, FileMode.Open))
                {
                    bm.SetSource(imageStream);
                }
            }

            return bm;
        }
    }
}
