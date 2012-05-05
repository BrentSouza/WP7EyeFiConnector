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

namespace EyeFiConnector
{
    public partial class PictureViewer : PhoneApplicationPage
    {
        private int _selectedItem;
        double _initialAngle, _initialScale;
        double _currentPageX, _currentPageY;
        private bool _isZoomed = false;

        public PictureViewer()
        {
            InitializeComponent();
            this.DataContext = App.ViewModel.ImageCollection;
            this.Loaded += new RoutedEventHandler(PictureViewer_Loaded);
            this.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(PictureViewer_OrientationChanged);
        }

        void PictureViewer_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.Portrait || e.Orientation == PageOrientation.PortraitUp || e.Orientation == PageOrientation.PortraitDown)
            {
                this.ViewerHorizontal.Visibility = Visibility.Collapsed;
                this.ViewerPortrait.Visibility = Visibility.Visible;
            }
            else
            {
                this.ViewerPortrait.Visibility = Visibility.Collapsed;
                this.ViewerHorizontal.Visibility = Visibility.Visible;
            }
        }

        void PictureViewer_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Orientation == PageOrientation.Portrait || this.Orientation == PageOrientation.PortraitUp || this.Orientation == PageOrientation.PortraitDown)
            {
                this.ViewerHorizontal.Visibility = Visibility.Collapsed;
                this.ViewerPortrait.Visibility = Visibility.Visible;

                this.LayoutRoot.Width = App.ViewModel.ImageCollection.Count * 480d;
                this.Viewer.SetValue(Canvas.LeftProperty, _selectedItem * 480d);
                
            }
            else
            {
                this.ViewerPortrait.Visibility = Visibility.Collapsed;
                this.ViewerHorizontal.Visibility = Visibility.Visible;

                this.LayoutRoot.Width = App.ViewModel.ImageCollection.Count * 768d;
                this.Viewer.SetValue(Canvas.LeftProperty, _selectedItem * 768d);
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            _selectedItem = int.Parse(NavigationContext.QueryString["SelectedItem"]);
            base.OnNavigatedTo(e);
        }

        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            Image i = sender as Image;
            CompositeTransform transform = i.RenderTransform as CompositeTransform;

            _initialAngle = transform.Rotation;
            _initialScale = transform.ScaleX;

        }

        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            Image i = sender as Image;
            CompositeTransform transform = i.RenderTransform as CompositeTransform;

            transform.ScaleX = _initialScale * e.DistanceRatio;
            transform.ScaleY = _initialScale * e.DistanceRatio;

            if (transform.ScaleX > 1d)
                _isZoomed = true;
            else if (i.ActualWidth < 1d)
                //zoom out the whole set of images
                _isZoomed = true;
            else
                _isZoomed = false;
        }

        private void OnImageDragStarted(object sender, DragStartedGestureEventArgs e)
        {
            
        }

        private void OnImageDragDelta(object sender, DragDeltaGestureEventArgs e)
        {

        }

        private void OnPageDragStarted(object sender, DragStartedGestureEventArgs e)
        {
            Grid g = sender as Grid;
            _currentPageX = Canvas.GetLeft(g);
            _currentPageY = Canvas.GetTop(g);
        }

        private void OnPageDragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            Grid g = sender as Grid;

            g.SetValue(Canvas.LeftProperty, _currentPageX + e.HorizontalChange);
            _currentPageX += e.HorizontalChange;

            if (_isZoomed)
            {
                g.SetValue(Canvas.TopProperty, _currentPageY + e.VerticalChange);
                _currentPageY += e.VerticalChange;
            }
        }
    }
}