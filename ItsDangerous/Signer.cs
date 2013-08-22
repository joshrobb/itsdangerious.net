using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace ItsDangerous
{

    /// <summary>
    /// This is a straight port of the Django Signer module
    /// </summary>
    public class Signer
    {
        private readonly byte[] _key;
        protected char Separator;
        private readonly byte[] _salt;

        public Signer(string key, char separator = ':', string salt = null) {
            _key = Encoding.UTF8.GetBytes(key); 
            Separator = separator;
            var tsalt = salt ?? GetType().Name;
            _salt = Encoding.UTF8.GetBytes(tsalt);
        }

        string Base64HMAC(string value) {
            return SaltedHMAC(value).UrlSafeBase64Encode();
        }

        byte[] SaltedHMAC(string value) {
            var bkey = _key.Concat(_salt).ToArray();

            using (var hmac = new HMACSHA1(bkey))
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
        }

        protected string Signature(string value) {
            return Base64HMAC(value);
        }

        public virtual string Sign(string value) {
            return value + Separator + Signature(value);
        }

        public virtual string Unsign(string signedvalue) {
            if (!signedvalue.Contains(Separator))
                throw new BadSignatureException(string.Format("No {0} found in value", Separator));

            var signature = signedvalue.Substring(signedvalue.LastIndexOf(Separator)+1);
            var value = signedvalue.Substring(0, signedvalue.LastIndexOf(Separator));

            if (!signature.ConstantTimeCompare(Signature(value)))
                throw new BadSignatureException(string.Format("Signature {0} does not match.", signature));
            return value;
        }
    }

    [Serializable]
    public class BadSignatureException : Exception
    {
        public BadSignatureException() { }
        public BadSignatureException(string message) : base(message) { }
        public BadSignatureException(string message, Exception inner) : base(message, inner) { }
        protected BadSignatureException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
