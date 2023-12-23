namespace Pancake.Scriptable.ExLib
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "LayerNamesToMask", menuName = "Pancake/Misc/LayerNamesToMask")]
    public class LayerNamesToMask : ScriptableObject
    {
        public int AsMask
        {
            get
            {
                var resultMask = 0;

                for (var i = 0; i < LayerNames.Length; i++)
                {
                    string currentLayer = LayerNames[i].Value;

                    if (LayerMask.NameToLayer(currentLayer) != -1)
                    {
                        resultMask |= 1 << LayerMask.NameToLayer(currentLayer);
                    }
                }

                return resultMask;
            }
        }

        [field: SerializeField] public StringConstant[] LayerNames { get; protected set; }
    }
}