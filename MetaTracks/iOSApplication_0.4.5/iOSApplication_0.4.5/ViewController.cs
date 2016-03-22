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
            Record.InitializeComponents();
            Thread newThread = new Thread(Record.RunRecord);

            // Event handler for simple "Record" button click and release.
            RecordButton.TouchUpInside += (sender, e) => {
                if (newThread.ThreadState == ThreadState.Running) {
                    Console.WriteLine("Thread is already running.");
                }
                else
                {
                    newThread.Start();
                }
            };

            // Event handler for simple "Stop" button click and release.
            StopButton.TouchUpInside += (sender, e) =>
            {
                if (newThread.ThreadState == ThreadState.Running)
                {
                    newThread.Abort();
                }
                else
                {
                    Console.WriteLine("Record is not running.");
                }
               
            };
        }
    }
}