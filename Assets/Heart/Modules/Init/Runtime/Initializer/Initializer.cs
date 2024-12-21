#pragma warning disable CS0414

using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.Internal
{
    // Base class for all Initializers; targeted by InitializerEditor.
    public abstract class Initializer : MonoBehaviour
    {
        internal abstract Object GetTarget();

        private protected void DestroySelfIfNotAsset()
        {
            if(!this)
            {
                return;
            }

#if UNITY_EDITOR
            if(gameObject.IsAsset(resultIfSceneObjectInEditMode: true))
            {
                return;
            }
#endif

            Destroy(this);
        }
    }
}