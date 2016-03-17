using System;

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

            RecordButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                for (int i = 0; i < 100; i++)
                {
                    Record.PrepareRecording(i);
                }
            };

            StopButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                Record.StopRecording();
            };
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}