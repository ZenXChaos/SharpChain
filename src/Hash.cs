using SharpChainBlockChain.Core;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SharpChainBlockChain
{
    public class SharpChainHash
    {
        public string Value { get; set; } // Hash
        public string PreviousValue { get; set; } // Hash

        public int Length // Hash Length
        {

        get
            {
                return Value.Length;
            }
        }

        public SharpChainHash(string _Value = null, string _PreviousValue = null) // Constructor
        {

            if (_Value != null)
            {
                this.Value = _Value;
            }

            if (_PreviousValue != null)
            {
                this.PreviousValue = _PreviousValue;
            }
        }

        public static string ComputeHash(int _time, string _PrevHash, string _dataS, string _Owner, string SHA256_Master) // Compute Hash for Block Data
        {
            string HashPrep = Convert.ToString(_time) + _PrevHash + Convert.ToString(_dataS.Length) + _dataS + Convert.ToString(_Owner.Length) + _Owner;

            SHA256 sha = SHA256.Create();
            sha.ComputeHash(Encoding.UTF8.GetBytes(HashPrep));

            string newHashVal = SharpChainUtils.HashByteArray(sha.Hash);

            sha = SHA256.Create();
            sha.ComputeHash(Encoding.UTF8.GetBytes(SHA256_Master + newHashVal));

            string generatedHash = SharpChainUtils.HashByteArray(sha.Hash);

            return generatedHash;
        }

    }
}
