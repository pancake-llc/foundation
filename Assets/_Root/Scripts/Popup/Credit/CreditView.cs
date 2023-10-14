using System.Collections.Generic;
using Pancake.Apex;
using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    public sealed class CreditView : View
    {
        [SerializeField, Array] private List<CreditElement> elements;
        protected override UniTask Initialize() { return UniTask.CompletedTask; }
    }
}