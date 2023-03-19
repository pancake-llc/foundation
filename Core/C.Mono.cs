using System.Collections;
using UnityEngine;

namespace Pancake
{
    public static partial class C
    {
        public static CoroutineHandle RunCoroutine(this MonoBehaviour owner, IEnumerator coroutine) { return new CoroutineHandle(owner, coroutine); }
        
    }
}