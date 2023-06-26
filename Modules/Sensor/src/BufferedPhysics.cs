using UnityEngine;
using System.Collections.Generic;

namespace Pancake.Sensor
{
    public interface ITestNonAlloc<S, T> where S : Sensor
    {
        int Test(S sensor, T[] results);
    }

    public class PhysicsNonAlloc<T>
    {
        public static int InitialSize = 20;
        public static bool DynamicallyIncreaseBufferSize = true;

        public T[] Buffer { get; private set; }
        public int Count { get; private set; }

        public bool IsAtCapacity { get { return Count == Buffer.Length; } }

        public PhysicsNonAlloc() { Expand(Mathf.Max(InitialSize, 1)); }

        public int PerformTest<S>(S sensor, ITestNonAlloc<S, T> tester) where S : Sensor
        {
            Count = tester.Test(sensor, Buffer);

            if (Count == Buffer.Length && DynamicallyIncreaseBufferSize)
            {
                Expand(Count * 2);
                return PerformTest(sensor, tester);
            }

            return Count;
        }

        void Expand(int toSize) { Buffer = new T[toSize]; }
    }
}