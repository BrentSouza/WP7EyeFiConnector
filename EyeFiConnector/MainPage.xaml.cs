using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using Microsoft.Phone;
using System.Threading.Tasks;
using System.Windows.Resources;
using Microsoft.Xna.Framework.Media;

namespace EyeFiConnector
{
    public partial class MainPage : PhoneApplicationPage
    {

        private bool _hasBackgroundChanged = true;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            App.ViewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ViewModel_PropertyChanged);
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_hasBackgroundChanged)
            {
                ChangeBackground();
            }
        }

        void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BackgroundImage")
            {
                _hasBackgroundChanged = true;
                ChangeBackground();
            }
        }

        private void ChangeBackground()
        {
            ImageBrush ib = new ImageBrush();
            ib.ImageSource = App.ViewModel.BackgroundImage;
            ib.Stretch = Stretch.UniformToFill;

            this.cpPanorama.Background = ib;

            _hasBackgroundChanged = false;
        }

        private void lbNavigation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NavigationItem item = e.AddedItems[0] as NavigationItem;
            ListBox lb = sender as ListBox;
            lb.SelectionChanged -= lbNavigation_SelectionChanged;
            lb.SelectedIndex = -1;
            lb.SelectionChanged += lbNavigation_SelectionChanged;
            NavigationService.Navigate(item.Uri);
        }
    }
}