using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public static class SobolData
    {
        /**
         * The first few lines of data taken from https://web.maths.unsw.edu.au/~fkuo/sobol/new-joe-kuo-6.21201
         * These are used to calculate the direction vectors for the first 4 dimensions.
         * If more are needed then subsequent lines can be copied from the source.
         */
        internal static uint[][] dirData = new uint[][] {new uint[] {2, 1, 0, 1}, new uint[] {3, 2, 1, 1, 3}, new uint[] {4, 3, 1, 1, 3, 1}};

        public static int MaxDimensions => dirData.Length + 1;

        public static uint[][] GenerateDirectionVectors(int dimensions, int maxBitsNeeded)
        {
            if (dimensions < 1 || dimensions > MaxDimensions)
            {
                throw new System.ArgumentException("Dimensions must be between 1 and " + MaxDimensions);
            }

            var v = new uint[dimensions][];
            v[0] = new uint[maxBitsNeeded + 1];
            for (uint i = 1; i <= maxBitsNeeded; i++) v[0][i] = 1u << (32 - (int) i);
            for (uint j = 1; j < dimensions; j++)
            {
                v[j] = new uint[maxBitsNeeded + 1];
                var line = dirData[j - 1];
                var d = line[0];
                var s = line[1];
                var a = line[2];
                uint[] m = new uint[s + 1];
                for (uint i = 1; i <= s; i++) m[i] = line[i + 2];

                if (maxBitsNeeded <= s)
                {
                    for (uint i = 1; i <= maxBitsNeeded; i++) v[j][i] = m[i] << (32 - (int) i);
                }
                else
                {
                    for (uint i = 1; i <= s; i++) v[j][i] = m[i] << (32 - (int) i);
                    for (uint i = s + 1; i <= maxBitsNeeded; i++)
                    {
                        v[j][i] = v[j][i - s] ^ (v[j][i - s] >> (int) s);
                        for (uint k = 1; k <= s - 1; k++)
                            v[j][i] ^= (((a >> (int) (s - 1 - k)) & 1) * v[j][i - k]);
                    }
                }
            }

            return v;
        }
    }
}