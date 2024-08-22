using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.ComputationalGeometry
{
    public class LinkedVertex
    {
        public MyVector2 pos;

        public LinkedVertex prevLinkedVertex;
        public LinkedVertex nextLinkedVertex;

        public LinkedVertex(MyVector2 pos) { this.pos = pos; }
    }
}