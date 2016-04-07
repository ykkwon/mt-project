namespace AcousticFingerprintingLibrary_0._4._5.DistanceClasses
{
    /// <summary>
    ///   Distance class
    /// </summary>
    public class Distance
    {
        /// <summary>
        ///   First distance
        /// </summary>
        public int FirstDistance { get; protected set; }

        /// <summary>
        ///   Distance size
        /// </summary>
        private readonly int _distanceSize;

        /// <summary>
        ///   Constructor of static distance object
        /// </summary>
        /// <param name = "distanceSize">Distance size, used each time GetDistance method is invoked</param>
        public Distance(int distanceSize)
        {
            _distanceSize = distanceSize;
            FirstDistance = 0;
        }

        /// <summary>
        ///   Gets distance size in terms of bit samples, which need to be skipped
        /// </summary>
        /// <returns>Bit samples to skip, between 2 consecutive overlapping fingerprints</returns>
        public int GetDistance()
        {
            return _distanceSize;
        }

        /// <summary>
        ///   Get very first distance
        /// </summary>
        /// <returns></returns>
        public int GetFirstDistance()
        {
            return FirstDistance;
        }
    }
}