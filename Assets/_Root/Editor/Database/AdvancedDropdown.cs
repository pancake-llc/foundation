using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Pancake.Database
{
    internal class EntityDropdownItem : AdvancedDropdownItem
    {
        public Entity entity;

        public EntityDropdownItem(string name, Entity entity)
            : base(name)
        {
            this.entity = entity;
        }
    }

    internal class AdvancedDropdown : UnityEditor.IMGUI.Controls.AdvancedDropdown
    {
        /*
         TODO
         https://forum.unity.com/threads/dropdownfield-popup-height-in-runtime-how-to-limit.1197157/
         m_OuterContainer.style.height = Mathf.Min(300,
                    m_MenuContainer.layout.height - m_MenuContainer.layout.y - m_OuterContainer.layout.y,
                    m_ScrollView.layout.height + m_OuterContainer.resolvedStyle.borderBottomWidth + m_OuterContainer.resolvedStyle.borderTopWidth);
         */

        public SerializedProperty targetProperty;

        public AdvancedDropdown(AdvancedDropdownState state)
            : base(state)
        {
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(EntityDropdownDrawer.type.ToString());
            foreach (var data in EntityDropdownDrawer.content)
            {
                root.AddChild(new EntityDropdownItem(data.Title, data));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            var entity = ((EntityDropdownItem) item).entity;
            EntityDropdownDrawer.ItemSelected(targetProperty, entity);
            targetProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}