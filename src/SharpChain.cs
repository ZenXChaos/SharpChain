using Newtonsoft.Json;
using SharpChainBlockChain.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SharpChainBlockChain
{
    public class SharpChain
    {
        public UInt32 Magic { get; internal set; } // Blockchain Version

        public string basedir { get; internal set; }

        public string ID { get; internal set; }

        public string db { get; set;}
        
        public string Database
        {
            get
            {
                if (this.db == String.Empty)
                    this.db = "sc";

                return this.basedir + "Blockchain-" + this.db + "-";
            }
        }

        public static string DatabaseIndexFormat // Blockchain Index Extension
        {
            get
            {
                return ".sidx";
            }
        }

        private List<SharpChainBlock> blocks { get; set; }

        public List<SharpChainBlock> Blocks
        {
            get
            {
                if(blocks.Count == 0)
                {
                    blocks = this.ReadBlocks(true, true);
                }
                return blocks;
            }
        }

        public SharpChainBlock LastBlock
        {
            get
            {
                if ( this.blocks.Count > 0 )
                {
                    return this.blocks[this.blocks.Count - 1];
                }else
                {
                    return this.GetLastBlock(this, true);
                }
            }
        }

        public string SHA256_Master = "MASTER-HASH-VALUE"; // Can be anything. This secret is hashed on top of block byte data for extra security.
        public SharpChain(string ExistingGuid = null, string _basedir = "", string _SHA256_Master = "MASTER-HASH-VALUE")
        {
            this.Magic = 0xD5E8A97F;
            this.db = String.Empty;
            this.ID = ExistingGuid != null ? ExistingGuid : Guid.NewGuid().ToString(); // Assign GUID if not exists
            this.db = this.ID; // Assign Database Filename = GUID
            this.basedir = _basedir; // Set Base Directory
            this.blocks = new List<SharpChainBlock>();

            this.SHA256_Master = _SHA256_Master; // Master Key; Hashed into all blocks
        }

        #region SharpChain Utils
        public List<SharpChainBlock> GetBlocks() // Get Blocks, Refresh?
        {
            return this.ReadBlocks(true, true); // Read All Blocks, {Check Consistency, Copy File Before Reading}
        }

        public bool SanityCheck(Dictionary<string, string> forceEQProps = null) // Check For Inconsistencies In Blockchain
        {
            if(!File.Exists(this.Database))
            {
                return false;
            }

            var _Blocks = this.ReadBlocks(true, true, 0, 0, true);

            try
            {
                var item = _Blocks.First((p) => p.Inconsistent == true); // Block Is Inconsistent?
                return false;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message); // Psuedo Error, Actually successful !

                if (forceEQProps == null)
                    return true;

                foreach(KeyValuePair<string, string> kvp in forceEQProps) // Each Block Must Have Property Equal To kvp
                {
                    try
                    {
                        Dictionary<string, string> data = null;

                        
                        for (int i = 0; i < _Blocks.Count; i++)
                        {
                            data = JsonConvert.DeserializeObject<Dictionary<string, string>>(_Blocks[i].Data); // Get Block Properties

                            if (data[kvp.Key] != kvp.Value)
                                return false; // Failed, Inconsistent With Forced Property
                        }

                        return true;
                    }
                    catch (Exception eK)
                    {
                        Console.WriteLine(eK.Message); // Error
                        return false;
                    }
                }

                return _Blocks != null; // If Blocks Not Null
            }
            
        }
        #endregion

        #region SharpChain Reading
        public List<SharpChainBlock> ReadBlocks(bool checkConsistency = true, bool CopyEx = false, int start = 0, int length = 0, bool SanityCheck = false) // Read Blocks in the Blockchain
        {

            string filename = this.Database;
            List<SharpChainBlock> blocks = new List<SharpChainBlock>();

            if(!File.Exists(this.Database))
            {
                return blocks;
            }

            string consist_prev_hash = "0000000000000000000000000000000000000000000000000000000000000000"; // Genesis hash, 64 0's
            var newFile = filename;
            if(CopyEx)
            {
                newFile = filename + "-copy-" + Guid.NewGuid().ToString();
                File.Copy(filename, newFile);
            }

            using (FileStream fs = File.Open(newFile, FileMode.Open))
            {
                if (fs != null)
                {
                    byte[] testarr = new byte[(int)fs.Length];

                    int posOffset = 0;

                    UInt32 _magic;
                    int count = 0;
                    int _dataLen = 0;
                    int _time = 0;
                    int _olength = 0;
                    string _dataS = "";
                    string _owner = "";
                    string _PrevHash = "";
                    string _BlockHash = "";

                    byte[] hash_gen = new byte[99999];

                    fs.Read(testarr, 0, (int)fs.Length);

                    var len = testarr.Length;
                    var max = start > 0 ? length + start : len;

                    for (int i = 0; i < testarr.Length; i++)
                    {
                        if(i>=start && i <= max)
                        {
                            hash_gen[i] = testarr[i];
                            int testtt = (i + _dataLen) - posOffset;
                            if (i - posOffset == 0) // MAGIC
                            {
                                _magic = BitConverter.ToUInt32(testarr, i);
                            }
                            else if (i - posOffset == 1 && i != 1)
                            {
                                _magic = BitConverter.ToUInt32(testarr, i);
                            }
                            else if (i - posOffset == 5)
                            {
                                _time = BitConverter.ToInt32(testarr, i);
                            }
                            else if (i - posOffset == 9) // Data Length
                            {
                                _dataLen = BitConverter.ToInt32(testarr, i);
                            }
                            else if (i - posOffset >= 13 && i - posOffset < (13 + _dataLen))
                            {
                                _dataS += System.Text.Encoding.UTF8.GetString(new[] { testarr[i] });
                            }
                            else if (i - posOffset > (13 + _dataLen) - 1 && i - posOffset < ((13 + _dataLen) + 64)) // Previous Hash
                            {
                                _PrevHash += System.Text.Encoding.UTF8.GetString(new[] { testarr[i] });
                            }
                            else if (i - posOffset > ((13 + _dataLen) + 64) - 1 && i - posOffset < (((13 + _dataLen) + 64) + 64)) //Block Hash
                            {
                                _BlockHash += System.Text.Encoding.UTF8.GetString(new[] { testarr[i] });

                            }
                            else if (i - posOffset > (((13 + _dataLen) + 64) + 64) - 1 && i - posOffset < (((13 + _dataLen) + 64) + 64) + 1 ) //Block Hash
                            {
                                _olength = testarr[i];
                            }else if (i - posOffset > (((13 + _dataLen) + 64) + 64) + 3 && i - posOffset < (((13 + _dataLen) + 64) + 64) + 3 + _olength+1)
                            {
                                _owner += System.Text.Encoding.UTF8.GetString(new[] { testarr[i] });
                            }
                            else if (i-posOffset > (((13 + _dataLen) + 64) + 64) + 3 + _olength+1 || i >= fs.Length - 1)
                            {
                                count++;
                                posOffset = i;

                                // Check validation
                                // Compute SharpChain Hash
                                string currentHash = SharpChainHash.ComputeHash(_time, _PrevHash, _dataS, _owner, SHA256_Master);

                                bool Inconsistent = false;
                                if (currentHash != _BlockHash && i != 0 && checkConsistency)
                                {
                                    Console.WriteLine("Found inconsistency with data!...");
                                    Console.WriteLine("Found " + currentHash + " but expected " + _BlockHash);
                                    Console.WriteLine("*********************");
                                    Console.WriteLine("INCONSISTENT @ BLOCK " + Convert.ToString(count - 1));
                                    Console.WriteLine("DATA '" + _dataS + "' is unreliable!");
                                    Console.WriteLine("*********************");
                                    Console.WriteLine("Chain has been broken! ");
                                    Console.WriteLine("");
                                    Console.WriteLine("");
                                    Console.WriteLine("");
                                    Inconsistent = true;

                                    if(SanityCheck)
                                    {
                                        return null;
                                    }
                                }

                                if (consist_prev_hash != _PrevHash && i != 0 && checkConsistency)
                                {

                                    Console.WriteLine("Found inconsistency with data!...");
                                    Console.WriteLine("Found " + _PrevHash + " but expected " + consist_prev_hash);
                                    Console.WriteLine("*********************");
                                    Console.WriteLine("INCONSISTENT @ BLOCK " + Convert.ToString(count - 1));
                                    Console.WriteLine("DATA '" + _dataS + "' is unreliable!");
                                    Console.WriteLine("*********************");
                                    Console.WriteLine("Chain has been broken! ");
                                    Console.WriteLine("");
                                    Console.WriteLine("");
                                    Console.WriteLine("");
                                    Inconsistent = true;
                                    consist_prev_hash = _PrevHash;
                                    if (SanityCheck)
                                    {
                                        return null;
                                    }
                                }
                                else
                                {
                                    consist_prev_hash = currentHash;
                                }

                                if (Inconsistent == false && checkConsistency)
                                {
                                    Console.WriteLine("Found expected Hash " + consist_prev_hash);
                                    Console.WriteLine("*********************");
                                    Console.WriteLine("CONSISTENT @ BLOCK " + Convert.ToString(count - 1));
                                    Console.WriteLine("DATA '" + _dataS + "' is reliable!");
                                    Console.WriteLine("*********************");
                                    Console.WriteLine("Chain has not yet been broken! ");
                                    Console.WriteLine("");
                                    Console.WriteLine("");
                                    Console.WriteLine("");
                                }

                                SharpChainBlock block = new SharpChainBlock(_time, currentHash, _PrevHash, _dataS, filename, _owner, Inconsistent, SharpChainIndex.IndexOf(this, currentHash, true));
                                block.Height = count-1;
                                blocks.Add(block);


                                _dataLen = 0;
                                _dataS = null;
                                _PrevHash = null;
                                _BlockHash = null;
                                _owner = null;
                                _olength = 0;

                                if (i >= fs.Length - 1)
                                {
                                    break;
                                }
                            }
                        }else
                        {
                            posOffset = i;
                        }

                    }

                    fs.Flush();
                    fs.Close();
                }
                else
                {

                    Console.WriteLine("Could not open SharpChain file!");
                }
            }

            if (checkConsistency)
            {
                if (blocks.Count > 0)
                {
                    if (blocks[blocks.Count - 1].Inconsistent == false)
                    {
                        Console.WriteLine("Found no inconsistencies!");
                    }
                }
            }

            if(CopyEx)
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
            }
            return blocks;
        }
        public SharpChainBlock GetLastBlock(SharpChain sc, bool CopyEx) // Get the Last Block
        {
            List<SharpChainIndex> parsedIndexes = SharpChainIndex.ParseSharpChainIndexList(sc,0, CopyEx);

            if (parsedIndexes.Count > 0)
            {
                var cnt = parsedIndexes.Count - 1;
                var hash = parsedIndexes[cnt].Hash;
                return SharpChainIndex.IndexOfBlock(sc, hash, CopyEx, parsedIndexes);
            }

            return null;
        }
        #endregion

        #region SharpChain Writing
        private void WriteBlock(string filename, string data, string owner, string prevHash, int BlockIndex) // (Physical) Write Block to Blockchain
        {
            if (prevHash == null)
            {
                prevHash = "0000000000000000000000000000000000000000000000000000000000000000"; // Genesis Hash
            }

            FileStream fs = File.Open(filename, FileMode.Append);

            int time = (int)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
            string HashPrep = Convert.ToString(time) + prevHash + Convert.ToString(data.Length) + data + Convert.ToString(owner.Length) + owner;
            SHA256 sha = SHA256.Create();
            sha.ComputeHash(Encoding.UTF8.GetBytes(HashPrep));

            string HashVal = SharpChainUtils.HashByteArray(sha.Hash);

            sha = SHA256.Create();
            sha.ComputeHash(Encoding.UTF8.GetBytes(SHA256_Master + HashVal));

            string MasterHashVal = SharpChainUtils.HashByteArray(sha.Hash);

            int bytesDataLength = BitConverter.GetBytes(data.Length).Length;
            int bytesOwnerDataLength = BitConverter.GetBytes(owner.Length).Length;

            SharpChainBlock block = new SharpChainBlock(time, MasterHashVal, prevHash, data, fs.Name);

            SharpChainIndex.UpdateIndex(fs.Name, block, (int)fs.Length, bytesDataLength + data.Length + 139 + bytesOwnerDataLength + owner.Length);

            // Write to SharpChain
            fs.Write(BitConverter.GetBytes(this.Magic), 0, 3); // Byte: 0
            fs.Write(BitConverter.GetBytes(BlockIndex), 0, 1); // Byte: 0
            fs.Write(BitConverter.GetBytes('\x01'), 0, 1); // Byte: 4
            fs.Write(BitConverter.GetBytes(time), 0, 4); //Byte: 5
            fs.Write(BitConverter.GetBytes(data.Length), 0, bytesDataLength); // Byte: 10
            fs.Write(Encoding.ASCII.GetBytes(data), 0, data.Length); // Byte: 13 + data.Length
            fs.Write(Encoding.ASCII.GetBytes(prevHash), 0, 64); // Byte: 13 + data.Length + 64
            fs.Write(Encoding.ASCII.GetBytes(MasterHashVal), 0, 64); // Byte: 13 + data.Length + 128
            fs.Write(BitConverter.GetBytes(owner.Length), 0, bytesOwnerDataLength); // Byte: 10
            fs.Write(Encoding.ASCII.GetBytes(owner), 0, owner.Length); // Byte: 13 + data.Length
            fs.Write(BitConverter.GetBytes('\0'), 0, 1); // Byte: 13 + data.Length + 128 + 1 (END OF BLOCK!)

            fs.Flush();
            fs.Close();
        }

        public SharpChainBlock AddBlock(string data, string Owner = null, Dictionary<string,string> dict = null) // Add Block to Blockchain
        {
            if (dict == null)
            {
                dict = new Dictionary<string, string>();
            }

            bool exists = File.Exists(this.Database);
            SharpChainBlock lastBlock = null;
            
            dict.Add("Data", data);
            dict.Add("GUID", Owner != null ? Owner : this.ID);
            string jsonData = JsonConvert.SerializeObject(dict); // Convert data to JSON
            
            try
            {
                if (!exists)
                {
                    // Write Genesis Block
                    WriteBlock(this.Database, jsonData, Owner, "0000000000000000000000000000000000000000000000000000000000000000", 0);
                }
                else
                {
                    // Read Last Block
                    lastBlock = GetLastBlock(this, true);
                    
                    // Write A Block
                    string h = lastBlock != null ? lastBlock.Hash.Value : null;
                    WriteBlock(this.Database, jsonData, Owner, h, 0);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            
            return lastBlock; // Return the last block
        }
        #endregion
    }
}
