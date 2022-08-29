using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.Database
{
    public class DashboardDragManipulator : PointerManipulator
    {
        private Vector2 PointerStartPos { get; set; }
        private bool IsDragging { get; set; }

        private static Label m_floaterElement;
        private static VisualElement m_rootWindow;
        private readonly VisualElement m_originElement;
        private GroupFoldableButton m_currentHoverTarget;
        private bool m_isPressed;

        public DashboardDragManipulator(VisualElement origin, VisualElement rootWindow)
        {
            m_originElement = origin;
            m_rootWindow = rootWindow;

            if (m_floaterElement == null) m_floaterElement = new Label();
            m_floaterElement.name = "Floater Drag";
            m_floaterElement.text = "";
            m_floaterElement.style.height = 0;
            m_floaterElement.style.width = 0;
            m_floaterElement.style.position = new StyleEnum<Position>(Position.Absolute);
            m_floaterElement.pickingMode = PickingMode.Ignore;

            m_floaterElement.SetEnabled(false);
        }

        protected override void RegisterCallbacksOnTarget()
        {
            m_originElement.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            m_originElement.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
            m_originElement.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            m_originElement.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
            m_originElement.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
            m_originElement.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
        }

        private void PointerDownHandler(PointerDownEvent evt)
        {
            PointerStartPos = evt.position;
            m_isPressed = true;
        }

        private void PointerMoveHandler(PointerMoveEvent evt)
        {
            if (!m_isPressed)
            {
                return;
            }

            if (!IsDragging && Vector2.Distance(evt.position, PointerStartPos) > 30)
            {
                IsDragging = true;

                m_originElement.CapturePointer(evt.pointerId);
                m_rootWindow.Add(m_floaterElement);

                m_floaterElement.SetEnabled(true);
                m_floaterElement.transform.position = PointerStartPos;
                m_floaterElement.text = m_originElement.Q<Label>().text;
                m_floaterElement.BringToFront();
            }

            if (!IsDragging)
            {
                return;
            }

            m_floaterElement.transform.position = (Vector2) evt.position;
            VisualElement hovered = m_rootWindow.panel.Pick(evt.position);

            if (hovered is Button && hovered.parent is GroupFoldableButton groupbutton &&
                FilterColumn.CustomGroupsFoldout.Q<GroupFoldableButton>(groupbutton.name) != null)
            {
                m_floaterElement.style.color = new StyleColor(Color.white);
                m_floaterElement.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
                m_currentHoverTarget = groupbutton;
            }
            else
            {
                m_floaterElement.style.color = new StyleColor(Color.red);
                m_floaterElement.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal);
                m_currentHoverTarget = null;
            }
        }

        private void PointerUpHandler(PointerUpEvent evt)
        {
            if (m_rootWindow != null && m_floaterElement != null && m_floaterElement.parent == m_rootWindow) m_rootWindow.Remove(m_floaterElement);
            m_originElement?.ReleasePointer(evt.pointerId);
            m_floaterElement?.SetEnabled(false);

            if (m_currentHoverTarget != null)
            {
                m_currentHoverTarget.Group.Add(Dashboard.CurrentSelectedEntity);
            }

            IsDragging = false;
            m_isPressed = false;
        }
    }
}