using System;
using System.Security.Cryptography;
using RIS.Randomizing;

namespace RIS.Extensions
{
    public static class RandomNumberGeneratorExtensions
    {
        public static Random AsRandom(this RandomNumberGenerator randomNumberGenerator)
        {
            if (randomNumberGenerator == null)
            {
                throw new ArgumentNullException(nameof(randomNumberGenerator));
            }

            return new RNGRandom(randomNumberGenerator);
        }
    }
}
