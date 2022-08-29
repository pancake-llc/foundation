using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Pancake.Database
{
    public class InspectorColumn : DashboardColumn
    {
        protected SerializedObject target;
        private readonly ScrollView _scroll;

        public InspectorColumn()
        {
            target = GetSerializedObj();
            _scroll = new ScrollView
            {
                style =
                {
                    flexShrink = 1f,
                    flexGrow = 1f,
                    paddingBottom = 10,
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 10
                }
            };

            Dashboard.onCurrentEntityChanged -= Rebuild;
            Dashboard.onCurrentEntityChanged += Rebuild;
            Dashboard.onDeleteEntityStart = InspectNothing;

            name = "Inspector";
            viewDataKey = "inspector";
            style.flexShrink = 1f;
            style.flexGrow = 1f;
            style.paddingBottom = 10;
            style.paddingLeft = 10;
            style.paddingRight = 10;
            style.paddingTop = 10;
            Add(_scroll);
        }

        public override void Rebuild()
        {
            _scroll.Clear();
            if (Dashboard.CurrentSelectedEntity == null)
            {
                InspectNothing();
                return;
            }

            target = GetSerializedObj();

            bool success = BuildInspectorProperties(target, _scroll);
            if (success) _scroll.Bind(target); // TODO BUG
        }

        private void InspectNothing()
        {
            _scroll.Clear();
            _scroll.Add(new Label {text = " ⓘ Inspector"});
            _scroll.Add(new Label("\n\n    ⚠ Please select an asset from the column to the left."));
        }

        private static SerializedObject GetSerializedObj()
        {
            return Dashboard.CurrentSelectedEntity == null ? null : UnityEditor.Editor.CreateEditor(Dashboard.CurrentSelectedEntity).serializedObject;
        }

        private static bool BuildInspectorProperties(SerializedObject obj, VisualElement wrapper)
        {
            if (obj == null || wrapper == null) return false;
            wrapper.Add(new Label {text = " ⓘ Inspector"});

            // if Unity ever makes their InspectorElement work then we can just use that instead of
            // butchering through the object and making each field manually. (since 2019)

            /*
            InspectorElement inspector = new InspectorElement(obj);
            inspector.style.flexGrow = 1;
            inspector.style.flexShrink = 1;
            inspector.style.alignSelf = new StyleEnum<Align>(Align.Stretch);
            inspector.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            inspector.style.alignItems = new StyleEnum<Align>(Align.Stretch);
            inspector.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            wrapper.Add(inspector);
            return true;
            */

            var iterator = obj.GetIterator();
            var targetType = obj.targetObject.GetType();
            var members = new List<MemberInfo>(targetType.GetMembers());

            if (!iterator.NextVisible(true)) return false;
            do
            {
                var isHidden = false;
                var propertyField = new PropertyField(iterator.Copy()) {name = "PropertyField:" + iterator.propertyPath};

                var member = members.Find(x => x.Name == propertyField.bindingPath);
                if (member != null)
                {
                    var hides = member.GetCustomAttributes(typeof(HideInInspector)).ToArray();
                    foreach (var _ in hides)
                    {
                        isHidden = true;
                    }

                    if (!isHidden)
                    {
                        var allAttributes = member.CustomAttributes.ToArray();
                        var headers = member.GetCustomAttributes(typeof(HeaderAttribute)).ToArray();
                        var spaces = member.GetCustomAttributes(typeof(SpaceAttribute)).ToArray();

                        // BUG seems like if there is more than 1 attribute, they all get drawn fine by Unity but 1 alone requires manual drawing?
                        if (allAttributes.Length < 2)
                        {
                            foreach (var h in headers)
                            {
                                var actual = (HeaderAttribute) h;
                                var header = new Label {text = actual.header, style = {unityFontStyleAndWeight = FontStyle.Bold}};
                                wrapper.Add(new Label {text = " "});
                                wrapper.Add(header);
                            }

                            foreach (var _ in spaces)
                            {
                                wrapper.Add(new Label {text = " "});
                            }
                        }
                    }
                }

                // for the db key field
                if (iterator.propertyPath == "id" && obj.targetObject != null)
                {
                    // build the container
                    var container = new VisualElement
                    {
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                            alignItems = new StyleEnum<Align>(Align.Stretch),
                            flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                        }
                    };

                    propertyField.SetEnabled(true);
                    propertyField.style.flexGrow = 1;
                    propertyField.style.flexShrink = 1;

                    // draw it
                    container.Add(propertyField);
                    wrapper.Add(container);
                }

                // if this property is the script field
                if (iterator.propertyPath == "m_Script" && obj.targetObject != null)
                {
                    // build the container
                    var container = new VisualElement
                    {
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                            alignItems = new StyleEnum<Align>(Align.Stretch),
                            flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                        }
                    };

                    propertyField.SetEnabled(false);
                    propertyField.style.flexGrow = 1;
                    propertyField.style.flexShrink = 1;

                    // build the focus script button
                    var focusButton = new Button(() =>
                    {
                        var objPing = obj.FindProperty("m_Script").objectReferenceValue;
                        EditorGUIUtility.PingObject(objPing);
                        Selection.activeObject = objPing;
                    }) {text = "☲", style = {minWidth = 20, maxWidth = 20}, tooltip = "Ping this Script"};

                    // build the focus object button
                    var focusAsset = new Button(() =>
                    {
                        EditorGUIUtility.PingObject(obj.targetObject);
                        Selection.activeObject = obj.targetObject;
                    }) {text = "☑", style = {minWidth = 20, maxWidth = 20}, tooltip = "Ping this Entity"};

                    // draw it
                    container.Add(propertyField);
                    container.Add(focusButton);
                    container.Add(focusAsset);
                    wrapper.Add(container);
                }
                // if it isn't the script field, just add the property field like normal.
                else if (!isHidden) wrapper.Add(propertyField);
            } while (iterator.NextVisible(false));

            return true;
        }
    }
}