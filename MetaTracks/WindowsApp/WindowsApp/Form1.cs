using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace WindowsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        
        // Refresh Sources button function
        // will find any in audio sources that exists and its capabilities
        // like name and how many channels it has
        private void button1_Click(object sender, EventArgs e)
        {
            // Creates a list that holds all in audio devices
            List<NAudio.Wave.WaveInCapabilities> sources = new List<WaveInCapabilities>();

            // Loops over all audio devices
            for (int i = 0; i < NAudio.Wave.WaveIn.DeviceCount; i++)
            {
                // populates the list with audio devices
                sources.Add(NAudio.Wave.WaveIn.GetCapabilities(i));
            }

            // Clears the source list to make sure we start from a blank slate
            sourceList.Items.Clear();

            // Adds each resource to the listView in the form
            foreach (var source in sources)
            {
                ListViewItem item = new ListViewItem(source.ProductName);
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, source.Channels.ToString()));
                sourceList.Items.Add(item);
            }
        }

        // Source like a mic
        private NAudio.Wave.WaveIn sourceStream = null;
        // A sync
        private NAudio.Wave.DirectSoundOut waveOut = null;
        // field for waveWriter
        private NAudio.Wave.WaveFileWriter waveWriter = null;
        //private byte[] buffer = new byte[] {};

        // Start button function
        private void button2_Click(object sender, EventArgs e)
        {
            // Make sure at least one source is selected
            // Make sure the count is not zero.
            if (sourceList.SelectedItems.Count == 0) return;

            // Gets the device number of the selected source
            // ask sourcelist for the first selected item
            int deviceNumber = sourceList.SelectedItems[0].Index;

            sourceStream = new NAudio.Wave.WaveIn(); // sets up the sourceStream
            sourceStream.DeviceNumber = deviceNumber;  // Set the devicenumber
            // Set a wave format and sets the sampleRate and asks how many channels are available to the device
            sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(44100, NAudio.Wave.WaveIn.GetCapabilities(deviceNumber).Channels); 

            // Creates a WaveInProvider to bridge the gap between the 
            // waveIn object and the DirectSoundOUt
            NAudio.Wave.WaveInProvider waveIn = new NAudio.Wave.WaveInProvider(sourceStream);

            // Create wave out device
            waveOut = new NAudio.Wave.DirectSoundOut();
            // initialize the wave out 
            waveOut.Init(waveIn);

            // Creates a buffer 
            sourceStream.StartRecording();
            // Playback
            waveOut.Play();
        }

        // To wave button function
        private void button5_Click(object sender, EventArgs e)
        {
            // Make sure at least one source is selected
            // Make sure the count is not zero.
            if (sourceList.SelectedItems.Count == 0) return;

            // opens a save file window
            SaveFileDialog save = new SaveFileDialog();
            // Sets a filter to only save .wav files
            save.Filter = "Wave File (*.wav)|*.wav*";
            if (save.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            // Gets the device number of the selected source
            // ask sourcelist for the first selected item
            int deviceNumber = sourceList.SelectedItems[0].Index;

            sourceStream = new NAudio.Wave.WaveIn(); // sets up the sourceStream
            sourceStream.DeviceNumber = deviceNumber;  // Set the devicenumber
            // Set a wave format and sets the sampleRate and asks how many channels are available to the device
            sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(44100, NAudio.Wave.WaveIn.GetCapabilities(deviceNumber).Channels);
            //sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(5512, 1);
            sourceStream.DataAvailable += new EventHandler<NAudio.Wave.WaveInEventArgs>(sourceStream_DataAvailable);
            waveWriter = new NAudio.Wave.WaveFileWriter(save.FileName, sourceStream.WaveFormat);

            sourceStream.StartRecording();
        }

        private void sourceStream_DataAvailable(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            if (waveWriter == null) return;

            // eventuelt deprecated function: waveWriter.WriteData(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Flush();
        }


        // Stop button function
        private void button3_Click(object sender, EventArgs e)
        {
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

        // exit button function
        private void button4_Click(object sender, EventArgs e)
        {
            // call button3_click to make sure it has stopped both recording and output
            button3_Click(sender, e);
            // Closes the form
            this.Close();
        }


    }
}
