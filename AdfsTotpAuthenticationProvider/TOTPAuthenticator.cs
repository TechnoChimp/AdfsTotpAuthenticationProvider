using System;
using System.Linq;
using System.Security.Cryptography;
using AdfsTotpAuthenticationProvider.Interfaces;

namespace AdfsTotpAuthenticationProvider
{
    public class TotpAuthenticator
    {
        private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567"; // Due to Base32 encoding; https://code.google.com/p/vellum/wiki/GoogleAuthenticator
        private const int ValidityPeriodSeconds = 30; // RFC6238 4.1; X represents the time step in seconds (default value X = 30 seconds) and is a system parameter.
        private const int FutureIntervals = 1; // How much time in the future can the client be; in validityPeriodSeconds intervals.
        private const int PastIntervals = 1; // How much time in the past can the client be; in validityPeriodSeconds intervals.
        private const int SecretKeyLength = 16; // Must be a multiple of 8, iPhones accept up to 16 characters (apparently; didn't test it; don't own an iPhone)
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Beginning of time, according to Unix

        public static string GenerateSecretKey()
        {
            return new string((new char[SecretKeyLength])
                .Select(c => c = AllowedCharacters[NextRandomInt(0, AllowedCharacters.Length)]).ToArray());
        }

        private static Int32 NextRandomInt(Int32 minValue, Int32 maxValue)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var uint32Buffer = new byte[sizeof(UInt32)];

                if (minValue > maxValue)
                    throw new ArgumentOutOfRangeException(nameof(minValue));

                if (minValue == maxValue) return minValue;

                Int64 diff = maxValue - minValue;

                while (true)
                {
                    rng.GetBytes(uint32Buffer);
                    UInt32 rand = BitConverter.ToUInt32(uint32Buffer, 0);

                    Int64 max = (1 + (Int64)UInt32.MaxValue);

                    Int64 remainder = max % diff;

                    if (rand < max - remainder)
                    {
                        return (Int32)(minValue + (rand % diff));
                    }
                }
            }

        }

        private static long GetInterval(DateTime dateTime)
        {
            var elapsedTime = dateTime.ToUniversalTime() - UnixEpoch;

            return (long)elapsedTime.TotalSeconds / ValidityPeriodSeconds;
        }

        public static string GetCode(string secretKey)
        {
            return GetCode(secretKey, DateTime.Now);
        }

        public static string GetCode(string secretKey, DateTime when)
        {
            return GetCode(secretKey, GetInterval(when));
        }

        private static string GetCode(string secretKey, long timeIndex)
        {
            var secretKeyBytes = Base32Encode(secretKey);
            //for (int i = secretKey.Length; i < secretKeyBytes.Length; i++) {secretKeyBytes[i] = 0;}
            var hmac = new HMACSHA1(secretKeyBytes);
            var challenge = BitConverter.GetBytes(timeIndex);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(challenge);
            var hash = hmac.ComputeHash(challenge);
            var offset = hash[19] & 0xf;
            var truncatedHash = hash[offset] & 0x7f;
            for (var i = 1; i < 4; i++)
            {
                truncatedHash <<= 8;
                truncatedHash |= hash[offset + i] & 0xff;
            }
            truncatedHash %= 1000000;
            return truncatedHash.ToString("D6");
        }

        public static bool CheckCode(string upn, string secretKey, string code, IUsedCodeProvider usedCodeProvider)
        {
            return CheckCode(upn, secretKey, code, usedCodeProvider, DateTime.Now);
        }

        private static bool CheckCode(string upn, string secretKey, string code, IUsedCodeProvider usedCodeProvider, DateTime when)
        {
            var currentInterval = GetInterval(when);
            var success = false;

            for (var timeIndex = currentInterval - PastIntervals; timeIndex <= currentInterval + FutureIntervals; timeIndex++)
            {
                var intervalCode = GetCode(secretKey, timeIndex);
                var intervalCodeHasBeenUsed = usedCodeProvider.IsCodeUsed(upn, timeIndex);

                if (!intervalCodeHasBeenUsed && ConstantTimeEquals(intervalCode, code))
                {
                    success = true;
                    usedCodeProvider.SetUsedCode(upn, timeIndex);
                    break;
                }
            }

            return success;
        }

        private static byte[] Base32Encode(string source)
        {
            var bits = source.ToUpper().ToCharArray().Select(c => Convert.ToString(AllowedCharacters.IndexOf(c), 2).PadLeft(5, '0')).Aggregate((a, b) => a + b);
            return Enumerable.Range(0, bits.Length / 8).Select(i => Convert.ToByte(bits.Substring(i * 8, 8), 2)).ToArray();
        }

        protected static bool ConstantTimeEquals(string a, string b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;

            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)a[i] ^ (uint)b[i];
            }

            return diff == 0;
        }
    }
}
