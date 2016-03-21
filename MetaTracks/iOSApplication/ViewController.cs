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
            BassNet.Registration("kristian.stoylen93@gmail.com", "2X20371028152222");
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            Console.Text = "Press Record to start.";
            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
            Thread newThread = new Thread(Record.RunRecord);

            RecordButton.TouchUpInside += (object sender, EventArgs e) => {
                newThread.Start();
                Console.Text = "Recording . . .";
            };


            StopButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                newThread.Abort();
                Console.Text = "Recording stopped.";
            };
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}