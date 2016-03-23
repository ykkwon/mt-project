namespace AcousticFingerprintingLibrary_0._4._5.SoundFingerprint.AudioProxies.Strides
{
    /// <summary>
    ///   StaticStride class
    /// </summary>
    public class StaticStride : IStride
    {
        /// <summary>
        ///   First stride
        /// </summary>
        public int FirstStride { get; protected set; }

        /// <summary>
        ///   Stride size
        /// </summary>
        private readonly int _strideSize;

        /// <summary>
        ///   Constructor of static stride object
        /// </summary>
        /// <param name = "strideSize">Stride size, used each time GetStride method is invoked</param>
        public StaticStride(int strideSize)
        {
            _strideSize = strideSize;
            FirstStride = 0;
        }

        /// <summary>
        ///   Constructor for static stride object
        /// </summary>
        /// <param name = "strideSize">Stride size</param>
        /// <param name = "firstStride">First stride</param>
        public StaticStride(int strideSize, int firstStride) : this(strideSize)
        {
            FirstStride = firstStride;
        }

        #region IStride Members

        /// <summary>
        ///   Get's stride size in terms of bit samples, which need to be skipped
        /// </summary>
        /// <returns>Bit samples to skip, between 2 consecutive overlapping fingerprints</returns>
        public int GetStride()
        {
            return _strideSize;
        }

        /// <summary>
        ///   Get very first stride
        /// </summary>
        /// <returns></returns>
        public int GetFirstStride()
        {
            return FirstStride;
        }

        #endregion
    }
}