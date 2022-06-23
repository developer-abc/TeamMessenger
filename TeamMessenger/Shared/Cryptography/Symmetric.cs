using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TeamMessenger.Shared.Cryptography
{
    public class Symmetric
    {
        public static byte[] Encrypt(byte[] inputData, byte[] key, byte[] IV)
        {
            RijndaelManaged Rijndael = new RijndaelManaged() { Key = key, IV = IV };

            using MemoryStream input = new MemoryStream(inputData);
            using ICryptoTransform encryptor = Rijndael.CreateEncryptor();
            using CryptoStream cryptoStream = new CryptoStream(input, encryptor, CryptoStreamMode.Read);
            using MemoryStream output = new MemoryStream();
            byte[] bytearrayinput = new byte[1024];
            int length;
            do
            {
                length = cryptoStream.Read(bytearrayinput, 0, 1024);
                output.Write(bytearrayinput, 0, length);
            } while (length != 0);
            // https://docs.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/partial-byte-reads-in-streams
            //} while (length == 1024);
            output.Flush();
            return output.ToArray();
        }
        public static byte[] Decrypt(byte[] inputData, byte[] key, byte[] IV)
        {
            RijndaelManaged Rijndael = new RijndaelManaged() { Key = key, IV = IV };

            using MemoryStream input = new MemoryStream(inputData);
            using ICryptoTransform decryptor = Rijndael.CreateDecryptor();
            using CryptoStream cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            using MemoryStream output = new MemoryStream();
            byte[] bytearrayinput = new byte[1024];
            int length;
            do
            {
                length = cryptoStream.Read(bytearrayinput, 0, 1024);
                output.Write(bytearrayinput, 0, length);
            } while (length != 0);
            // https://docs.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/partial-byte-reads-in-streams
            //} while (length == 1024);
            output.Flush();
            return output.ToArray();
        }
    }
}
