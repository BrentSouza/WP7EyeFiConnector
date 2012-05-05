using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.IO.IsolatedStorage;
using System.IO;

namespace EyeFiConnector
{
    public partial class Pictures : PhoneApplicationPage
    {
        public CollectionViewSource AllImages, FavoriteImages, DatedImages;

        public Pictures()
        {
            this.Loaded += new RoutedEventHandler(Pictures_Loaded);
            InitializeComponent();
        }

        void Pictures_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsImagesLoaded)
            {
                LoadImageList();
                lbAllImages.ItemsSource = App.ViewModel.ImageCollection;
            }
        }

        private void LoadImageList()
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string[] files = store.GetFileNames(System.IO.Path.Combine(App.ViewModel.ThumbnailLocation, "*.JPG"));               

                Random favGen = new Random();

                for (int i = 0; i < files.Length; i++)
                {
                    string fullPath = System.IO.Path.Combine(App.ViewModel.ThumbnailLocation, files[i]);

                    DateTimeOffset fileDate = store.GetCreationTime(fullPath);

                    using (IsolatedStorageFileStream thumbStream = store.OpenFile(fullPath, FileMode.Open))
                    {
                        ImageItem ii = new ImageItem();
                        ii.FileDate = fileDate.DateTime;
                        ii.IsFavorite = favGen.Next() % 2 == 0 ? true : false;
                        ii.FullImagePath = System.IO.Path.Combine(App.ViewModel.DownloadLocation,files[i]);

                        BitmapImage bi = new BitmapImage();
                        bi.SetSource(thumbStream);
                        ii.ThumbSource = bi;

                        App.ViewModel.ImageCollection.Add(ii);
                    }
                }
            }

            App.ViewModel.IsImagesLoaded = true;
        }

        private void lbAllImages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            ImageItem ii = e.AddedItems[0] as ImageItem;
            int selection = lb.SelectedIndex;

            lb.SelectionChanged -= lbAllImages_SelectionChanged;
            lb.SelectedIndex = -1;
            lb.SelectionChanged += lbAllImages_SelectionChanged;

            NavigationService.Navigate(new Uri(string.Format("/PictureViewer.xaml?SelectedItem={0}", selection), UriKind.Relative));
        }


    }
}