using System;
using System.Threading;
using UIKit;

namespace iOSApplication_0._4._5
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            Thread newThread = new Thread(Record.RunRecord);

            // Event handler for simple "Record" button click and release.
            RecordButton.TouchUpInside += (sender, e) => {
                try {
                    if (newThread.ThreadState == ThreadState.Running) {
                        Console.WriteLine("Thread is already running.");
                    }
                    else
                    {
                        Record.InitializeComponents();
                        foreground_label.Text = "Recording . .";
                        newThread.Start();
                    }
                }
                catch(ThreadStateException)
                {
                    Console.WriteLine("The recorder is already running.");
                }
            };

            // Event handler for simple "Stop" button click and release.
            StopButton.TouchUpInside += (sender, e) =>
            {
                foreground_label.Text = "Stopped recording.";
                newThread.Abort();  
            };
        }
    }
}