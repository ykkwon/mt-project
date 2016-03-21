using AVFoundation;
using System;
using System.Threading;
using UIKit;
using Un4seen.Bass;

namespace iOSApplication
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }
       
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Register Bass.NET license
            BassNet.Registration("kristian.stoylen93@gmail.com", "2X20371028152222");
            // Initialize BASS 
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            Console.Text = "Press Record to start.";
            // Create a new native iOS audio session
            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
            Thread newThread = new Thread(Record.RunRecord);

            // Event handler for simple "Record" button click and release.
            RecordButton.TouchUpInside += (sender, e) => {
                newThread.Start();
                Console.Text = "Recording . . .";
            };

            // Event handler for simple "Stop" button click and release.
            StopButton.TouchUpInside += (sender, e) =>
            {
                newThread.Abort();
                Console.Text = "Recording stopped.";
            };
        }
    }
}