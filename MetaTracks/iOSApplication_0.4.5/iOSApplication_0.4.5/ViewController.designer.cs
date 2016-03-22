// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace iOSApplication_0._4._5
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton RecordButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton StopButton { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (RecordButton != null) {
				RecordButton.Dispose ();
				RecordButton = null;
			}
			if (StopButton != null) {
				StopButton.Dispose ();
				StopButton = null;
			}
		}
	}
}
