using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChainBlockChain.Core
{
    // NOT YET IMPLEMENTED
    public class SharpChainUtils
    {
        public static byte[] Unpack(byte[] data, int ofs)
        {
            byte[] value1;
            using (var stream = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(stream))
                {
                    stream.Seek(4, SeekOrigin.Begin);

                    value1 = reader.ReadBytes(ofs);
                }
            }

            //return unpack('V', substr($data,$ofs, 4))[1];
            return value1;
        }

        public static byte[] fread(FileStream fs, int count)
        {
            byte[] b = new byte[1024];
            UTF8Encoding temp = new UTF8Encoding(true);

            string tmp = "";

            while (fs.Read(b, 0, b.Length) > 0)
            {
                tmp += temp.GetString(b);
                count--;

                if (count == 0)
                {
                    break;
                }
            }

            return b;
        }

        public static string hex2bin(string n)
        {
            string result = "";
            string temp = "";

            for (int i = 0; i < n.Length; i++)
            {
                switch (n[i])
                {
                    case '1':
                        temp = "0001";
                        break;
                    case '2':
                        temp = "0010";
                        break;
                    case '3':
                        temp = "0011";
                        break;
                    case '4':
                        temp = "0100";
                        break;
                    case '5':
                        temp = "0101";
                        break;
                    case '6':
                        temp = "0110";
                        break;
                    case '7':
                        temp = "0111";
                        break;
                    case '8':
                        temp = "1000";
                        break;
                    case '9':
                        temp = "1001";
                        break;
                    case 'A':
                        temp = "1010";
                        break;
                    case 'B':
                        temp = "1011";
                        break;
                    case 'C':
                        temp = "1100";
                        break;
                    case 'D':
                        temp = "1101";
                        break;
                    case 'E':
                        temp = "1110";
                        break;
                    case 'F':
                        temp = "1111";
                        break;
                    default:
                        break;
                }
                result += temp;
            }

            return result;
        }

        public static string ToHexString(string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.Unicode.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        public static string FromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.Unicode.GetString(bytes);
        }

        public static string HashByteArray(byte[] array)
        {
            string _hash = "";
            int i;
            for (i = 0; i < array.Length; i++)
            {
                _hash += String.Format("{0:X2}", array[i]);
            }

            return _hash;
        }

    }
}
