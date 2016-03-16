using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NAudio.Wave;
using Microsoft.Win32;
using System.Net.Http;
using System.Net.Http.Headers;
using AcousticFingerprintingLibrary;

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
        private WaveIn sourceStream = null;
        private DirectSoundOut waveOut = null;
        private WaveFileWriter waveWriter = null;
        List<WaveInCapabilities> sources = new List<WaveInCapabilities>();

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            {
                Main.Status = "Refreshing input sources";
                // Creates a list that holds all in audio devices
                

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
                    sourceList.Items.Add(new MyItem { Device = source.ProductName, Channels = source.Channels.ToString()});
                }
            }

        }

        public class MyItem
        {
            public string Device { get; set; }

            public string Channels { get; set; }
        }

        private void sourceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sourceList.SelectedItem = this;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            // Make sure at least one source is selected
            // Make sure the count is not zero.
            if (sourceList.SelectedItems.Count == 0) return;

            // Gets the device number of the selected source
            // ask sourcelist for the first selected item
            int deviceNumber = sourceList.SelectedIndex;
            Console.WriteLine("Device number:" + deviceNumber);

            sourceStream = new WaveIn(); // sets up the sourceStream
            sourceStream.DeviceNumber = deviceNumber; // Set the devicenumber
            // Set a wave format and sets the sampleRate and asks how many channels are available to the device
            sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(44100,
                NAudio.Wave.WaveIn.GetCapabilities(deviceNumber).Channels);

            // Creates a WaveInProvider to bridge the gap between the 
            // waveIn object and the DirectSoundOUt
            WaveInProvider waveIn = new NAudio.Wave.WaveInProvider(sourceStream);

            // Create wave out device
            waveOut = new NAudio.Wave.DirectSoundOut();
            // initialize the wave out 
            waveOut.Init(waveIn);

            Main.Status = "Starting recording with playback.";
            // Creates a buffer 
            sourceStream.StartRecording();
            // Playback
            waveOut.Play();
        }

        private void toWaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Make sure at least one source is selected
            // Make sure the count is not zero.
            if (sourceList.SelectedItems.Count == 0) return;

            // opens a save file window
            SaveFileDialog save = new SaveFileDialog();
            // Sets a filter to only save .wav files
            save.Filter = "Wave File (*.wav)|*.wav*";
            
            if (save.ShowDialog() != true) return;

            // Gets the device number of the selected source
            // ask sourcelist for the first selected item
            int deviceNumber = sourceList.SelectedIndex;
 

            Console.WriteLine("Device number:" + deviceNumber);

            sourceStream = new WaveIn(); // sets up the sourceStream
            sourceStream.DeviceNumber = deviceNumber; // Set the devicenumber
            // Set a wave format and sets the sampleRate and asks how many channels are available to the device
            sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(44100,
                NAudio.Wave.WaveIn.GetCapabilities(deviceNumber).Channels);
            sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(5512, 1);
            sourceStream.DataAvailable += new EventHandler<NAudio.Wave.WaveInEventArgs>(sourceStream_DataAvailable);
            waveWriter = new NAudio.Wave.WaveFileWriter(save.FileName, sourceStream.WaveFormat);
            Main.Status = "Starting recording to wave file without playback.";
            sourceStream.StartRecording();
        }

        private void sourceStream_DataAvailable(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            if (waveWriter == null) return;

            // eventuelt deprecated function: waveWriter.WriteData(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Flush();
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            {
                using (var client = new HttpClient())
                {

                    Main.Status = "Sending HTTP GET.";
                    client.BaseAddress = new Uri("http://localhost:58293/Fingerprints/GetFingerprintsByTitle?inputTitle=Braveheart%20Trailer");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // HTTP GET
                    HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                    Main.Status = "Response header: " + response.Content.Headers;
                    Console.WriteLine(response.Content.Headers);

                }
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {

            Main.Status = "Stopping and disposing recording.";
            // check if it is playing if it is then stop it and dispose of it
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            // check if the sourceStream is Recording, if so then stop recording 
            if (sourceStream != null)
            {
                sourceStream.StopRecording();
                sourceStream.Dispose();
                sourceStream = null;
            }

            if (waveWriter != null)
            {
                waveWriter.Dispose();
                waveWriter = null;
            }
        }
        internal string Status
        {
            get { return label.Content.ToString(); }
            set
            {
                Dispatcher.Invoke(() =>
                {
                    label.Content += value + "\n";
                });
            }
        }
    }
}
