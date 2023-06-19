using System.Security.Cryptography;
using System.Text;

namespace StudentRatingSystemWebApp
{
    public class HashCalculator
    {
        private SHA256 sha256;

        public HashCalculator()
        {
            sha256 = SHA256.Create();
        }

        public string CalculateHash(string str)
        {
            return ByteArrayToString(sha256.ComputeHash(StringToByteArray(str)));
        }
        private byte[] StringToByteArray(string str)
        {
            byte[] bytes = new byte[str.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)str[i];
            }
            return bytes;
        }
        private string ByteArrayToString(byte[] bytes)
        {
            StringBuilder strb = new StringBuilder();
            foreach(var b in bytes)
            {
                strb.Append((char)b);
            }
            return strb.ToString();
        }

        public string CalculateHash(string str, string salt)
        {
            return CalculateHash(str + salt);
        }

        public string CalculateHash(string str, string salt, string pepper)
        {
            return CalculateHash(CalculateHash(str, salt) + pepper);
        }
    }
}
