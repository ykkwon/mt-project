using System;
using System.Runtime.InteropServices;

namespace AcousticFingerprintingLibrary_0._4._5.FFT
{
    // Comments? Questions? Bugs? Tell Ben Houston at ben@exocortex.org
    // Version: May 4, 2002

    /// <summary>
    ///   <p>A double-precision complex number representation.</p>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Complex : IComparable
    {
        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------

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

        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------

        /// <summary>
        ///   Compare to other complex numbers or real numbers
        /// </summary>
        /// <param name = "o"></param>
        /// <returns></returns>
        public int CompareTo(object o)
        {
            if (o == null)
            {
                return 1; // null sorts before current
            }
            if (o is Complex)
            {
                return GetModulus().CompareTo(((Complex)o).GetModulus());
            }
            if (o is double)
            {
                return GetModulus().CompareTo((double)o);
            }
            if (o is ComplexF)
            {
                return GetModulus().CompareTo(((ComplexF)o).GetModulus());
            }
            if (o is float)
            {
                return GetModulus().CompareTo((float)o);
            }
            throw new ArgumentException();
        }

        
        /// <summary>
        ///   Multiply a complex number by a real
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "f"></param>
        /// <returns></returns>
        public static Complex operator *(Complex a, double f)
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

        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
        
    }
}