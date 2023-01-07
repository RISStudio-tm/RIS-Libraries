// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Security.Cryptography;
using RIS.Cryptography.Hash.Digests;

namespace RIS.Cryptography.Hash.Algorithms
{
    public class SHA3 : HashAlgorithm
    {
        private readonly SHA3Digest _digest;



        public override int HashSize
        {
            get 
            {
                return _digest.Size;
            }
        }



        internal SHA3(
            int size)
        {
            _digest = new SHA3Digest(
                size);
        }



        protected override void HashCore(
            byte[] data, int offset,
            int length)
        {
            if (HashValue == null)
                Initialize();

            _digest.BlockUpdate(
                data, offset,
                length);
        }


        protected override byte[] HashFinal()
        {
            _digest.DoFinal(
                HashValue, 0);

            return HashValue;
        }



        public override void Initialize()
        {
            HashValue = new byte[_digest.SizeBytes];
        }



        public static SHA3 SHA3b224()
        {
            return new SHA3(224);
        }
        public static SHA3 SHA3b256()
        {
            return new SHA3(256);
        }
        public static SHA3 SHA3b384()
        {
            return new SHA3(384);
        }
        public static SHA3 SHA3b512()
        {
            return new SHA3(512);
        }
    }
}