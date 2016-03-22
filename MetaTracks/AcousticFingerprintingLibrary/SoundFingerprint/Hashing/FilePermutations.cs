using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcousticFingerprintingLibrary.SoundFingerprint.Hashing
{
    class FilePermutations : IPermutations
    {
        private readonly string pathToPermutations;

        private readonly string separator;

        private int[][] permutations;

        public FilePermutations(string pathToPermutations, string separator)
        {
            this.pathToPermutations = pathToPermutations;
            this.separator = separator;
        }

        public int[][] GetPermutations()
        {
            return null;
        }
        /*
        public int[][] GetPermutations()
        {
            if (permutations != null)
            {
                return permutations;
            }

            
            //Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultFileStreamBufferSize, FileOptions.SequentialScan, Path.GetFileName(path), false, false, checkHost);

            List<int[]> result = new List<int[]>();
            String[] str = pathToPermutations.Split();
            for (int index = 0; index < str.Length-1; index++)
            {
                if (line != null)
                {
                    string[] ints = line.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                    int[] permutation = new int[ints.Length];
                    int i = 0;
                    foreach (string item in ints)
                    {
                        permutation[i++] = Convert.ToInt32(item, CultureInfo.InvariantCulture);
                    }

                    result.Add(permutation);
                }
            }

            permutations = result.ToArray();
            return permutations;
        }*/
    }
}
