using System.Collections.Generic;
using AcousticFingerprintingLibrary.SoundFingerprint.Model;

namespace AcousticFingerprintingLibrary.SoundFingerprint.DataAccess
{
    /// <summary>
    ///   Storage used for hashes and soundFile
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        ///   Insert SoundFile
        /// </summary>
        /// <param name = "SoundFile">SoundFile to be inserted</param>
        void InsertSoundFile(SoundFile soundfile);

        /// <summary>
        ///   Remove SoundFile
        /// </summary>
        /// <param name = "SoundFile">SoundFile to be removed</param>
        void RemoveSoundFile(SoundFile soundfile);

        /// <summary>
        ///   Insert hash
        /// </summary>
        /// <param name = "hash">Hash to be inserted</param>
        /// <param name = "type">Type of the hash</param>
        void InsertHash(HashSignature hash, HashType type);

        /// <summary>
        ///   Get SoundFile that correspond to a specific signature, with specified threshold
        /// </summary>
        /// <param name = "hashSignature">Hash signature</param>
        /// <param name = "hashTableThreshold">Hash threshold</param>
        /// <returns>SoundFiles that correspond to the hash</returns>
        Dictionary<SoundFile, int> GetSoundFiles(int[] hashSignature, int hashTableThreshold);

        /// <summary>
        ///   Get all hash signatures from a specific soundFile
        /// </summary>
        /// <param name = "SoundFile">Inquired SoundFile</param>
        /// <param name = "type">Hash type</param>
        /// <returns>Hash signatures</returns>
        HashSet<HashSignature> GetHashSignatures(SoundFile soundFile, HashType type);

        /// <summary>
        ///   Get all soundFile from the storage
        /// </summary>
        /// <returns>All soundFile in the storage</returns>
        List<SoundFile> GetAllSoundFile();

        /// <summary>
        ///   Clear all data from the storage
        /// </summary>
        void ClearAll();
    }
}