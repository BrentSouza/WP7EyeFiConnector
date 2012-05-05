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

namespace EyeFiConnector
{

    public class PictureReceivedEventArgs : EventArgs
    {
        private string _imagePath;
        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                if (value != _imagePath)
                    _imagePath = value;
            }
        }

        public PictureReceivedEventArgs(string imagePath)
        {
            _imagePath = imagePath;
        }
    }
}
