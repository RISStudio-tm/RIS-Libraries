// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Security.Cryptography;

namespace RIS.Cryptography.Hash.Algorithms
{
    public sealed class RIPEMD160 : HashAlgorithm
    {
        private byte[] _buffer;
        private long _count;
        private uint[] _stateMD160;
        private uint[] _blockDWords;

        public new static RIPEMD160 Create()
        {
            return new RIPEMD160();
        }
        public new static RIPEMD160 Create(string hashName)
        {
            return new RIPEMD160();
        }

        public RIPEMD160()
        {
            HashSizeValue = 160;

            _stateMD160 = new uint[5];
            _blockDWords = new uint[16];
            _buffer = new byte[64];

            InitializeState();
        }

        public override void Initialize()
        {
            InitializeState();

            Array.Clear(_blockDWords, 0, _blockDWords.Length);
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        protected override void HashCore(byte[] rgb, int ibStart, int cbSize)
        {
            HashData(rgb, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            return EndHash();
        }

        private void InitializeState()
        {
            _count = 0L;
            _stateMD160[0] = 1732584193U;
            _stateMD160[1] = 4023233417U;
            _stateMD160[2] = 2562383102U;
            _stateMD160[3] = 271733878U;
            _stateMD160[4] = 3285377520U;
        }

        private unsafe void HashData(byte[] partIn, int ibStart, int cbSize)
        {
            int byteCount = cbSize;
            int srcOffsetBytes = ibStart;
            int dstOffsetBytes = (int)(_count & 63L);

            _count += (long)byteCount;

            fixed (uint* state = _stateMD160)
            fixed (byte* block = _buffer)
            fixed (uint* blockDWords = _blockDWords)
            {
                if (dstOffsetBytes > 0 && dstOffsetBytes + byteCount >= 64)
                {
                    Buffer.BlockCopy(partIn, srcOffsetBytes, _buffer, dstOffsetBytes, 64 - dstOffsetBytes);

                    srcOffsetBytes += 64 - dstOffsetBytes;
                    byteCount -= 64 - dstOffsetBytes;

                    MDTransform(blockDWords, state, block);

                    dstOffsetBytes = 0;
                }

                while (byteCount >= 64)
                {
                    Buffer.BlockCopy(partIn, srcOffsetBytes, _buffer, 0, 64);

                    srcOffsetBytes += 64;
                    byteCount -= 64;

                    MDTransform(blockDWords, state, block);
                }

                if (byteCount > 0)
                {
                    Buffer.BlockCopy(partIn, srcOffsetBytes, _buffer, dstOffsetBytes, byteCount);
                }
            }
        }

        private byte[] EndHash()
        {
            int length = 64 - (int)(_count & 63L);

            if (length <= 8)
                length += 64;

            byte[] block = new byte[20];
            byte[] partIn = new byte[length];
            long num = _count * 8L;

            partIn[0] = (byte)128;
            partIn[length - 1] = (byte)((ulong)(num >> 56) & (ulong)byte.MaxValue);
            partIn[length - 2] = (byte)((ulong)(num >> 48) & (ulong)byte.MaxValue);
            partIn[length - 3] = (byte)((ulong)(num >> 40) & (ulong)byte.MaxValue);
            partIn[length - 4] = (byte)((ulong)(num >> 32) & (ulong)byte.MaxValue);
            partIn[length - 5] = (byte)((ulong)(num >> 24) & (ulong)byte.MaxValue);
            partIn[length - 6] = (byte)((ulong)(num >> 16) & (ulong)byte.MaxValue);
            partIn[length - 7] = (byte)((ulong)(num >> 8) & (ulong)byte.MaxValue);
            partIn[length - 8] = (byte)((ulong)num & (ulong)byte.MaxValue);

            HashData(partIn, 0, partIn.Length);
            DWORDToLittleEndian(block, _stateMD160, 5);

            HashValue = block;

            return block;
        }

        private static unsafe void MDTransform(uint* blockDWords, uint* state, byte* block)
        {
            DWORDFromLittleEndian(blockDWords, 16, block);

            uint num1 = *state;
            uint num2 = state[1];
            uint y1 = state[2];
            uint z1 = state[3];
            uint num3 = state[4];
            uint num4 = num1;
            uint num5 = num2;
            uint y2 = y1;
            uint z2 = z1;
            uint num6 = num3;
            uint num7 = num1 + (*blockDWords + F(num2, y1, z1));
            uint num8 = (num7 << 11 | num7 >> 21) + num3;
            uint z3 = y1 << 10 | y1 >> 22;
            uint num9 = num3 + (blockDWords[1] + F(num8, num2, z3));
            uint num10 = (num9 << 14 | num9 >> 18) + z1;
            uint z4 = num2 << 10 | num2 >> 22;
            uint num11 = z1 + (blockDWords[2] + F(num10, num8, z4));
            uint num12 = (num11 << 15 | num11 >> 17) + z3;
            uint z5 = num8 << 10 | num8 >> 22;
            uint num13 = z3 + (blockDWords[3] + F(num12, num10, z5));
            uint num14 = (num13 << 12 | num13 >> 20) + z4;
            uint z6 = num10 << 10 | num10 >> 22;
            uint num15 = z4 + (blockDWords[4] + F(num14, num12, z6));
            uint num16 = (num15 << 5 | num15 >> 27) + z5;
            uint z7 = num12 << 10 | num12 >> 22;
            uint num17 = z5 + (blockDWords[5] + F(num16, num14, z7));
            uint num18 = (num17 << 8 | num17 >> 24) + z6;
            uint z8 = num14 << 10 | num14 >> 22;
            uint num19 = z6 + (blockDWords[6] + F(num18, num16, z8));
            uint num20 = (num19 << 7 | num19 >> 25) + z7;
            uint z9 = num16 << 10 | num16 >> 22;
            uint num21 = z7 + (blockDWords[7] + F(num20, num18, z9));
            uint num22 = (num21 << 9 | num21 >> 23) + z8;
            uint z10 = num18 << 10 | num18 >> 22;
            uint num23 = z8 + (blockDWords[8] + F(num22, num20, z10));
            uint num24 = (num23 << 11 | num23 >> 21) + z9;
            uint z11 = num20 << 10 | num20 >> 22;
            uint num25 = z9 + (blockDWords[9] + F(num24, num22, z11));
            uint num26 = (num25 << 13 | num25 >> 19) + z10;
            uint z12 = num22 << 10 | num22 >> 22;
            uint num27 = z10 + (blockDWords[10] + F(num26, num24, z12));
            uint num28 = (num27 << 14 | num27 >> 18) + z11;
            uint z13 = num24 << 10 | num24 >> 22;
            uint num29 = z11 + (blockDWords[11] + F(num28, num26, z13));
            uint num30 = (num29 << 15 | num29 >> 17) + z12;
            uint z14 = num26 << 10 | num26 >> 22;
            uint num31 = z12 + (blockDWords[12] + F(num30, num28, z14));
            uint num32 = (num31 << 6 | num31 >> 26) + z13;
            uint z15 = num28 << 10 | num28 >> 22;
            uint num33 = z13 + (blockDWords[13] + F(num32, num30, z15));
            uint num34 = (num33 << 7 | num33 >> 25) + z14;
            uint z16 = num30 << 10 | num30 >> 22;
            uint num35 = z14 + (blockDWords[14] + F(num34, num32, z16));
            uint num36 = (num35 << 9 | num35 >> 23) + z15;
            uint z17 = num32 << 10 | num32 >> 22;
            uint num37 = z15 + (blockDWords[15] + F(num36, num34, z17));
            uint num38 = (num37 << 8 | num37 >> 24) + z16;
            uint z18 = num34 << 10 | num34 >> 22;
            uint num39 = z16 + (uint)((int)G(num38, num36, z18) + (int)blockDWords[7] + 1518500249);
            uint num40 = (num39 << 7 | num39 >> 25) + z17;
            uint z19 = num36 << 10 | num36 >> 22;
            uint num41 = z17 + (uint)((int)G(num40, num38, z19) + (int)blockDWords[4] + 1518500249);
            uint num42 = (num41 << 6 | num41 >> 26) + z18;
            uint z20 = num38 << 10 | num38 >> 22;
            uint num43 = z18 + (uint)((int)G(num42, num40, z20) + (int)blockDWords[13] + 1518500249);
            uint num44 = (num43 << 8 | num43 >> 24) + z19;
            uint z21 = num40 << 10 | num40 >> 22;
            uint num45 = z19 + (uint)((int)G(num44, num42, z21) + (int)blockDWords[1] + 1518500249);
            uint num46 = (num45 << 13 | num45 >> 19) + z20;
            uint z22 = num42 << 10 | num42 >> 22;
            uint num47 = z20 + (uint)((int)G(num46, num44, z22) + (int)blockDWords[10] + 1518500249);
            uint num48 = (num47 << 11 | num47 >> 21) + z21;
            uint z23 = num44 << 10 | num44 >> 22;
            uint num49 = z21 + (uint)((int)G(num48, num46, z23) + (int)blockDWords[6] + 1518500249);
            uint num50 = (num49 << 9 | num49 >> 23) + z22;
            uint z24 = num46 << 10 | num46 >> 22;
            uint num51 = z22 + (uint)((int)G(num50, num48, z24) + (int)blockDWords[15] + 1518500249);
            uint num52 = (num51 << 7 | num51 >> 25) + z23;
            uint z25 = num48 << 10 | num48 >> 22;
            uint num53 = z23 + (uint)((int)G(num52, num50, z25) + (int)blockDWords[3] + 1518500249);
            uint num54 = (num53 << 15 | num53 >> 17) + z24;
            uint z26 = num50 << 10 | num50 >> 22;
            uint num55 = z24 + (uint)((int)G(num54, num52, z26) + (int)blockDWords[12] + 1518500249);
            uint num56 = (num55 << 7 | num55 >> 25) + z25;
            uint z27 = num52 << 10 | num52 >> 22;
            uint num57 = z25 + (uint)((int)G(num56, num54, z27) + (int)*blockDWords + 1518500249);
            uint num58 = (num57 << 12 | num57 >> 20) + z26;
            uint z28 = num54 << 10 | num54 >> 22;
            uint num59 = z26 + (uint)((int)G(num58, num56, z28) + (int)blockDWords[9] + 1518500249);
            uint num60 = (num59 << 15 | num59 >> 17) + z27;
            uint z29 = num56 << 10 | num56 >> 22;
            uint num61 = z27 + (uint)((int)G(num60, num58, z29) + (int)blockDWords[5] + 1518500249);
            uint num62 = (num61 << 9 | num61 >> 23) + z28;
            uint z30 = num58 << 10 | num58 >> 22;
            uint num63 = z28 + (uint)((int)G(num62, num60, z30) + (int)blockDWords[2] + 1518500249);
            uint num64 = (num63 << 11 | num63 >> 21) + z29;
            uint z31 = num60 << 10 | num60 >> 22;
            uint num65 = z29 + (uint)((int)G(num64, num62, z31) + (int)blockDWords[14] + 1518500249);
            uint num66 = (num65 << 7 | num65 >> 25) + z30;
            uint z32 = num62 << 10 | num62 >> 22;
            uint num67 = z30 + (uint)((int)G(num66, num64, z32) + (int)blockDWords[11] + 1518500249);
            uint num68 = (num67 << 13 | num67 >> 19) + z31;
            uint z33 = num64 << 10 | num64 >> 22;
            uint num69 = z31 + (uint)((int)G(num68, num66, z33) + (int)blockDWords[8] + 1518500249);
            uint num70 = (num69 << 12 | num69 >> 20) + z32;
            uint z34 = num66 << 10 | num66 >> 22;
            uint num71 = z32 + (uint)((int)H(num70, num68, z34) + (int)blockDWords[3] + 1859775393);
            uint num72 = (num71 << 11 | num71 >> 21) + z33;
            uint z35 = num68 << 10 | num68 >> 22;
            uint num73 = z33 + (uint)((int)H(num72, num70, z35) + (int)blockDWords[10] + 1859775393);
            uint num74 = (num73 << 13 | num73 >> 19) + z34;
            uint z36 = num70 << 10 | num70 >> 22;
            uint num75 = z34 + (uint)((int)H(num74, num72, z36) + (int)blockDWords[14] + 1859775393);
            uint num76 = (num75 << 6 | num75 >> 26) + z35;
            uint z37 = num72 << 10 | num72 >> 22;
            uint num77 = z35 + (uint)((int)H(num76, num74, z37) + (int)blockDWords[4] + 1859775393);
            uint num78 = (num77 << 7 | num77 >> 25) + z36;
            uint z38 = num74 << 10 | num74 >> 22;
            uint num79 = z36 + (uint)((int)H(num78, num76, z38) + (int)blockDWords[9] + 1859775393);
            uint num80 = (num79 << 14 | num79 >> 18) + z37;
            uint z39 = num76 << 10 | num76 >> 22;
            uint num81 = z37 + (uint)((int)H(num80, num78, z39) + (int)blockDWords[15] + 1859775393);
            uint num82 = (num81 << 9 | num81 >> 23) + z38;
            uint z40 = num78 << 10 | num78 >> 22;
            uint num83 = z38 + (uint)((int)H(num82, num80, z40) + (int)blockDWords[8] + 1859775393);
            uint num84 = (num83 << 13 | num83 >> 19) + z39;
            uint z41 = num80 << 10 | num80 >> 22;
            uint num85 = z39 + (uint)((int)H(num84, num82, z41) + (int)blockDWords[1] + 1859775393);
            uint num86 = (num85 << 15 | num85 >> 17) + z40;
            uint z42 = num82 << 10 | num82 >> 22;
            uint num87 = z40 + (uint)((int)H(num86, num84, z42) + (int)blockDWords[2] + 1859775393);
            uint num88 = (num87 << 14 | num87 >> 18) + z41;
            uint z43 = num84 << 10 | num84 >> 22;
            uint num89 = z41 + (uint)((int)H(num88, num86, z43) + (int)blockDWords[7] + 1859775393);
            uint num90 = (num89 << 8 | num89 >> 24) + z42;
            uint z44 = num86 << 10 | num86 >> 22;
            uint num91 = z42 + (uint)((int)H(num90, num88, z44) + (int)*blockDWords + 1859775393);
            uint num92 = (num91 << 13 | num91 >> 19) + z43;
            uint z45 = num88 << 10 | num88 >> 22;
            uint num93 = z43 + (uint)((int)H(num92, num90, z45) + (int)blockDWords[6] + 1859775393);
            uint num94 = (num93 << 6 | num93 >> 26) + z44;
            uint z46 = num90 << 10 | num90 >> 22;
            uint num95 = z44 + (uint)((int)H(num94, num92, z46) + (int)blockDWords[13] + 1859775393);
            uint num96 = (num95 << 5 | num95 >> 27) + z45;
            uint z47 = num92 << 10 | num92 >> 22;
            uint num97 = z45 + (uint)((int)H(num96, num94, z47) + (int)blockDWords[11] + 1859775393);
            uint num98 = (num97 << 12 | num97 >> 20) + z46;
            uint z48 = num94 << 10 | num94 >> 22;
            uint num99 = z46 + (uint)((int)H(num98, num96, z48) + (int)blockDWords[5] + 1859775393);
            uint num100 = (num99 << 7 | num99 >> 25) + z47;
            uint z49 = num96 << 10 | num96 >> 22;
            uint num101 = z47 + (uint)((int)H(num100, num98, z49) + (int)blockDWords[12] + 1859775393);
            uint num102 = (num101 << 5 | num101 >> 27) + z48;
            uint z50 = num98 << 10 | num98 >> 22;
            uint num103 = z48 + (uint)((int)I(num102, num100, z50) + (int)blockDWords[1] - 1894007588);
            uint num104 = (num103 << 11 | num103 >> 21) + z49;
            uint z51 = num100 << 10 | num100 >> 22;
            uint num105 = z49 + (uint)((int)I(num104, num102, z51) + (int)blockDWords[9] - 1894007588);
            uint num106 = (num105 << 12 | num105 >> 20) + z50;
            uint z52 = num102 << 10 | num102 >> 22;
            uint num107 = z50 + (uint)((int)I(num106, num104, z52) + (int)blockDWords[11] - 1894007588);
            uint num108 = (num107 << 14 | num107 >> 18) + z51;
            uint z53 = num104 << 10 | num104 >> 22;
            uint num109 = z51 + (uint)((int)I(num108, num106, z53) + (int)blockDWords[10] - 1894007588);
            uint num110 = (num109 << 15 | num109 >> 17) + z52;
            uint z54 = num106 << 10 | num106 >> 22;
            uint num111 = z52 + (uint)((int)I(num110, num108, z54) + (int)*blockDWords - 1894007588);
            uint num112 = (num111 << 14 | num111 >> 18) + z53;
            uint z55 = num108 << 10 | num108 >> 22;
            uint num113 = z53 + (uint)((int)I(num112, num110, z55) + (int)blockDWords[8] - 1894007588);
            uint num114 = (num113 << 15 | num113 >> 17) + z54;
            uint z56 = num110 << 10 | num110 >> 22;
            uint num115 = z54 + (uint)((int)I(num114, num112, z56) + (int)blockDWords[12] - 1894007588);
            uint num116 = (num115 << 9 | num115 >> 23) + z55;
            uint z57 = num112 << 10 | num112 >> 22;
            uint num117 = z55 + (uint)((int)I(num116, num114, z57) + (int)blockDWords[4] - 1894007588);
            uint num118 = (num117 << 8 | num117 >> 24) + z56;
            uint z58 = num114 << 10 | num114 >> 22;
            uint num119 = z56 + (uint)((int)I(num118, num116, z58) + (int)blockDWords[13] - 1894007588);
            uint num120 = (num119 << 9 | num119 >> 23) + z57;
            uint z59 = num116 << 10 | num116 >> 22;
            uint num121 = z57 + (uint)((int)I(num120, num118, z59) + (int)blockDWords[3] - 1894007588);
            uint num122 = (num121 << 14 | num121 >> 18) + z58;
            uint z60 = num118 << 10 | num118 >> 22;
            uint num123 = z58 + (uint)((int)I(num122, num120, z60) + (int)blockDWords[7] - 1894007588);
            uint num124 = (num123 << 5 | num123 >> 27) + z59;
            uint z61 = num120 << 10 | num120 >> 22;
            uint num125 = z59 + (uint)((int)I(num124, num122, z61) + (int)blockDWords[15] - 1894007588);
            uint num126 = (num125 << 6 | num125 >> 26) + z60;
            uint z62 = num122 << 10 | num122 >> 22;
            uint num127 = z60 + (uint)((int)I(num126, num124, z62) + (int)blockDWords[14] - 1894007588);
            uint num128 = (num127 << 8 | num127 >> 24) + z61;
            uint z63 = num124 << 10 | num124 >> 22;
            uint num129 = z61 + (uint)((int)I(num128, num126, z63) + (int)blockDWords[5] - 1894007588);
            uint num130 = (num129 << 6 | num129 >> 26) + z62;
            uint z64 = num126 << 10 | num126 >> 22;
            uint num131 = z62 + (uint)((int)I(num130, num128, z64) + (int)blockDWords[6] - 1894007588);
            uint num132 = (num131 << 5 | num131 >> 27) + z63;
            uint z65 = num128 << 10 | num128 >> 22;
            uint num133 = z63 + (uint)((int)I(num132, num130, z65) + (int)blockDWords[2] - 1894007588);
            uint num134 = (num133 << 12 | num133 >> 20) + z64;
            uint z66 = num130 << 10 | num130 >> 22;
            uint num135 = z64 + (uint)((int)J(num134, num132, z66) + (int)blockDWords[4] - 1454113458);
            uint num136 = (num135 << 9 | num135 >> 23) + z65;
            uint z67 = num132 << 10 | num132 >> 22;
            uint num137 = z65 + (uint)((int)J(num136, num134, z67) + (int)*blockDWords - 1454113458);
            uint num138 = (num137 << 15 | num137 >> 17) + z66;
            uint z68 = num134 << 10 | num134 >> 22;
            uint num139 = z66 + (uint)((int)J(num138, num136, z68) + (int)blockDWords[5] - 1454113458);
            uint num140 = (num139 << 5 | num139 >> 27) + z67;
            uint z69 = num136 << 10 | num136 >> 22;
            uint num141 = z67 + (uint)((int)J(num140, num138, z69) + (int)blockDWords[9] - 1454113458);
            uint num142 = (num141 << 11 | num141 >> 21) + z68;
            uint z70 = num138 << 10 | num138 >> 22;
            uint num143 = z68 + (uint)((int)J(num142, num140, z70) + (int)blockDWords[7] - 1454113458);
            uint num144 = (num143 << 6 | num143 >> 26) + z69;
            uint z71 = num140 << 10 | num140 >> 22;
            uint num145 = z69 + (uint)((int)J(num144, num142, z71) + (int)blockDWords[12] - 1454113458);
            uint num146 = (num145 << 8 | num145 >> 24) + z70;
            uint z72 = num142 << 10 | num142 >> 22;
            uint num147 = z70 + (uint)((int)J(num146, num144, z72) + (int)blockDWords[2] - 1454113458);
            uint num148 = (num147 << 13 | num147 >> 19) + z71;
            uint z73 = num144 << 10 | num144 >> 22;
            uint num149 = z71 + (uint)((int)J(num148, num146, z73) + (int)blockDWords[10] - 1454113458);
            uint num150 = (num149 << 12 | num149 >> 20) + z72;
            uint z74 = num146 << 10 | num146 >> 22;
            uint num151 = z72 + (uint)((int)J(num150, num148, z74) + (int)blockDWords[14] - 1454113458);
            uint num152 = (num151 << 5 | num151 >> 27) + z73;
            uint z75 = num148 << 10 | num148 >> 22;
            uint num153 = z73 + (uint)((int)J(num152, num150, z75) + (int)blockDWords[1] - 1454113458);
            uint num154 = (num153 << 12 | num153 >> 20) + z74;
            uint z76 = num150 << 10 | num150 >> 22;
            uint num155 = z74 + (uint)((int)J(num154, num152, z76) + (int)blockDWords[3] - 1454113458);
            uint num156 = (num155 << 13 | num155 >> 19) + z75;
            uint z77 = num152 << 10 | num152 >> 22;
            uint num157 = z75 + (uint)((int)J(num156, num154, z77) + (int)blockDWords[8] - 1454113458);
            uint num158 = (num157 << 14 | num157 >> 18) + z76;
            uint z78 = num154 << 10 | num154 >> 22;
            uint num159 = z76 + (uint)((int)J(num158, num156, z78) + (int)blockDWords[11] - 1454113458);
            uint num160 = (num159 << 11 | num159 >> 21) + z77;
            uint z79 = num156 << 10 | num156 >> 22;
            uint num161 = z77 + (uint)((int)J(num160, num158, z79) + (int)blockDWords[6] - 1454113458);
            uint num162 = (num161 << 8 | num161 >> 24) + z78;
            uint z80 = num158 << 10 | num158 >> 22;
            uint num163 = z78 + (uint)((int)J(num162, num160, z80) + (int)blockDWords[15] - 1454113458);
            uint x1 = (num163 << 5 | num163 >> 27) + z79;
            uint z81 = num160 << 10 | num160 >> 22;
            uint num164 = z79 + (uint)((int)J(x1, num162, z81) + (int)blockDWords[13] - 1454113458);
            uint num165 = (num164 << 6 | num164 >> 26) + z80;
            uint num166 = num162 << 10 | num162 >> 22;
            uint num167 = num4 + (uint)((int)J(num5, y2, z2) + (int)blockDWords[5] + 1352829926);
            uint num168 = (num167 << 8 | num167 >> 24) + num6;
            uint z82 = y2 << 10 | y2 >> 22;
            uint num169 = num6 + (uint)((int)J(num168, num5, z82) + (int)blockDWords[14] + 1352829926);
            uint num170 = (num169 << 9 | num169 >> 23) + z2;
            uint z83 = num5 << 10 | num5 >> 22;
            uint num171 = z2 + (uint)((int)J(num170, num168, z83) + (int)blockDWords[7] + 1352829926);
            uint num172 = (num171 << 9 | num171 >> 23) + z82;
            uint z84 = num168 << 10 | num168 >> 22;
            uint num173 = z82 + (uint)((int)J(num172, num170, z84) + (int)*blockDWords + 1352829926);
            uint num174 = (num173 << 11 | num173 >> 21) + z83;
            uint z85 = num170 << 10 | num170 >> 22;
            uint num175 = z83 + (uint)((int)J(num174, num172, z85) + (int)blockDWords[9] + 1352829926);
            uint num176 = (num175 << 13 | num175 >> 19) + z84;
            uint z86 = num172 << 10 | num172 >> 22;
            uint num177 = z84 + (uint)((int)J(num176, num174, z86) + (int)blockDWords[2] + 1352829926);
            uint num178 = (num177 << 15 | num177 >> 17) + z85;
            uint z87 = num174 << 10 | num174 >> 22;
            uint num179 = z85 + (uint)((int)J(num178, num176, z87) + (int)blockDWords[11] + 1352829926);
            uint num180 = (num179 << 15 | num179 >> 17) + z86;
            uint z88 = num176 << 10 | num176 >> 22;
            uint num181 = z86 + (uint)((int)J(num180, num178, z88) + (int)blockDWords[4] + 1352829926);
            uint num182 = (num181 << 5 | num181 >> 27) + z87;
            uint z89 = num178 << 10 | num178 >> 22;
            uint num183 = z87 + (uint)((int)J(num182, num180, z89) + (int)blockDWords[13] + 1352829926);
            uint num184 = (num183 << 7 | num183 >> 25) + z88;
            uint z90 = num180 << 10 | num180 >> 22;
            uint num185 = z88 + (uint)((int)J(num184, num182, z90) + (int)blockDWords[6] + 1352829926);
            uint num186 = (num185 << 7 | num185 >> 25) + z89;
            uint z91 = num182 << 10 | num182 >> 22;
            uint num187 = z89 + (uint)((int)J(num186, num184, z91) + (int)blockDWords[15] + 1352829926);
            uint num188 = (num187 << 8 | num187 >> 24) + z90;
            uint z92 = num184 << 10 | num184 >> 22;
            uint num189 = z90 + (uint)((int)J(num188, num186, z92) + (int)blockDWords[8] + 1352829926);
            uint num190 = (num189 << 11 | num189 >> 21) + z91;
            uint z93 = num186 << 10 | num186 >> 22;
            uint num191 = z91 + (uint)((int)J(num190, num188, z93) + (int)blockDWords[1] + 1352829926);
            uint num192 = (num191 << 14 | num191 >> 18) + z92;
            uint z94 = num188 << 10 | num188 >> 22;
            uint num193 = z92 + (uint)((int)J(num192, num190, z94) + (int)blockDWords[10] + 1352829926);
            uint num194 = (num193 << 14 | num193 >> 18) + z93;
            uint z95 = num190 << 10 | num190 >> 22;
            uint num195 = z93 + (uint)((int)J(num194, num192, z95) + (int)blockDWords[3] + 1352829926);
            uint num196 = (num195 << 12 | num195 >> 20) + z94;
            uint z96 = num192 << 10 | num192 >> 22;
            uint num197 = z94 + (uint)((int)J(num196, num194, z96) + (int)blockDWords[12] + 1352829926);
            uint num198 = (num197 << 6 | num197 >> 26) + z95;
            uint z97 = num194 << 10 | num194 >> 22;
            uint num199 = z95 + (uint)((int)I(num198, num196, z97) + (int)blockDWords[6] + 1548603684);
            uint num200 = (num199 << 9 | num199 >> 23) + z96;
            uint z98 = num196 << 10 | num196 >> 22;
            uint num201 = z96 + (uint)((int)I(num200, num198, z98) + (int)blockDWords[11] + 1548603684);
            uint num202 = (num201 << 13 | num201 >> 19) + z97;
            uint z99 = num198 << 10 | num198 >> 22;
            uint num203 = z97 + (uint)((int)I(num202, num200, z99) + (int)blockDWords[3] + 1548603684);
            uint num204 = (num203 << 15 | num203 >> 17) + z98;
            uint z100 = num200 << 10 | num200 >> 22;
            uint num205 = z98 + (uint)((int)I(num204, num202, z100) + (int)blockDWords[7] + 1548603684);
            uint num206 = (num205 << 7 | num205 >> 25) + z99;
            uint z101 = num202 << 10 | num202 >> 22;
            uint num207 = z99 + (uint)((int)I(num206, num204, z101) + (int)*blockDWords + 1548603684);
            uint num208 = (num207 << 12 | num207 >> 20) + z100;
            uint z102 = num204 << 10 | num204 >> 22;
            uint num209 = z100 + (uint)((int)I(num208, num206, z102) + (int)blockDWords[13] + 1548603684);
            uint num210 = (num209 << 8 | num209 >> 24) + z101;
            uint z103 = num206 << 10 | num206 >> 22;
            uint num211 = z101 + (uint)((int)I(num210, num208, z103) + (int)blockDWords[5] + 1548603684);
            uint num212 = (num211 << 9 | num211 >> 23) + z102;
            uint z104 = num208 << 10 | num208 >> 22;
            uint num213 = z102 + (uint)((int)I(num212, num210, z104) + (int)blockDWords[10] + 1548603684);
            uint num214 = (num213 << 11 | num213 >> 21) + z103;
            uint z105 = num210 << 10 | num210 >> 22;
            uint num215 = z103 + (uint)((int)I(num214, num212, z105) + (int)blockDWords[14] + 1548603684);
            uint num216 = (num215 << 7 | num215 >> 25) + z104;
            uint z106 = num212 << 10 | num212 >> 22;
            uint num217 = z104 + (uint)((int)I(num216, num214, z106) + (int)blockDWords[15] + 1548603684);
            uint num218 = (num217 << 7 | num217 >> 25) + z105;
            uint z107 = num214 << 10 | num214 >> 22;
            uint num219 = z105 + (uint)((int)I(num218, num216, z107) + (int)blockDWords[8] + 1548603684);
            uint num220 = (num219 << 12 | num219 >> 20) + z106;
            uint z108 = num216 << 10 | num216 >> 22;
            uint num221 = z106 + (uint)((int)I(num220, num218, z108) + (int)blockDWords[12] + 1548603684);
            uint num222 = (num221 << 7 | num221 >> 25) + z107;
            uint z109 = num218 << 10 | num218 >> 22;
            uint num223 = z107 + (uint)((int)I(num222, num220, z109) + (int)blockDWords[4] + 1548603684);
            uint num224 = (num223 << 6 | num223 >> 26) + z108;
            uint z110 = num220 << 10 | num220 >> 22;
            uint num225 = z108 + (uint)((int)I(num224, num222, z110) + (int)blockDWords[9] + 1548603684);
            uint num226 = (num225 << 15 | num225 >> 17) + z109;
            uint z111 = num222 << 10 | num222 >> 22;
            uint num227 = z109 + (uint)((int)I(num226, num224, z111) + (int)blockDWords[1] + 1548603684);
            uint num228 = (num227 << 13 | num227 >> 19) + z110;
            uint z112 = num224 << 10 | num224 >> 22;
            uint num229 = z110 + (uint)((int)I(num228, num226, z112) + (int)blockDWords[2] + 1548603684);
            uint num230 = (num229 << 11 | num229 >> 21) + z111;
            uint z113 = num226 << 10 | num226 >> 22;
            uint num231 = z111 + (uint)((int)H(num230, num228, z113) + (int)blockDWords[15] + 1836072691);
            uint num232 = (num231 << 9 | num231 >> 23) + z112;
            uint z114 = num228 << 10 | num228 >> 22;
            uint num233 = z112 + (uint)((int)H(num232, num230, z114) + (int)blockDWords[5] + 1836072691);
            uint num234 = (num233 << 7 | num233 >> 25) + z113;
            uint z115 = num230 << 10 | num230 >> 22;
            uint num235 = z113 + (uint)((int)H(num234, num232, z115) + (int)blockDWords[1] + 1836072691);
            uint num236 = (num235 << 15 | num235 >> 17) + z114;
            uint z116 = num232 << 10 | num232 >> 22;
            uint num237 = z114 + (uint)((int)H(num236, num234, z116) + (int)blockDWords[3] + 1836072691);
            uint num238 = (num237 << 11 | num237 >> 21) + z115;
            uint z117 = num234 << 10 | num234 >> 22;
            uint num239 = z115 + (uint)((int)H(num238, num236, z117) + (int)blockDWords[7] + 1836072691);
            uint num240 = (num239 << 8 | num239 >> 24) + z116;
            uint z118 = num236 << 10 | num236 >> 22;
            uint num241 = z116 + (uint)((int)H(num240, num238, z118) + (int)blockDWords[14] + 1836072691);
            uint num242 = (num241 << 6 | num241 >> 26) + z117;
            uint z119 = num238 << 10 | num238 >> 22;
            uint num243 = z117 + (uint)((int)H(num242, num240, z119) + (int)blockDWords[6] + 1836072691);
            uint num244 = (num243 << 6 | num243 >> 26) + z118;
            uint z120 = num240 << 10 | num240 >> 22;
            uint num245 = z118 + (uint)((int)H(num244, num242, z120) + (int)blockDWords[9] + 1836072691);
            uint num246 = (num245 << 14 | num245 >> 18) + z119;
            uint z121 = num242 << 10 | num242 >> 22;
            uint num247 = z119 + (uint)((int)H(num246, num244, z121) + (int)blockDWords[11] + 1836072691);
            uint num248 = (num247 << 12 | num247 >> 20) + z120;
            uint z122 = num244 << 10 | num244 >> 22;
            uint num249 = z120 + (uint)((int)H(num248, num246, z122) + (int)blockDWords[8] + 1836072691);
            uint num250 = (num249 << 13 | num249 >> 19) + z121;
            uint z123 = num246 << 10 | num246 >> 22;
            uint num251 = z121 + (uint)((int)H(num250, num248, z123) + (int)blockDWords[12] + 1836072691);
            uint num252 = (num251 << 5 | num251 >> 27) + z122;
            uint z124 = num248 << 10 | num248 >> 22;
            uint num253 = z122 + (uint)((int)H(num252, num250, z124) + (int)blockDWords[2] + 1836072691);
            uint num254 = (num253 << 14 | num253 >> 18) + z123;
            uint z125 = num250 << 10 | num250 >> 22;
            uint num255 = z123 + (uint)((int)H(num254, num252, z125) + (int)blockDWords[10] + 1836072691);
            uint num256 = (num255 << 13 | num255 >> 19) + z124;
            uint z126 = num252 << 10 | num252 >> 22;
            uint num257 = z124 + (uint)((int)H(num256, num254, z126) + (int)*blockDWords + 1836072691);
            uint num258 = (num257 << 13 | num257 >> 19) + z125;
            uint z127 = num254 << 10 | num254 >> 22;
            uint num259 = z125 + (uint)((int)H(num258, num256, z127) + (int)blockDWords[4] + 1836072691);
            uint num260 = (num259 << 7 | num259 >> 25) + z126;
            uint z128 = num256 << 10 | num256 >> 22;
            uint num261 = z126 + (uint)((int)H(num260, num258, z128) + (int)blockDWords[13] + 1836072691);
            uint num262 = (num261 << 5 | num261 >> 27) + z127;
            uint z129 = num258 << 10 | num258 >> 22;
            uint num263 = z127 + (uint)((int)G(num262, num260, z129) + (int)blockDWords[8] + 2053994217);
            uint num264 = (num263 << 15 | num263 >> 17) + z128;
            uint z130 = num260 << 10 | num260 >> 22;
            uint num265 = z128 + (uint)((int)G(num264, num262, z130) + (int)blockDWords[6] + 2053994217);
            uint num266 = (num265 << 5 | num265 >> 27) + z129;
            uint z131 = num262 << 10 | num262 >> 22;
            uint num267 = z129 + (uint)((int)G(num266, num264, z131) + (int)blockDWords[4] + 2053994217);
            uint num268 = (num267 << 8 | num267 >> 24) + z130;
            uint z132 = num264 << 10 | num264 >> 22;
            uint num269 = z130 + (uint)((int)G(num268, num266, z132) + (int)blockDWords[1] + 2053994217);
            uint num270 = (num269 << 11 | num269 >> 21) + z131;
            uint z133 = num266 << 10 | num266 >> 22;
            uint num271 = z131 + (uint)((int)G(num270, num268, z133) + (int)blockDWords[3] + 2053994217);
            uint num272 = (num271 << 14 | num271 >> 18) + z132;
            uint z134 = num268 << 10 | num268 >> 22;
            uint num273 = z132 + (uint)((int)G(num272, num270, z134) + (int)blockDWords[11] + 2053994217);
            uint num274 = (num273 << 14 | num273 >> 18) + z133;
            uint z135 = num270 << 10 | num270 >> 22;
            uint num275 = z133 + (uint)((int)G(num274, num272, z135) + (int)blockDWords[15] + 2053994217);
            uint num276 = (num275 << 6 | num275 >> 26) + z134;
            uint z136 = num272 << 10 | num272 >> 22;
            uint num277 = z134 + (uint)((int)G(num276, num274, z136) + (int)*blockDWords + 2053994217);
            uint num278 = (num277 << 14 | num277 >> 18) + z135;
            uint z137 = num274 << 10 | num274 >> 22;
            uint num279 = z135 + (uint)((int)G(num278, num276, z137) + (int)blockDWords[5] + 2053994217);
            uint num280 = (num279 << 6 | num279 >> 26) + z136;
            uint z138 = num276 << 10 | num276 >> 22;
            uint num281 = z136 + (uint)((int)G(num280, num278, z138) + (int)blockDWords[12] + 2053994217);
            uint num282 = (num281 << 9 | num281 >> 23) + z137;
            uint z139 = num278 << 10 | num278 >> 22;
            uint num283 = z137 + (uint)((int)G(num282, num280, z139) + (int)blockDWords[2] + 2053994217);
            uint num284 = (num283 << 12 | num283 >> 20) + z138;
            uint z140 = num280 << 10 | num280 >> 22;
            uint num285 = z138 + (uint)((int)G(num284, num282, z140) + (int)blockDWords[13] + 2053994217);
            uint num286 = (num285 << 9 | num285 >> 23) + z139;
            uint z141 = num282 << 10 | num282 >> 22;
            uint num287 = z139 + (uint)((int)G(num286, num284, z141) + (int)blockDWords[9] + 2053994217);
            uint num288 = (num287 << 12 | num287 >> 20) + z140;
            uint z142 = num284 << 10 | num284 >> 22;
            uint num289 = z140 + (uint)((int)G(num288, num286, z142) + (int)blockDWords[7] + 2053994217);
            uint num290 = (num289 << 5 | num289 >> 27) + z141;
            uint z143 = num286 << 10 | num286 >> 22;
            uint num291 = z141 + (uint)((int)G(num290, num288, z143) + (int)blockDWords[10] + 2053994217);
            uint num292 = (num291 << 15 | num291 >> 17) + z142;
            uint z144 = num288 << 10 | num288 >> 22;
            uint num293 = z142 + (uint)((int)G(num292, num290, z144) + (int)blockDWords[14] + 2053994217);
            uint num294 = (num293 << 8 | num293 >> 24) + z143;
            uint z145 = num290 << 10 | num290 >> 22;
            uint num295 = z143 + (F(num294, num292, z145) + blockDWords[12]);
            uint num296 = (num295 << 8 | num295 >> 24) + z144;
            uint z146 = num292 << 10 | num292 >> 22;
            uint num297 = z144 + (F(num296, num294, z146) + blockDWords[15]);
            uint num298 = (num297 << 5 | num297 >> 27) + z145;
            uint z147 = num294 << 10 | num294 >> 22;
            uint num299 = z145 + (F(num298, num296, z147) + blockDWords[10]);
            uint num300 = (num299 << 12 | num299 >> 20) + z146;
            uint z148 = num296 << 10 | num296 >> 22;
            uint num301 = z146 + (F(num300, num298, z148) + blockDWords[4]);
            uint num302 = (num301 << 9 | num301 >> 23) + z147;
            uint z149 = num298 << 10 | num298 >> 22;
            uint num303 = z147 + (F(num302, num300, z149) + blockDWords[1]);
            uint num304 = (num303 << 12 | num303 >> 20) + z148;
            uint z150 = num300 << 10 | num300 >> 22;
            uint num305 = z148 + (F(num304, num302, z150) + blockDWords[5]);
            uint num306 = (num305 << 5 | num305 >> 27) + z149;
            uint z151 = num302 << 10 | num302 >> 22;
            uint num307 = z149 + (F(num306, num304, z151) + blockDWords[8]);
            uint num308 = (num307 << 14 | num307 >> 18) + z150;
            uint z152 = num304 << 10 | num304 >> 22;
            uint num309 = z150 + (F(num308, num306, z152) + blockDWords[7]);
            uint num310 = (num309 << 6 | num309 >> 26) + z151;
            uint z153 = num306 << 10 | num306 >> 22;
            uint num311 = z151 + (F(num310, num308, z153) + blockDWords[6]);
            uint num312 = (num311 << 8 | num311 >> 24) + z152;
            uint z154 = num308 << 10 | num308 >> 22;
            uint num313 = z152 + (F(num312, num310, z154) + blockDWords[2]);
            uint num314 = (num313 << 13 | num313 >> 19) + z153;
            uint z155 = num310 << 10 | num310 >> 22;
            uint num315 = z153 + (F(num314, num312, z155) + blockDWords[13]);
            uint num316 = (num315 << 6 | num315 >> 26) + z154;
            uint z156 = num312 << 10 | num312 >> 22;
            uint num317 = z154 + (F(num316, num314, z156) + blockDWords[14]);
            uint num318 = (num317 << 5 | num317 >> 27) + z155;
            uint z157 = num314 << 10 | num314 >> 22;
            uint num319 = z155 + (F(num318, num316, z157) + *blockDWords);
            uint num320 = (num319 << 15 | num319 >> 17) + z156;
            uint z158 = num316 << 10 | num316 >> 22;
            uint num321 = z156 + (F(num320, num318, z158) + blockDWords[3]);
            uint num322 = (num321 << 13 | num321 >> 19) + z157;
            uint z159 = num318 << 10 | num318 >> 22;
            uint num323 = z157 + (F(num322, num320, z159) + blockDWords[9]);
            uint x2 = (num323 << 11 | num323 >> 21) + z158;
            uint z160 = num320 << 10 | num320 >> 22;
            uint num324 = z158 + (F(x2, num322, z160) + blockDWords[11]);
            uint num325 = (num324 << 11 | num324 >> 21) + z159;
            uint num326 = (num322 << 10 | num322 >> 22) + (x1 + state[1]);
            state[1] = state[2] + num166 + z160;
            state[2] = state[3] + z81 + z159;
            state[3] = state[4] + z80 + num325;
            state[4] = *state + num165 + x2;
            *state = num326;
        }

        private static uint F(uint x, uint y, uint z)
        {
            return x ^ y ^ z;
        }

        private static uint G(uint x, uint y, uint z)
        {
            return (uint)((int)x & (int)y | ~(int)x & (int)z);
        }

        private static uint H(uint x, uint y, uint z)
        {
            return (x | ~y) ^ z;
        }

        private static uint I(uint x, uint y, uint z)
        {
            return (uint)((int)x & (int)z | (int)y & ~(int)z);
        }

        private static uint J(uint x, uint y, uint z)
        {
            return x ^ (y | ~z);
        }

        private static unsafe void DWORDFromLittleEndian(uint* x, int digits, byte* block)
        {
            int i;
            int j;

            for (i = 0, j = 0; i < digits; ++i, j += 4)
            {
                x[i] = (uint)(block[j] | (block[j + 1] << 8) | (block[j + 2] << 16) | (block[j + 3] << 24));
            }
        }

        private static void DWORDToLittleEndian(byte[] block, uint[] x, int digits)
        {
            int i;
            int j;

            for (i = 0, j = 0; i < digits; ++i, j += 4)
            {
                ref var element = ref x[i];

                block[j] = (byte)(element & 0xff);
                block[j + 1] = (byte)((element >> 8) & 0xff);
                block[j + 2] = (byte)((element >> 16) & 0xff);
                block[j + 3] = (byte)((element >> 24) & 0xff);
            }
        }
    }
}
