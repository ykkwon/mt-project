﻿using System;
using System.Collections.Generic;

namespace AcousticFingerprintingLibrary_0._4._5.Hashing
{
    /// <summary>
    ///  Class for Min Hash algorithm implementation
    /// </summary>
    public class MinHash
    {
        /// <summary>
        ///   Maximum number of hash buckets in the database
        /// </summary>
        private const int HashBucketSize = 100000;
        
        // Highest primenumber
        private const long PrimeP = 2147483659;
        
        
        private readonly int[][] _permutations;

        /// <summary>
        ///   Public constructor 
        ///  perm length = 100, int[255] in each
        /// </summary>
        public MinHash()
        {
            _permutations = DefaultPermutations.GetDefaultPermutations();
        }

        public byte[] ComputeMinHashSignatureByte(bool[] fingerprint)
        {
            bool[] signature = fingerprint;
            int[][] perms = _permutations;
            byte[] minHash = new byte[perms.Length]; /*100*/
            for (int i = 0; i < perms.Length /*100*/; i++)
            {
                minHash[i] = 255;
                for (int j = 0; j < perms[i].Length /*256*/; j++)
                {
                    if (signature[perms[i][j]])
                    {
                        minHash[i] = (byte)j; /*Looking for first occurrence of '1'*/
                        break;
                    }
                }
            }

            return minHash; /*Array of 100 elements with bit turned ON if permutation captured successfully a TRUE bit*/
        }

        public Dictionary<int, long> GroupMinHashToLshBucketsByte(byte[] minHashes, int numberOfHashTables, int numberOfMinHashesPerKey)
        {
            const int A = 1;
            const int B = 0;
            
            Dictionary<int, long> result = new Dictionary<int, long>();

            const int maxNumber = 8; /*Int64 biggest value for MinHash*/
            if (numberOfMinHashesPerKey > maxNumber)
                throw new ArgumentException("numberOfMinHashesPerKey cannot be bigger than 8");
            for (int i = 0; i < numberOfHashTables /*hash functions*/; i++)
            {
                byte[] array = new byte[maxNumber];
                for (int j = 0; j < numberOfMinHashesPerKey /*r min hash signatures*/; j++)
                {
                    array[j] = minHashes[i * numberOfMinHashesPerKey + j];
                }
                long hashbucket = BitConverter.ToInt64(array, 0); //actual value of the signature
                hashbucket = ((A * hashbucket + B) % PrimeP) % HashBucketSize;
                result.Add(i, hashbucket);
            }
            return result;
        }
    }
}