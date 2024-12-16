using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Backend.Data
{
    public class KeyGenerator
    {
        public static string GenerateKey()
        {
            // Generate a 256-bit (32-byte) key
            byte[] key = new byte[32];
            RandomNumberGenerator.Fill(key);

            // Convert the key to a Base64 string to store in configuration
            return Convert.ToBase64String(key);
        }
    }
}
