namespace Pancake.Editor.Guide
{
    using UnityEngine;

    public class Validators_SceneObjectsOnlySample : ScriptableObject
    {
        [SceneObjectsOnly] public GameObject obj;
    }
}