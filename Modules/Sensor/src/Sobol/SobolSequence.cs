using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public class SobolSequence
    {
        const int L = 32;
        internal static uint[][] V = SobolData.GenerateDirectionVectors(SobolData.MaxDimensions, L);

        int dimension;
        uint currIndex;
        uint[] currPoint;
        double[] currFracPoint;

        public SobolSequence(int dimension)
        {
            this.dimension = dimension;
            currIndex = 1;
            currPoint = new uint[this.dimension];
            currFracPoint = new double[this.dimension];
        }

        public double[] Next()
        {
            var zeroBit = RightmostZeroBitPosition(currIndex);
            if (zeroBit > L)
            {
                // Sequence is exhausted, must restart.
                currIndex = 1;
                zeroBit = RightmostZeroBitPosition(currIndex);
            }

            for (int i = 0; i < dimension; i++)
            {
                currPoint[i] = currPoint[i] ^ V[i][zeroBit];
                currFracPoint[i] = (double) currPoint[i] / Math.Pow(2.0, 32);
            }

            currIndex++;
            return currFracPoint;
        }

        uint RightmostZeroBitPosition(uint number)
        {
            uint i = 1;
            uint value = number;
            while ((value & 1) != 0)
            {
                value >>= 1;
                i++;
            }

            return i;
        }
    }
}