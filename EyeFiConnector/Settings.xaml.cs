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
using System.IO.IsolatedStorage;
using System.IO;

namespace EyeFiConnector
{
    public partial class Settings : PhoneApplicationPage
    {

        long logIndex = 0;

        public Settings()
        {
            InitializeComponent();
        }

        private void btnReadLog_Click(object sender, RoutedEventArgs e)
        {
            Utilities.WaitForLog.WaitOne();
            // Obtain a virtual store for the application.
            IsolatedStorageFile myStore = IsolatedStorageFile.GetUserStoreForApplication();

            try
            {
                // Specify the file path and options.
                using (var isoFileStream = new IsolatedStorageFileStream("log.txt", FileMode.Open, myStore))
                {
                    isoFileStream.Seek(logIndex, SeekOrigin.Begin);
                    // Read the data.
                    using (var isoFileReader = new StreamReader(isoFileStream))
                    {
                        while (isoFileReader.Peek() >= 0)
                        {
                            lbLogDump.Items.Add(isoFileReader.ReadLine());
                            logIndex = isoFileStream.Position;
                        }
                    }
                }
            }
            catch
            {
                // Handle the case when the user attempts to click the Read button first.
                lbLogDump.Items.Add("Need to create directory and the file first.");
            }
            finally
            {
                Utilities.WaitForLog.Set();
            }
        }

        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            Utilities.Log("", true);
            logIndex = 0;
            lbLogDump.Items.Clear();
        }
    }
}