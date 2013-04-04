using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace V8Reader.Comparison
{
    class StreamComparator : IComparator
    {

        public bool CompareStreams(Stream Compared, Stream Comparand)
        {
            byte[] hash1;
            byte[] hash2;

            using (var md5impl = MD5.Create())
            {
                hash1 = md5impl.ComputeHash(Compared);
                hash2 = md5impl.ComputeHash(Comparand);
            }
            
            if (hash1.Length != hash2.Length)
                return false;

            bool match = true;

            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                {
                    match = false;
                    break;
                }
            }

            return match;

        }

        #region IComparator Members

        public bool CompareObjects(object Compared, object Comparand)
        {
            return CompareStreams((Stream)Compared, (Stream)Comparand);
        }

        #endregion
    }

}
