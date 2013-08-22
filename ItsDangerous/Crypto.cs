using System;
using System.Text;
using System.Security.Cryptography;

namespace ItsDangerous
{
    public static class Crypto
    {
        internal static string UrlSafeBase64Encode(this byte[] value) {
            string svalue = Convert.ToBase64String(value);

            // Apply URL variant
            svalue = svalue.Replace('+', '-').Replace('/', '_');

            svalue = svalue.Trim('=');

            return svalue;
        }

        //string UrlSafeBase64Encode(string value) {
        //    byte[] encodedBytes = Encoding.UTF8.GetBytes(value);
        //}

        internal static byte[] UrlSafeBase64Decode(this string value) {
            value = value.PadRight(value.Length + (4 - value.Length % 4) % 4, '=');
            value = value.Replace('-', '+').Replace('_', '/');

            var bytes = Convert.FromBase64String(value);

            return bytes;
        }

        /// <summary>
        /// Comparason takes the same ammount of time irresepective of differences between strings. 
        /// </summary>
        public static bool ConstantTimeCompare(this string s1, string s2) {
            var b1 = Encoding.UTF8.GetBytes(s1);
            var b2 = Encoding.UTF8.GetBytes(s2);

            var result = 0;

            result |= b1.Length ^ b2.Length;

            //avoid side channel attacks by using a constant time bitwise 
            //operator rather than potentially short circuting boolean logic.
            for (int i = 0; i < b1.Length; i++) {
                result |= b1[i] ^ b2[i];
            }
            return result == 0;
        }


        public static byte[] GetSecureBytes(int length) {
            var bytes = new byte[length];

            using (var rng = new RNGCryptoServiceProvider()) {
                rng.GetBytes(bytes);
            }

            return bytes;
        }
    }
}
