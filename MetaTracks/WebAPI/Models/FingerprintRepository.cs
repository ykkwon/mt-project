using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class FingerprintRepository : IFingerprintRepository
    {
        private List<Fingerprint> fingerprints = new List<Fingerprint>();
        private int _nextId = 1;

        public FingerprintRepository()
        {
            {
                Add(new Fingerprint { Id = 1, Hash = "12345678", Title = "For a Few Dollars More", Type = "Movie"});
                Add(new Fingerprint { Id = 2, Hash = "23456789", Title = "Interstellar", Type = "Movie" });
                Add(new Fingerprint { Id = 3, Hash = "34567890", Title = "Top Secret!", Type = "Movie" });
            }
        }

        public IEnumerable<Fingerprint> GetAll()
        {
            return fingerprints;
        }

        public Fingerprint Get(int id)
        {
            return fingerprints.Find(p => p.Id == id);
        }

        public Fingerprint Add(Fingerprint item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            item.Id = _nextId++;
            fingerprints.Add(item);
            return item;
        }

        public bool Update(Fingerprint item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            int index = fingerprints.FindIndex(p => p.Id == item.Id);
            if (index == -1)
            {
                return false;
            }
            fingerprints.RemoveAt(index);
            fingerprints.Add(item);
            return true;
        }
    }
}