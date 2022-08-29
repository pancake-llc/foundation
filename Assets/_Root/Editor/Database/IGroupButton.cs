using UnityEngine.UIElements;

namespace Pancake.Database
{
    public interface IGroupButton
    {
        string Title { get; set; }
        IGroup Group { get; set; }
        VisualElement MainElement { get; set; }
        VisualElement InternalElement { get; set; }

        void SetAsCurrent();
        void SetIsSelected(bool state);
        void SetIsHighlighted(bool state);
        void SetShowFoldout(bool show);
    }
}