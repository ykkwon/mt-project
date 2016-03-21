using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iOSApplication.SoundFingerprint.AudioProxies.Strides;
using iOSApplication.SoundFingerprint.Hashing;
using iOSApplication.SoundFingerprint.Model;

namespace iOSApplication.SoundFingerprint.DataAccess
{
    /// <summary>
    ///   Singleton class for repository container
    /// </summary>
    public class Repository
    {
        /// <summary>
        ///   Min hasher
        /// </summary>
        private readonly MinHash _hasher;

        /// <summary>
        ///   Creates fingerprints according to the theoretical constructs
        /// </summary>
        private readonly Fingerprinter _manager;

        /// <summary>
        ///   Storage for min-hash permutations
        /// </summary>
        private readonly IPermutations _permutations;

        /// <summary>
        ///   Storage for hash signatures and SoundFile
        /// </summary>
        private readonly IStorage _storage;

        /// <summary>
        ///   Each repository should have storage for permutations and for fingerprints
        /// </summary>
        /// <param name = "storage">SoundFile/Signatures storage</param>
        /// <param name = "permutations">Permutations storage</param>
        public Repository(IStorage storage, IPermutations permutations)
        {
            _permutations = permutations;
            _storage = storage;
            _manager = new Fingerprinter();
            _hasher = new MinHash(_permutations);
        }

        /// <summary>
        ///   Create fingerprints out of down sampled samples
        /// </summary>
        /// <param name = "samples">Down sampled to 5512 samples </param>
        /// <param name = "soundFile">soundFile</param>
        /// <param name = "stride">Stride</param>
        /// <param name = "hashTables">Number of hash tables</param>
        /// <param name = "hashKeys">Number of hash keys</param>
        public void CreateInsertFingerprints(float[] samples,
            SoundFile soundFile,
            IStride stride,
            int hashTables,
            int hashKeys)
        {
            if (soundFile == null) return; /*SoundFile is not eligible*/
            /*Create fingerprints that will be used as initial fingerprints to be queried*/
            //List<bool[]> dbFingers = _manager.CreateFingerprints(samples, stride);
            List<Fingerprint> dbFingers = _manager.CreateFingerprints(samples, stride);
            _storage.InsertSoundFile(soundFile); /*Insert SoundFile into the storage*/
            /*Get fingerprint's hash signature, and associate it to a specific SoundFile*/
            List<HashSignature> creationalsignatures = GetSignatures(dbFingers, soundFile, hashTables, hashKeys);
            foreach (HashSignature hash in creationalsignatures)
            {
                _storage.InsertHash(hash, HashType.Creational);
                /*Set this hashes as also the query hashes*/
                _storage.InsertHash(hash, HashType.Query);
            }
            return;
        }


        // ReSharper disable ReturnTypeCanBeEnumerable.Local
        private List<HashSignature> GetSignatures(IEnumerable<Fingerprint> fingerprints, SoundFile soundFile, int hashTables, int hashKeys)
        //private List<HashSignature> GetSignatures(IEnumerable<bool[]> fingerprints, SoundFile soundFile, int hashTables, int hashKeys)
        {
            List<HashSignature> signatures = new List<HashSignature>();
            foreach (var fingerprint in fingerprints)
            {
                int[] signature = _hasher.ComputeMinHashSignature(fingerprint.Signature);
                /*Compute min-hash signature out of fingerprint*/
                Dictionary<int, long> buckets = _hasher.GroupMinHashToLSHBuckets(signature, hashTables, hashKeys);
                /*Group Min-Hash signature into LSH buckets*/
                int[] hashSignature = new int[buckets.Count];
                foreach (KeyValuePair<int, long> bucket in buckets)
                    hashSignature[bucket.Key] = (int) bucket.Value;
                HashSignature hash = new HashSignature(soundFile, hashSignature); /*associate soundFile to hash-signature*/
                signatures.Add(hash);
            }
            return signatures; /*Return the signatures*/
        }

        /// <summary>
        ///   Find duplicates between existing SoundFiles in the database
        /// </summary>
        /// <param name = "SoundFiles">SoundFiles to be processed (this list should contain only SoundFiles that have been inserted previously)</param>
        /// <param name = "threshold">Number of threshold tables</param>
        /// <param name = "percentageThreshold">Percentage of fingerprints threshold</param>
        /// <param name = "callback">Callback invoked at each processed SoundFiles</param>
        /// <returns>Sets of duplicates</returns>
        public HashSet<SoundFile>[] FindDuplicates(List<SoundFile> soundFiles, int threshold, double percentageThreshold, Action<SoundFile, int, int> callback)
        {
            List<HashSet<SoundFile>> duplicates = new List<HashSet<SoundFile>>();
            int total = soundFiles.Count, current = 0;
            foreach (SoundFile soundFile in soundFiles)
            {
                Dictionary<SoundFile, int> soundFileDuplicates = new Dictionary<SoundFile, int>(); /*this will be a set with duplicates*/
                HashSet<HashSignature> fingerprints = _storage.GetHashSignatures(soundFile, HashType.Query); /*get all existing signatures for a specific soundFile*/
                int fingerthreshold = (int)((float)fingerprints.Count / 100 * percentageThreshold);
                foreach (HashSignature fingerprint in fingerprints)
                {
                    Dictionary<SoundFile, int> results = _storage.GetSoundFiles(fingerprint.Signature, threshold); /*get all duplicate soundFile including the original soundFile*/
                    foreach (KeyValuePair<SoundFile, int> result in results)
                    {
                        if (!soundFileDuplicates.ContainsKey(result.Key))
                            soundFileDuplicates.Add(result.Key, 1);
                        else
                            soundFileDuplicates[result.Key]++;
                    }
                }
                if (soundFileDuplicates.Count > 1)
                {
                    IEnumerable<KeyValuePair<SoundFile, int>> d = soundFileDuplicates.Where((pair) => pair.Value > fingerthreshold);
                    if (d.Count() > 1)
                        duplicates.Add(new HashSet<SoundFile>(d.Select((pair) => pair.Key)));
                }
                if (callback != null)
                    callback.Invoke(soundFile, total, ++current);
            }

            for (int i = 0; i < duplicates.Count - 1; i++)
            {
                HashSet<SoundFile> set = duplicates[i];
                for (int j = i + 1; j < duplicates.Count; j++)
                {
                    IEnumerable<SoundFile> result = set.Intersect(duplicates[j]);
                    if (result.Count() > 0)
                    {
                        duplicates.RemoveAt(j); /*Remove the duplicate set*/
                        i = -1; /*Start iterating from the beginning*/
                        break;
                    }
                }
            }
            return duplicates.ToArray();
        }

        /// <summary>
        ///   Clear current storage
        /// </summary>
        public void ClearStorage()
        {
            _storage.ClearAll();
        }
    }
}