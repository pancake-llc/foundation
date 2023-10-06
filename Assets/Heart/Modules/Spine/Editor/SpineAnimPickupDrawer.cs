using Pancake.Spine;
using UnityEditor;

#if PANCAKE_SPINE
namespace Pancake.SpineEditor
{
    using UnityEngine;

    [CustomPropertyDrawer(typeof(SpineAnimPickupAttribute))]
    public class SpineAnimPickupDrawer : PropertyDrawer
    {

    }

}
#endif