using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using NAudio;
using NAudio.Wave;

namespace WindowsApplication_0._4._5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Main = this;
            InitializeComponent();
            MouseDown += delegate { DragMove(); };
        }
        internal static MainWindow Main;

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            {
                // Creates a list that holds all in audio devices
                List<NAudio.Wave.WaveInCapabilities> sources = new List<WaveInCapabilities>();

                // Loops over all audio devices
                for (int i = 0; i < WaveIn.DeviceCount; i++)
                {
                    // populates the list with audio devices
                    sources.Add(WaveIn.GetCapabilities(i));
                }

                // Clears the source list to make sure we start from a blank slate
                sourceList.Items.Clear();

                // Adds each resource to the listView in the form
                foreach (var source in sources)
                {
                    sourceList.Items.Add(new MyItem { Device = source.ProductName, Channels = source.ProductGuid.ToString() });
                    //ListViewItem item = new ListViewItem(source.ProductName);
                    //item.SubItems.Add(new ListViewItem.ListViewSubItem(item, source.Channels.ToString()));
                    //sourceList.Items.Add(item);
                }
            }

        }
        public class MyItem
        {
            public string Device { get; set; }

            public string Channels { get; set; }
        }
    }
}
