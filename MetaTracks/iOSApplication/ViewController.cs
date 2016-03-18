using AVFoundation;
using System;
using System.Threading;
using UIKit;

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
            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
            Thread newThread = new Thread(Record.RunRecord);

            RecordButton.TouchUpInside += (object sender, EventArgs e) => { 
                newThread.Start();
            };
    

            StopButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                newThread.Abort();
            };
        }



        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}