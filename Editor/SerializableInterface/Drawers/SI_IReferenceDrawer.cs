using UnityEngine;

namespace Pancake.Editor
{
    internal interface SI_IReferenceDrawer
    {
        float GetHeight();
        void OnGUI(Rect position);
    }
}