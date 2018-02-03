using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SharpChainBlockChain
{
    public class SharpChainBlock
    {
        public SharpChainHash Hash { get; set; } // Block Hash
        public SharpChainIndex Index{ get; set; } // Index Data

        public int Height { get; set; } // Block Height

        public int TimeStamp { get; internal set; } // Time in seconds

        public string Data { get; set; } // Data
        public string Owner { get; set; } // Block Submitted By ID
        
        public Dictionary<string,string> JsonData  // this.Data (JSON->Dictionary)
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(this.Data);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            }
        }

        public bool Inconsistent { get; internal set; } // Is Database Inconsistent
        
        public string GenerateHash(string SHA256_Master) // Generate The Hash
        {
            return SharpChainHash.ComputeHash(this.TimeStamp, this.Hash.PreviousValue, this.Hash.Value, this.Owner, SHA256_Master);
        }

        public string GetProperty(string prop)
        {
            return this.JsonData[prop] != null ? this.JsonData[prop] : String.Empty; // Get Blockchain Property Value
        }

        public SharpChainBlock(int _Time = 0, string _Hash = "", string _PrevHash = "", string _Data = "", string _DataFile = "", string _Owner = null, bool _Inconsistent = false, SharpChainIndex _Index = null) // Constructor
        {
            this.TimeStamp = _Time;
            this.Hash = new SharpChainHash();
            this.Hash.Value = _Hash;
            this.Hash.PreviousValue = _PrevHash;
            this.Data = _Data;
            this.Owner = _Owner;
            this.Inconsistent = _Inconsistent;
            this.Index = _Index;
            this.Height = 0;
        }
        
        /* STATIC */
        public static string HASH_ALGO = "sha256";
        /* STATIC */
    }

}
