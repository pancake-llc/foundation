using Pancake.Attribute;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.IAP
{
    [EditorIcon("scriptable_variable")]
    [CreateAssetMenu(fileName = "scriptable_variable_product.asset", menuName = "Pancake/IAP/Product Varriable")]
    [System.Serializable]
    public class ProductVarriable : ScriptableVariable<IAPData>
    {
    }
}