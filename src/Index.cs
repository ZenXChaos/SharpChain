using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpChainBlockChain
{
    public class SharpChainIndex
    {
        public int Start { get; set; } // Seek Start Pos of File
        public int Length { get; set; } // Length of Block Data
        public string Hash { get; set; } // Block Hash

        public SharpChainIndex(int _Start, int _Length, string _Hash) // Constructor
        {
            this.Start = _Start;
            this.Length = _Length;
            this.Hash = _Hash;
        }
        public static SharpChainIndex UpdateIndex(string filename, SharpChainBlock block, int index, int length) // Update the Index File
        {
            if (index == 0)
            {
                index -= 1; // Start at -1
            }

            FileStream fs = File.Open(filename + SharpChain.DatabaseIndexFormat, FileMode.Append); // Open Index File

            string bindex = Convert.ToString(index + 1);
            string blength = Convert.ToString(length);

            fs.Write(Encoding.ASCII.GetBytes(block.Hash.Value), 0, block.Hash.Length);
            fs.Write(Encoding.ASCII.GetBytes(":"), 0, 1);
            fs.Write(Encoding.ASCII.GetBytes(bindex), 0, bindex.Length);
            fs.Write(Encoding.ASCII.GetBytes(":"), 0, 1);
            fs.Write(Encoding.ASCII.GetBytes(blength), 0, blength.Length);
            fs.Write(Encoding.ASCII.GetBytes("~"), 0, 1);

            fs.Flush();
            fs.Close();

            return new SharpChainIndex(index + 1, length, block.Hash.Value); // Return the Index
        }
        public static Dictionary<string, SharpChainIndex> ParseSharpChainIndex(SharpChain sc, int start = 0, bool CopyEx = false) // Parse the Chain Index File
        {
            string newFile = sc.Database + SharpChain.DatabaseIndexFormat;

            if (CopyEx)
            {
                newFile = sc.Database + SharpChain.DatabaseIndexFormat + "-copy-" + Guid.NewGuid().ToString();
                File.Copy(sc.Database + SharpChain.DatabaseIndexFormat, newFile);
            }

            FileStream fs = File.Open(sc.Database + SharpChain.DatabaseIndexFormat, FileMode.Open);
            string data = "";
            byte[] file_data = null;
            if ((int)fs.Length < 1 || start > (int)fs.Length)
            {
                file_data = new byte[0];
            }
            else
            {
                file_data = new byte[((int)fs.Length) - 1 - start];
                fs.Read(file_data, start, (((int)fs.Length) - 1) - start);
            }



            for (int i = 0; i < file_data.Length; i++)
            {
                data += System.Text.Encoding.UTF8.GetString(new[] { file_data[i] });
            }

            fs.Flush();
            fs.Close();

            string[] _lines = data.Split('~');
            Dictionary<string, SharpChainIndex> lines = new Dictionary<string, SharpChainIndex>();

            foreach (string line in _lines)
            {
                string[] l = line.Split(':');
                try
                {
                    lines.Add(l[0], new SharpChainIndex(Convert.ToInt32(l[1]), Convert.ToInt32(l[2]), l[0]));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            if (CopyEx)
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
            }
            return lines;
        }
        public static List<SharpChainIndex> ParseSharpChainIndexList(SharpChain sc, int start = 0, bool CopyEx = false) // Parse the Chain Index File
        {
            string newFile = sc.Database + SharpChain.DatabaseIndexFormat;

            if(CopyEx)
            {
                newFile = newFile + "-copy-" + Guid.NewGuid().ToString();
                File.Copy(sc.Database + SharpChain.DatabaseIndexFormat, newFile);
            }

            FileStream fs = File.Open(newFile, FileMode.Open);
            string data = "";

            byte[] file_data = null;
            if ((int)fs.Length < 1 || start > (int)fs.Length)
            {
                file_data = new byte[0];
            }
            else
            {
                file_data = new byte[((int)fs.Length) - 1 - start];
                fs.Read(file_data, start, (((int)fs.Length) - 1) - start);
            }

            for (int i = 0; i < file_data.Length; i++)
            {
                data += System.Text.Encoding.UTF8.GetString(new[] { file_data[i] });
            }

            fs.Flush();
            fs.Close();

            string[] _lines = data.Split('~');
            List<SharpChainIndex> lines = new List<SharpChainIndex>();

            foreach (string line in _lines)
            {
                string[] l = line.Split(':');
                try
                {
                    lines.Add(new SharpChainIndex(Convert.ToInt32(l[1]), Convert.ToInt32(l[2]), l[0]));
                }
                catch (Exception e)
                {
                    if (CopyEx)
                    {
                        if (File.Exists(newFile))
                        {
                            File.Delete(newFile);
                        }
                    }
                    Console.WriteLine(e.Message);
                }
            }

            if(CopyEx)
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
            }
            return lines;
        }
        public static SharpChainIndex IndexOf(SharpChain sc, string Hash, bool CopyEx) // Index of Block Hash
        {
            Dictionary<string, SharpChainIndex> parsedIndexes = SharpChainIndex.ParseSharpChainIndex(sc, 0, CopyEx);

            try
            {
                return parsedIndexes.First((p) => p.Key == Hash).Value;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Could not find hash index!");
            }

            return null;
        }
        public static SharpChainBlock IndexOfBlock(SharpChain sc, string Hash, bool CopyEx, List<SharpChainIndex> parsedIndexes = null) // Get Entire Block via Index of Block Hash
        {
            if (parsedIndexes == null)
            {
                parsedIndexes = SharpChainIndex.ParseSharpChainIndexList(sc, 0, true);
            }

            SharpChainIndex index = null;
            try
            {
                index = parsedIndexes.First((p) => p.Hash == Hash);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Could not find hash index!");
            }

            if (index != null)
            {
                List <SharpChainBlock> blocks = sc.ReadBlocks(false, true, index.Start, index.Length);

                return blocks.Count>0 ? blocks[0] : null;
            }

            return null;
        }
    }
}
