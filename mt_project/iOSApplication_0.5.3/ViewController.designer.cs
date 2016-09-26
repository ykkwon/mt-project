// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using AcousticFingerprintingLibrary_0._4._5;
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
		UIButton LongRecordButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton MoviePicker { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton RecordButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton StopButton { get; set; }

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
			if (LongRecordButton != null) {
				LongRecordButton.Dispose ();
				LongRecordButton = null;
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
		}
	}
}
