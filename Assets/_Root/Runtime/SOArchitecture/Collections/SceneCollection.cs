using UnityEngine;

namespace Pancake.SOA
{
    [CreateAssetMenu(
        fileName = "SceneCollection.asset",
        menuName = SOArchitecture_Utility.ADVANCED_VARIABLE_COLLECTION + "Scene",
        order = 120)]
    public class SceneCollection : Collection<SceneInfo>
    {

    }
}