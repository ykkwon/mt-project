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

namespace iOSApplication_0._5._3
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel ForegroundLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton GetFingerprintsButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton IndexButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton MoviePicker { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton RecordButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton StopButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton TestButton { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (ForegroundLabel != null) {
				ForegroundLabel.Dispose ();
				ForegroundLabel = null;
			}
			if (GetFingerprintsButton != null) {
				GetFingerprintsButton.Dispose ();
				GetFingerprintsButton = null;
			}
			if (IndexButton != null) {
				IndexButton.Dispose ();
				IndexButton = null;
			}
			if (MoviePicker != null) {
				MoviePicker.Dispose ();
				MoviePicker = null;
			}
			if (RecordButton != null) {
				RecordButton.Dispose ();
				RecordButton = null;
			}
			if (StopButton != null) {
				StopButton.Dispose ();
				StopButton = null;
			}
			if (TestButton != null) {
				TestButton.Dispose ();
				TestButton = null;
			}
		}
	}
}
