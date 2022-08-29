using UnityEngine.UIElements;

namespace Pancake.Database
{
    public abstract class DashboardColumn : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<VisualElement, UxmlTraits>
        {
        }

        public abstract void Rebuild();
    }
}