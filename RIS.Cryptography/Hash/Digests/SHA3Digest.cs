// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Cryptography.Hash.Digests
{
    public class SHA3Digest
        : KeccakDigest
    {
        public SHA3Digest()
            : this(256)
        {

        }
        public SHA3Digest(
            int size)
            : base(CheckSize(size))
        {

        }



        protected override int DoFinal(
            byte[] data, int offset,
            byte partialByte, int partialBits)
        {
            if (partialBits < 0 || partialBits > 7)
            {
                var exception = new ArgumentException(
                    $"{nameof(partialBits)}[{partialBits}] must be in the range [0,7]",
                    nameof(partialBits));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            var finalPartial =
                (partialByte & ((1 << partialBits) - 1))
                | (0x02 << partialBits);
            var finalPartialBits = partialBits + 2;

            if (finalPartialBits >= 8)
            {
                Absorb(
                    (byte)finalPartial);

                finalPartialBits -= 8;
                finalPartial >>= 8;
            }

            return base.DoFinal(
                data, offset,
                (byte)finalPartial, finalPartialBits);
        }



        public override int DoFinal(
            byte[] data, int offset)
        {
            AbsorbBits(
                0x02, 2);

            return base.DoFinal(
                data, offset);
        }



        private static int CheckSize(
            int size)
        {
            switch (size)
            {
                case 224:
                case 256:
                case 384:
                case 512:
                    return size;
                default:
                    var exception = new ArgumentException(
                        $"{nameof(size)}[{size}] not supported for SHA-3 (must be 224, 256, 384 or 512)",
                        nameof(size));
                    Events.OnError(new RErrorEventArgs(
                        exception, exception.Message));
                    throw exception;
            }
        }
    }
}