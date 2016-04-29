using System;
using System.Runtime.InteropServices;

namespace AcousticFingerprintingLibrary_0._4._5.FFT
{
    /// <summary>
    ///   A double-precision complex number representation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Complex : IComparable
    {
        /// <summary>
        ///   The real component of the complex number
        /// </summary>
        public double Re;

        /// <summary>
        ///   The imaginary component of the complex number
        /// </summary>
        public double Im;
        
        /// <summary>
        ///   The modulus (length) of the complex number
        /// </summary>
        /// <returns></returns>
        public double GetModulus()
        {
            double x = Re;
            double y = Im;
            return Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        ///   Get the hash code of the complex number
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (Re.GetHashCode() ^ Im.GetHashCode());
        }

        public static Complex Multiplier(Complex a, double f)
        {
            a.Re = (a.Re * f);
            a.Im = (a.Im * f);
            return a;
        }
        
        /// <summary>
        ///   Get the string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("( {0}, {1}i )", Re, Im);
        }

        public int CompareTo(object obj)
        {
            return 1;
        }
    }
}