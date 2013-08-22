using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Shouldly;
using Xunit;

namespace ItsDangerous.Tests
{
    public class SigningTests
    {
        [Fact]
        public void can_sign_and_unsign_value() {
            var signer = new Signer("testing");

            var signed = signer.Sign("hello world");

            var unsigned = signer.Unsign(signed);

            unsigned.ShouldBe("hello world");
        }

        [Fact]
        public void tampered_value_throws() {
            var signer = new Signer("testing");

            var signed = signer.Sign("hello world");

            signed = "!" + signed;

            Assert.Throws<BadSignatureException>(() => {
                var unsigned = signer.Unsign(signed);
                Assert.False(true);
            });
        }

        [Fact]
        public void default_key_works() {
            var signer = new Signer("some key");
            var signed = signer.Sign("test");
            signer.Unsign(signed).ShouldBe("test");
        }

        [Fact]
        public void can_sign_with_timestamp() {
            var timedsigner = new TimeStampSigner("testing");
            var signed = timedsigner.Sign("hello world");

            var unsigned = timedsigner.Unsign(signed);

            unsigned.ShouldBe("hello world");
        }

        [Fact]
        public void timestamp_age_check_works() {
            var timedsigner = new TimeStampSigner("testing");
            var signed = timedsigner.Sign("hello world");

            Trace.WriteLine(signed);

            System.Threading.Thread.Sleep(1100);

            Assert.Throws<SignatureExpiredException>(() => {
                var unsigned = timedsigner.Unsign(signed, 1);
            });
        }

        [Fact]
        public void tampering_with_timestamp_throws() {
            var timedsigner = new TimeStampSigner("testing");
            var signed = timedsigner.Sign("hello world");

            signed = signed.Substring(0, 15) + 3 + signed.Substring(15);

            Assert.Throws<BadSignatureException>(() => {
                var unsigned = timedsigner.Unsign(signed, 1);
            });
        }

        [Fact]
        public void checking_expiry_with_valid_ticket_works() {
            var signer = new TimeStampSigner("testing");
            var signed = signer.Sign("hello world");

            signer.HasExpired(signed, 2).ShouldBe(false);
        }

        [Fact]
        public void checking_expiry_with_expired_ticket_works() {
            var signer = new TimeStampSigner("testing");
            var signed = signer.Sign("hello world");

            System.Threading.Thread.Sleep(1100);

            signer.HasExpired(signed, 1).ShouldBe(true);
        }

        [Fact(Skip = "Measuring this is fucking hard")]
        public void constant_time_comparason_is_constant() {

            var string1 = "".PadRight(10000, '*');
            var string2 = "".PadRight(10000, '*');
            var string3 = "".PadRight(9999, '*') + '1';

            //warm up caches

            for (int i = 0; i < 10; i++) {
                string1.ConstantTimeCompare(string2).ShouldBe(true);
                string1.ConstantTimeCompare(string3).ShouldBe(false);
                string1.ConstantTimeCompare(string1).ShouldBe(true);
            }

            Stopwatch timer = Stopwatch.StartNew();

            var sametimes = new List<long>();
            var difftimes = new List<long>();

            for (int i = 0; i < 50000; i++) {
                sametimes.Add(TimeCompare(string1, string2, timer));
            }

            for (int i = 0; i < 50000; i++) {
                difftimes.Add(TimeCompare(string1, string3, timer, match: false));
            }

            var samevariance = CalculateVariance(sametimes.Select(t => (decimal)t).ToArray());
            var diffvariance = CalculateVariance(difftimes.Select(t => (decimal)t).ToArray());

            samevariance.ShouldBe(diffvariance);

            difftimes.Sum().ShouldBe(sametimes.Sum());

        }

        private double CalculateVariance(decimal[] input) {
            double sumOfSquares = 0.0;
            double total = 0.0;
            foreach (var d in input) {
                total += (double) d;
                sumOfSquares += Math.Pow((double) d, 2);
            }
            int n = input.Length;
            return ((n * sumOfSquares) - Math.Pow(total, 2)) / (n * (n - 1));
        }

        private long TimeCompare(string string1, string string2, Stopwatch timer, bool match = true) {
            timer.Reset();
            timer.Start();
            string1.ConstantTimeCompare(string2).ShouldBe(match);
            timer.Stop();
            var time = timer.ElapsedTicks;
            return time;
        }
    }
}
