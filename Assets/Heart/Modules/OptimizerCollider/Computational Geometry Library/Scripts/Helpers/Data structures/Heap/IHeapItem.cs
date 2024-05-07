using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Pancake.ComputationalGeometry
{
    //Interface for each item in the heap
    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }
}