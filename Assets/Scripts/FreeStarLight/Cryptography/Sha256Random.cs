using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VirusPet
{
    public class Sha256Random : IDisposable
    {
        private SHA256 Provider;
        private byte[] CurrentHash;

        public Sha256Random(byte[] seed)
        {
            Provider = SHA256.Create();

            CurrentHash = Provider.ComputeHash(seed);
        }
        public Sha256Random(string seed) : this(Encoding.UTF8.GetBytes(seed)) { }

        public long GetNextLong()
        {
            long result = BitConverter.ToInt64(CurrentHash,0);
            nextHash();

            return result;
        }

        public long GetNextLongRange(long min, long max)
        {
            var rand = BitConverter.ToUInt64(CurrentHash, 0);

            ulong dif = (ulong)max - (ulong)min;
            
            ulong rangePoint = rand % dif;

            long result = (long)(min + (long)rangePoint);
            
            nextHash();
            return result;
        }

        public double GetNextDouble()
        {
            double result = BitConverter.ToDouble(CurrentHash, 0);
            nextHash();

            return result;
        }

        public List<long> GetNextDistribution(int divisions, long value, long? minAmount = null, long? maxAmount = null)
        {
            List<long> randList = new List<long>();
            List<long> sectionsValues = new List<long>();
            randList.Add(0L);
            randList.Add(value);
            
            long rand;
            for (int i = 0; i < divisions - 1; i++)
            {
                rand = GetNextLongRange(0L, value - 1L);
                
                randList.Add(rand);
            }
            randList.Sort();

            for (int i = 0; i < divisions; i++)
            {
                sectionsValues.Add(randList[i + 1] - randList[i]);
            }
            
            return sectionsValues;
        }
        
        private void nextHash()
        {
            CurrentHash = Provider.ComputeHash(CurrentHash);
        }
        
        public void Dispose()
        {
            Provider.Dispose();
        }
    }
}
