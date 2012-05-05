using System;
using System.Collections.Generic;
using System.Text;

namespace EyeFiConnector
{
    public static class EyeFiCrypto
    {

        // The TCP checksum requires an even number of bytes. If an even
        // number of bytes is not passed in then null pad the input and then
        // compute the checksum
        public static byte[] CalculateTCPChecksum(byte[] bytes)
        {
            uint checksum = 0;

            // If the number of bytes I was given is not a multiple of 2
            // pad the input
            if (bytes.Length % 2 != 0)
                Array.Resize(ref bytes, bytes.Length + 1);           

            // Loop over all the bytes, two at a time
            // For each pair of bytes, cast them into a 2 byte uint16
            for (int i = 0; i < bytes.Length; i += 2)
                checksum += BitConverter.ToUInt16(bytes, i);

            // The sum at this point is probably an int. Take the left 16 bits
            // and the right 16 bits, interpret both as an integer with max value 2^16 and
            // add them together. If the resulting value is still bigger than 2^16 then do it
            // again until we get a value less than 16 bits.
            while ((checksum >> 16) != 0)
                checksum = ((checksum & 0xFFFF) + (checksum >> 16));

            // Get complement
            checksum = ~checksum;

            // This just gets the last 16 bits
            return BitConverter.GetBytes(checksum & 0xFFFF);
        }

        public static string CalculateIntegrityDigest(byte[] bytes, string uploadKey)
        {
            // If the number of bytes provided is not a multiple of 512
            // pad the input to get the proper alignment
            if (bytes.Length % 512 != 0)
                Array.Resize(ref bytes, 512 - (bytes.Length % 512));

            // Convert the upload key from hex to binary
            byte[] key = Utilities.HexToBytes(uploadKey);

            // Create a byte[] array to store all the checksums and the upload key
            // We'll return 2 bytes for every 512 byte chunk, hence length/256
            // Then we add the length of the key
            byte[] integrityDigest = new byte[(bytes.Length / 256) + key.Length];

            // Loop over all the bytes, using 512 byte blocks
            for (int i = 0; i < bytes.Length; i += 512)
            {
                byte[] checksumInput = new byte[512];
                Buffer.BlockCopy(bytes, i, checksumInput, 0, 512);
                int offset = 2 * (i / 512);
                Buffer.BlockCopy(CalculateTCPChecksum(checksumInput), 0, integrityDigest, offset, 2);
            }
            
            // I don't think this will work
            /*
            StringBuilder builder = new StringBuilder();
            foreach (byte[] b in tcpChecksums)
            {
                string s = Encoding.UTF8.GetString(b, 0, b.Length);
                builder.Append(s);
            }
            string integrityDigest = builder.ToString();
            integrityDigest += Utilities.HexToByteString(uploadKey);
            */
                        
            // Copy the byte arrays into a single array for hashing
            Buffer.BlockCopy(key, 0, integrityDigest, integrityDigest.Length - key.Length, key.Length);

            // Return the MD5 hash string
            return MD5Core.GetHashString(integrityDigest).ToLower();

        }        
    }
}
