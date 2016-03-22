using AVFoundation;
using System;
using Un4seen.Bass;

namespace iOSApplication_0._4._5
{
    public static class Record
    {
        internal static void RunRecord()
        {
            Console.WriteLine("TODO: Implement RunRecord methods.");
        }

        public static void InitializeComponents()
        {
            // Register Bass.NET license
            BassNet.Registration("kristian.stoylen93@gmail.com", "2X20371028152222");
            // Initialize BASS 
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            // Create a new native iOS audio session
            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
        }
    }
}