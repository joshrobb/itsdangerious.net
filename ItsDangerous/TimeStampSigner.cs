using System;

namespace ItsDangerous
{
    public class TimeStampSigner : Signer
    {

        public TimeStampSigner(string key = null, char separator = ':', string salt = null) : base(key, separator, salt) {
        }

        //our custom epoc for measuring expiry.
        static readonly DateTime EPOCH = new DateTime(2012, 1, 1);

        string TimeStamp() {

            var diff = DateTime.Now - EPOCH;

            var secondssinceepoch= (int)diff.TotalSeconds;

            var time = BitConverter.GetBytes(secondssinceepoch);
            return time.UrlSafeBase64Encode();
        }

        public override string Sign(string value) {
            value = value + Separator + TimeStamp();
            return value + Separator + Signature(value);
        }


        public override string Unsign(string signedvalue) {
            return Unsign(signedvalue);
        }

        public string Unsign(string signedvalue, int maxAge = 0) {

            var ticketvalues = GetTicketValues(signedvalue);

            if (maxAge > 0 && HasExpired(ticketvalues, maxAge))
                throw new SignatureExpiredException(string.Format("Signature age {0} > {1} seconds", GetAge(ticketvalues.Item1), maxAge));

            return ticketvalues.Item2;
        }

        public bool HasExpired(string signedvalue, int maxAge) {
            var ticketvalues = GetTicketValues(signedvalue);

            return HasExpired(ticketvalues, maxAge);
        }

        static bool HasExpired(Tuple<int, string> values, int maxAge) {
            var time = values.Item1;
            var age = GetAge(time);
            if (age.TotalSeconds > maxAge)
                return true;
            return false;
        }

        private static TimeSpan GetAge(int time) {
            var age = DateTime.Now - EPOCH.AddSeconds(time);
            return age;
        }

        Tuple<int, string> GetTicketValues(string signedvalue) {
            var res = base.Unsign(signedvalue);
            var values = res.Split(':');

            var value = values[0];

            var time = BitConverter.ToInt32(values[1].UrlSafeBase64Decode(), 0);

            return Tuple.Create(time, value);
        }
    }

    [Serializable]
    public class SignatureExpiredException : Exception
    {
        public SignatureExpiredException() { }
        public SignatureExpiredException(string message) : base(message) { }
        public SignatureExpiredException(string message, Exception inner) : base(message, inner) { }
        protected SignatureExpiredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
