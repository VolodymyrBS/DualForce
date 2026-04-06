using System;

namespace DualForce.LowLevel
{
    public static class Crc32LE
    {
        private const int CRC_LE_BITS = 8;
        private const uint CRCPOLY_LE = 0xedb88320;
        private static uint[] crc32table_le = new uint[
            (1 << CRC_LE_BITS) * (1 << CRC_LE_BITS)
        ];

        static Crc32LE()
        {
            uint i, j;
            uint crc = 1;

            crc32table_le[0] = 0;

            for (i = 1 << (CRC_LE_BITS - 1); i != 0; i >>= 1)
            {
                // coverity[overflow_const]
                crc = (crc >> 1) ^ ((crc & 1) != 0 ? CRCPOLY_LE : 0);
                for (j = 0; j < 1 << CRC_LE_BITS; j += 2 * i)
                    crc32table_le[i + j] = crc ^ crc32table_le[j];
            }
        }

        public static uint Calculate(uint crc, Span<byte> data)
        {
            foreach (var t in data)
            {
                crc = (crc >> 8) ^ crc32table_le[(crc ^ t) & 255];
            }

            return crc;
        }
    }
}