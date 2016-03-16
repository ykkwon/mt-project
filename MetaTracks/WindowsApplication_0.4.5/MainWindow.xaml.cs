using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NAudio.Wave;
using Microsoft.Win32;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace WindowsApplication_0._4._5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            Main = this;
            InitializeComponent();
            MouseDown += delegate { DragMove(); };
        }

        private string _entryName;
        internal static MainWindow Main;
        private WaveIn _sourceStream;
        private DirectSoundOut _waveOut;
        private WaveFileWriter _waveWriter;
        List<WaveInCapabilities> sources = new List<WaveInCapabilities>();
        private dynamic _json;

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
            Console.WriteLine(@"Device number:" + deviceNumber);

            _sourceStream = new WaveIn(); // sets up the sourceStream
            _sourceStream.DeviceNumber = deviceNumber; // Set the devicenumber
            // Set a wave format and sets the sampleRate and asks how many channels are available to the device
            _sourceStream.WaveFormat = new WaveFormat(44100,
                WaveIn.GetCapabilities(deviceNumber).Channels);

            // Creates a WaveInProvider to bridge the gap between the 
            // waveIn object and the DirectSoundOUt
            WaveInProvider waveIn = new WaveInProvider(_sourceStream);

            // Create wave out device
            _waveOut = new DirectSoundOut();
            // initialize the wave out 
            _waveOut.Init(waveIn);

            Main.Status = "Starting recording with playback.";
            // Creates a buffer 
            _sourceStream.StartRecording();
            // Playback
            _waveOut.Play();
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
 

            Console.WriteLine(@"Device number:" + deviceNumber);

            _sourceStream = new WaveIn(); // sets up the sourceStream
            _sourceStream.DeviceNumber = deviceNumber; // Set the devicenumber
            // Set a wave format and sets the sampleRate and asks how many channels are available to the device
            _sourceStream.WaveFormat = new WaveFormat(44100,
                WaveIn.GetCapabilities(deviceNumber).Channels);
            _sourceStream.WaveFormat = new WaveFormat(5512, 1);
            _sourceStream.DataAvailable += sourceStream_DataAvailable;
            _waveWriter = new WaveFileWriter(save.FileName, _sourceStream.WaveFormat);
            Main.Status = "Starting recording to wave file without playback.";
            _sourceStream.StartRecording();
        }

        private void sourceStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_waveWriter == null) return;

            // eventuelt deprecated function: waveWriter.WriteData(e.Buffer, 0, e.BytesRecorded);
            _waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            _waveWriter.Flush();
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            {
                var dialog = new Popup();
                if (dialog.ShowDialog() == true)
                {
                    _entryName = dialog.ResponseText;
                }

                using (var client = new HttpClient())
                {
                    var inputString =
                    String.Format("http://localhost:58293/Fingerprints/GetSingleFingerprintByHash?inputHash=" + "{0}",
                    _entryName);
                    Main.Status = "Sending HTTP GET.";
                    client.BaseAddress = new Uri(inputString);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // HTTP GET
                    HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    Main.Status = response.Content.Headers.ContentLength != 0 ? "Found a match." : "No match.";
                    _json = JsonConvert.DeserializeObject(responseString, typeof(object));
                    Main.Status = "JSON: " + _json;
                }
            }
            
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {

            Main.Status = "Stopping and disposing recording.";
            // check if it is playing if it is then stop it and dispose of it
            if (_waveOut != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = null;
            }

            // check if the sourceStream is Recording, if so then stop recording 
            if (_sourceStream != null)
            {
                _sourceStream.StopRecording();
                _sourceStream.Dispose();
                _sourceStream = null;
            }

            if (_waveWriter != null)
            {
                _waveWriter.Dispose();
                _waveWriter = null;
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
