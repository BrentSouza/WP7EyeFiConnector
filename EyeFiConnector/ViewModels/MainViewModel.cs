using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.IO;


namespace EyeFiConnector
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
        }

        #region Properties
        
        private string _eyeFiUploadKey;
        public string EyeFiUploadKey
        {
            get
            {
                return _eyeFiUploadKey;
            }
            set
            {
                if (value != _eyeFiUploadKey)
                {
                    _eyeFiUploadKey = value;
                    NotifyPropertyChanged("EyeFiUploadKey");
                }
            }
        }
        
        private int _eyeFiFileId;
        public int EyeFiFileId
        {
            get { return _eyeFiFileId; }
            set
            {
                if (value != _eyeFiFileId)
                {
                    _eyeFiFileId = value;
                    NotifyPropertyChanged("EyeFiFileId");
                }
            }
        }

        private BitmapImage _backgroundImage;
        public BitmapImage BackgroundImage
        {
            get { return _backgroundImage; }
            set
            {
                if (value != _backgroundImage)
                {
                    _backgroundImage = value;
                    NotifyPropertyChanged("BackgroundImage");
                }
            }
        }

        private ObservableCollection<NavigationItem> _navigationItems = new ObservableCollection<NavigationItem>();
        public ObservableCollection<NavigationItem> NavigationItems
        {
            get { return _navigationItems; }
            set
            {
                if (value != _navigationItems)
                {
                    _navigationItems = value;
                    NotifyPropertyChanged("NavigationItems");
                }
            }
        }

        private ObservableCollection<ImageItem> _imageCollection = new ObservableCollection<ImageItem>();
        public ObservableCollection<ImageItem> ImageCollection
        {
            get { return _imageCollection; }
            set
            {
                if (value != _imageCollection)
                {
                    _imageCollection = value;
                    NotifyPropertyChanged("ImageCollection");
                }
            }
        }

        private ObservableCollection<BitmapImage> _recentPictures = new ObservableCollection<BitmapImage>();
        public ObservableCollection<BitmapImage> RecentPictures
        {
            get { return _recentPictures; }
            set
            {
                if (value != _recentPictures)
                {
                    _recentPictures = value;
                    NotifyPropertyChanged("RecentPictures");
                }
            }
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public bool IsImagesLoaded
        {
            get;
            set;
        }

        #endregion
        
        #region Fields

        public EyeFiServer EyeFiServerInstance;
        public string DownloadLocation = "PicsToUpload";
        public string ThumbnailLocation = "Thumbnails";

        #endregion

        public void LoadData()
        {
            _eyeFiUploadKey = "94e6bd6b3b655857acf868a4be0cc3ac";
            _eyeFiFileId = 1;

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string[] files = store.GetFileNames(System.IO.Path.Combine(ThumbnailLocation, "*"));

                Random rand = new Random();
                int bgIndex = rand.Next(0, files.Length);
                _backgroundImage = new BitmapImage();
                _backgroundImage.CreateOptions = BitmapCreateOptions.BackgroundCreation;

                using (IsolatedStorageFileStream bgImageStream = store.OpenFile(System.IO.Path.Combine(ThumbnailLocation, files[bgIndex]), FileMode.Open))
                {
                    _backgroundImage.SetSource(bgImageStream);
                }

                Array.Reverse(files);

                for (int i = 0; i < (files.Length > 8 ? 8 : files.Length); i++)
                {
                    using (IsolatedStorageFileStream imageStream = store.OpenFile(System.IO.Path.Combine(ThumbnailLocation, files[i]), FileMode.Open))
                    {
                        BitmapImage bm = new BitmapImage();
                        bm.CreateOptions = BitmapCreateOptions.BackgroundCreation;
                        bm.SetSource(imageStream);
                        App.ViewModel.RecentPictures.Add(bm);
                    }
                }
            }

            _navigationItems.Add(new NavigationItem { Name = "pics", Uri = new Uri("/Pictures.xaml", UriKind.Relative) });
            _navigationItems.Add(new NavigationItem { Name = "upload", Uri = new Uri("/Settings.xaml", UriKind.Relative) });
            _navigationItems.Add(new NavigationItem { Name = "settings", Uri = new Uri("/Settings.xaml", UriKind.Relative) });
            _navigationItems.Add(new NavigationItem { Name = "about", Uri = new Uri("/Settings.xaml", UriKind.Relative) });

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.DirectoryExists(DownloadLocation))
                    store.CreateDirectory(DownloadLocation);

                if (!store.DirectoryExists(ThumbnailLocation))
                    store.CreateDirectory(ThumbnailLocation);
            }

            EyeFiServerInstance = new EyeFiServer();

            this.IsDataLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}