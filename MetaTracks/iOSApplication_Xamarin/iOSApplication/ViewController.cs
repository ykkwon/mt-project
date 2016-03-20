using System;

using UIKit;
using Un4seen.Bass;

namespace iOSApplication
{
	public partial class ViewController : UIViewController
	{
		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			BassNet.Registration ("kristian.stoylen93@gmail.com", "2X20371028152222");
			Bass.BASS_Init (-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
			var ver = Bass.BASSVERSION;
			Console.WriteLine ("Initialization successful");
			Console.WriteLine ("Bass version: " + ver);
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}

