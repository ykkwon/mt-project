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
	partial class NavigationController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel foreground_label { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton RecordButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UISearchDisplayController searchDisplayController { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton StopButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		NavigationController ViewController { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (foreground_label != null) {
				foreground_label.Dispose ();
				foreground_label = null;
			}
			if (RecordButton != null) {
				RecordButton.Dispose ();
				RecordButton = null;
			}
			if (searchDisplayController != null) {
				searchDisplayController.Dispose ();
				searchDisplayController = null;
			}
			if (StopButton != null) {
				StopButton.Dispose ();
				StopButton = null;
			}
			if (ViewController != null) {
				ViewController.Dispose ();
				ViewController = null;
			}
		}
	}
}