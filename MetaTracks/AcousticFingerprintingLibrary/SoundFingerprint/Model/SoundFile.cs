using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcousticFingerprintingLibrary.SoundFingerprint.Model
{
    public class SoundFile
    {
        #region Constants

        /// <summary>
        ///   Maximum artist's length
        /// </summary>
        private const int MAX_ARTIST_LENGTH = 255;

        /// <summary>
        ///   Maximum title's length
        /// </summary>
        private const int MAX_TITLE_LENGTH = 255;

        /// <summary>
        ///   Maximum path's length
        /// </summary>
        private const int MAX_PATH_LENGTH = 255;

        #endregion

        /// <summary>
        ///   Lock object used for concurrency purposes
        /// </summary>
        private static readonly object LockObj = new object();

        /// <summary>
        ///   Incremental Id
        /// </summary>
        private static Int32 _increment;

        #region Private fields
        

        /// <summary>
        ///   Id of the track
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Int32 _id;

        /// <summary>
        ///   Track length
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _path;

        /// <summary>
        ///   Title of the track
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _title;

        /// <summary>
        ///   Track length
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private double _fileLength;

        #endregion

        #region Constructors

        /// <summary>
        ///   Parameter less Constructor
        /// </summary>
        public SoundFile()
        {
            lock (LockObj)
            {
                _id = _increment++;
            }
        }

        /// <summary>
        ///   Track constructor
        /// </summary>
        /// <param name = "title">Title</param>
        /// <param name = "path">Path to file to local system</param>
        /// <param name = "length">Length of the file</param>
        public SoundFile(string title, string path, int length)
            : this()
        {
            Title = title;
            Path = path;
            FileLength = length;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Soundfile's id
        /// </summary>
        public Int32 Id
        {
            get { return _id; }
            private set { _id = value; }
        }

        /// <summary>
        ///   SoundFile's title
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value.Length > MAX_TITLE_LENGTH ? value.Substring(0, MAX_TITLE_LENGTH) : value; }
        }

        /// <summary>
        ///   SoundFile's Length
        /// </summary>
        public double FileLength
        {
            get { return _fileLength; }
            set
            {
                if (value < 0)
                    _fileLength = 0;
                _fileLength = value;
            }
        }

        /// <summary>
        ///   Path to file on local system
        /// </summary>
        public string Path
        {
            get { return _path; }
            set { _path = value.Length > MAX_PATH_LENGTH ? value.Substring(0, MAX_PATH_LENGTH) : value; }
        }

        #endregion

        /// <summary>
        ///   Returns hash code of a track object.
        /// </summary>
        /// <returns>Id is returned as it is unique</returns>
        public override int GetHashCode()
        {
            return _id;
        }
    }
}