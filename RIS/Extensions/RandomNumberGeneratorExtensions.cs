// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

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
