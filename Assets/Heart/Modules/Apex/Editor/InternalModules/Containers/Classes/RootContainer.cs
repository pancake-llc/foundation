using Pancake.Apex;
using Pancake.ExLib.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    public sealed class RootContainer : ListContainer, IRootContainer
    {
        private SerializedObject serializedObject;
        private bool objectChanged;
        private bool guiChanged;

        internal RootContainer(string name, SerializedObject serializedObject, UnityAction repaint)
            : base(name)
        {
            this.serializedObject = serializedObject;
            this.Repaint = repaint;

            int order = 0;
            FindFields(in serializedObject, ref entities, ref order, in Repaint);
            FindMethods(in serializedObject, ref entities, ref order, in Repaint);
            FindProperties(in serializedObject, ref entities, ref order, in Repaint);

            entities.TrimExcess();
            entities.Sort();

            RegisterFieldCallbacks(ref entities);
            LayoutBoxGroups(ref entities, in Repaint);
            LayoutTabGroups(ref entities, in Repaint);
            LayoutFoldout(ref entities, in Repaint);
            LayoutHorizontalGroup(ref entities, in Repaint);
            RegisterGUICallbacks(entities);
        }

        #region [VisualEntity Implementation]

        /// <summary>
        /// Called for rendering and handling root container entities.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public override void OnGUI(Rect position)
        {
            DrawEntities(position, in entities);

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Total height required to draw root container entities.
        /// </summary>
        public override float GetHeight() { return GetEntitiesHeight(in entities); }

        #endregion

        #region [IRootContainer Implemetation]

        public void DoLayout()
        {
            objectChanged = false;
            guiChanged = false;

            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].DrawLayout();
            }

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion

        /// <summary>
        /// Register serialized field OnValueChanged events to notify that target object has been changed.
        /// </summary>
        /// <param name="entities"></param>
        private void RegisterFieldCallbacks(ref List<VisualEntity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                VisualEntity entity = entities[i];
                if (entity is SerializedField field)
                {
                    field.ValueChanged += OnValueChanged;
                }
            }
        }

        /// <summary>
        /// Register serialized field OnValueChanged events to notify that target object has been changed.
        /// </summary>
        /// <param name="entities"></param>
        private void RegisterGUICallbacks(IEnumerable<VisualEntity> entities)
        {
            foreach (VisualEntity entity in entities)
            {
                if (entity is Container container)
                {
                    RegisterGUICallbacks(container.Entities);
                }

                if (entity is IGUIChangedCallback callback)
                {
                    callback.GUIChanged += OnGUIChanged;
                }
            }
        }

        /// <summary>
        /// Serialized field OnValueChanged event to notify that target object has been changed.
        /// </summary>
        private void OnValueChanged(object value) { objectChanged = true; }

        /// <summary>
        /// Serialized member OnGUIChanged event to notify that target object GUI has been changed.
        /// </summary>
        private void OnGUIChanged() { guiChanged = true; }

        #region [Static Methods]

        /// <summary>
        /// Find all fields in serialized object type.
        /// </summary>
        /// <param name="serializedObject">Serialized object passed by reference.</param>
        /// <param name="visualEntities">Visual entities list passed by reference.</param>
        /// <param name="order">Current order passed by reference.</param>
        /// <param name="repaint">Repaint unity action delegate passed by reference.</param>
        internal static void FindFields(in SerializedObject serializedObject, ref List<VisualEntity> visualEntities, ref int order, in UnityAction repaint)
        {
            Type type = serializedObject.targetObject.GetType();
            bool hideMonoScript = type.GetCustomAttribute<HideMonoScriptAttribute>() != null;

            using (SerializedProperty iterator = serializedObject.GetIterator())
            {
                if (iterator.NextVisible(true))
                {
                    do
                    {
                        if (iterator.name == "m_Script" && hideMonoScript)
                        {
                            continue;
                        }

                        try
                        {
                            SerializedField serializedField = new SerializedField(serializedObject, iterator.propertyPath) {Repaint = repaint};
                            
                            serializedField.SetOrder(order++);
                            visualEntities.Add(serializedField);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(
                                $"Failed to create a serialized field on the path: {iterator.propertyPath} (Field has been ignored)\nException: <i>{ex.Message}</i>");
                        }
                    } while (iterator.NextVisible(false));
                }
            }
        }

        /// <summary>
        /// Find all methods in serialized object type.
        /// </summary>
        /// <param name="serializedObject">Serialized object passed by reference.</param>
        /// <param name="visualEntities">Visual entities list passed by reference.</param>
        /// <param name="order">Current order passed by reference.</param>
        /// <param name="repaint">Repaint unity action delegate passed by reference.</param>
        internal static void FindMethods(in SerializedObject serializedObject, ref List<VisualEntity> visualEntities, ref int order, in UnityAction repaint)
        {
            var target = serializedObject.targetObject;
            var type = target.GetType();
            var limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
            foreach (MethodInfo methodInfo in type.AllMethods(limitDescendant))
            {
                MethodButtonAttribute methodButtonAttribute = methodInfo.GetCustomAttribute<MethodButtonAttribute>();
                if (methodButtonAttribute != null)
                {
                    if (methodButtonAttribute is ButtonAttribute)
                    {
                        try
                        {
                            MethodButton button = new Button(serializedObject, methodInfo.Name) {Repaint = repaint};

                            button.SetOrder(order++);
                            visualEntities.Add(button);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(
                                $"Failed to create a method button <b>{methodInfo.Name}</b> of the {serializedObject.targetObject.GetType().Name} object. (Button has been ignored)\n<b><color=red>Exception: {ex.Message}</color></b>\n\nStacktrace:");
                        }
                    }
                    else if (methodButtonAttribute is RepeatButtonAttribute)
                    {
                        try
                        {
                            MethodButton button = new RepeatButton(serializedObject, methodInfo.Name) {Repaint = repaint};

                            button.SetOrder(order++);
                            visualEntities.Add(button);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(
                                $"Failed to create a method button <b>{methodInfo.Name}</b> of the {serializedObject.targetObject.GetType().Name} object. (Button has been ignored)\n<b><color=red>Exception: {ex.Message}</color></b>\n\nStacktrace:");
                        }
                    }
                    else if (methodButtonAttribute is ToggleButtonAttribute)
                    {
                        try
                        {
                            MethodButton button = new ToggleButton(serializedObject, methodInfo.Name) {Repaint = repaint};

                            button.SetOrder(order++);
                            visualEntities.Add(button);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(
                                $"Failed to create a method button <b>{methodInfo.Name}</b> of the {serializedObject.targetObject.GetType().Name} object. (Button has been ignored)\n<b><color=red>Exception: {ex.Message}</color></b>\n\nStacktrace:");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find all properties in serialized object type.
        /// </summary>
        /// <param name="serializedObject">Serialized object passed by reference.</param>
        /// <param name="visualEntities">Visual entities list passed by reference.</param>
        /// <param name="order">Current order passed by reference.</param>
        /// <param name="repaint">Repaint unity action delegate passed by reference.</param>
        internal static void FindProperties(in SerializedObject serializedObject, ref List<VisualEntity> visualEntities, ref int order, in UnityAction repaint)
        {
            var target = serializedObject.targetObject;
            var type = target.GetType();
            var limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);

            foreach (PropertyInfo propertyInfo in type.AllProperties(limitDescendant))
            {
                SerializePropertyAttribute attribute = propertyInfo.GetCustomAttribute<SerializePropertyAttribute>();
                if (attribute != null)
                {
                    try
                    {
                        SerializedCsProperty serializedCsProperty = new SerializedCsProperty(serializedObject, propertyInfo.Name) {Repaint = repaint};

                        serializedCsProperty.SetOrder(order++);
                        visualEntities.Add(serializedCsProperty);
                    }
                    catch
                    {
                        Debug.LogError(
                            $"Failed to create a serialized property {propertyInfo.Name} of the {serializedObject.targetObject.GetType().Name} object. (Property has been ignored)");
                    }
                }
            }
        }

        /// <summary>
        /// Find groups attributes in visual entities and layout all of them.
        /// </summary>
        /// <param name="visualEntities">Visual entities list passed by reference.</param>
        /// <param name="repaint">Repaint unity action delegate passed by reference.</param>
        internal static void LayoutBoxGroups(ref List<VisualEntity> visualEntities, in UnityAction repaint)
        {
            IL_01:
            for (int i = 0; i < visualEntities.Count; i++)
            {
                VisualEntity visualElement = visualEntities[i];

                if (visualElement is SerializedMember serializedMember)
                {
                    GroupAttribute attribute = serializedMember.GetAttribute<GroupAttribute>();
                    if (attribute != null)
                    {
                        GroupContainer boxGroup = visualEntities.Where(e => e is GroupContainer bg && bg.GetName() == attribute.Name).FirstOrDefault() as GroupContainer;
                        if (boxGroup != null)
                        {
                            boxGroup.AddEntity(visualElement);
                            visualEntities.RemoveAt(i);
                            goto IL_01;
                        }

                        boxGroup = new GroupContainer(attribute.Name, new List<VisualEntity>() {visualElement}) {Repaint = repaint};
                        boxGroup.SetOrder(visualElement.GetOrder());
                        visualEntities[i] = boxGroup;
                        goto IL_01;
                    }
                }
            }
        }

        /// <summary>
        /// Find tab groups attributes in visual entities and layout all of them.
        /// </summary>
        /// <param name="visualEntities">Visual entities list passed by reference.</param>
        /// <param name="repaint">Repaint unity action delegate passed by reference.</param>
        internal static void LayoutTabGroups(ref List<VisualEntity> visualEntities, in UnityAction repaint)
        {
            IL_01:
            for (int i = 0; i < visualEntities.Count; i++)
            {
                VisualEntity visualElement = visualEntities[i];
                if (visualElement is SerializedMember serializedMember)
                {
                    TabGroupAttribute attribute = serializedMember.GetAttribute<TabGroupAttribute>();
                    if (attribute != null)
                    {
                        TabContainer tabGroup = visualEntities.Where(e => e is TabContainer tg && tg.GetName() == attribute.Name).FirstOrDefault() as TabContainer;
                        if (tabGroup != null)
                        {
                            tabGroup.AddEntity(attribute.Key, visualElement);
                            visualEntities.RemoveAt(i);
                            goto IL_01;
                        }

                        tabGroup = new TabContainer(attribute.Name, new List<TabContainer.Tab>()) {Repaint = repaint};
                        tabGroup.AddEntity(attribute.Key, visualElement);
                        tabGroup.SetOrder(visualElement.GetOrder());
                        visualEntities[i] = tabGroup;
                        goto IL_01;
                    }
                }
                else if (visualElement is GroupContainer boxGroup)
                {
                    IL_02:
                    for (int j = 0; j < boxGroup.GetEntityCount(); j++)
                    {
                        VisualEntity child = boxGroup.GetEntity(j);
                        if (child is SerializedMember childMember)
                        {
                            TabGroupAttribute attribute = childMember.GetAttribute<TabGroupAttribute>();
                            if (attribute != null)
                            {
                                TabContainer tabGroup =
                                    boxGroup.GetEntities().Where(e => e is TabContainer tg && tg.GetName() == attribute.Name).FirstOrDefault() as TabContainer;
                                if (tabGroup != null)
                                {
                                    tabGroup.AddEntity(attribute.Key, child);
                                    boxGroup.RemoveEntity(j);
                                    goto IL_02;
                                }

                                tabGroup = new TabContainer(attribute.Name, new List<TabContainer.Tab>()) {Repaint = repaint};
                                tabGroup.AddEntity(attribute.Key, child);
                                tabGroup.SetOrder(child.GetOrder());
                                boxGroup.SetEntity(j, tabGroup);
                                goto IL_02;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find foldout attributes in visual entities and layout all of them.
        /// </summary>
        /// <param name="visualEntities">Visual entities list passed by reference.</param>
        /// <param name="repaint">Repaint unity action delegate passed by reference.</param>
        internal static void LayoutFoldout(ref List<VisualEntity> visualEntities, in UnityAction repaint)
        {
            IL_01:
            for (int i = 0; i < visualEntities.Count; i++)
            {
                VisualEntity visualElement = visualEntities[i];
                if (visualElement is SerializedMember serializedMember)
                {
                    FoldoutAttribute attribute = serializedMember.GetAttribute<FoldoutAttribute>();
                    if (attribute != null)
                    {
                        FoldoutContainer foldout =
                            visualEntities.Where(e => e is FoldoutContainer f && f.GetName() == attribute.Name).FirstOrDefault() as FoldoutContainer;
                        if (foldout != null)
                        {
                            foldout.AddEntity(visualElement);
                            visualEntities.RemoveAt(i);
                            goto IL_01;
                        }

                        foldout = new FoldoutContainer(attribute.Name, attribute.Style, new List<VisualEntity>() {visualElement}) {Repaint = repaint};
                        visualEntities[i] = foldout;
                        foldout.SetOrder(visualElement.GetOrder());
                        goto IL_01;
                    }
                }
                else if (visualElement is TabContainer tabGroup)
                {
                    IL_02:
                    foreach (TabContainer.Tab pair in tabGroup.GetTabs())
                    {
                        for (int j = 0; j < pair.entities.Count; j++)
                        {
                            VisualEntity child = pair.entities[j];
                            if (child is SerializedMember childMember)
                            {
                                FoldoutAttribute attribute = childMember.GetAttribute<FoldoutAttribute>();
                                if (attribute != null)
                                {
                                    FoldoutContainer foldout =
                                        tabGroup.GetTabs()
                                            .Where(d => d.name == pair.name)
                                            .SelectMany(m => m.entities)
                                            .Where(e => e is FoldoutContainer f && f.GetName() == attribute.Name)
                                            .FirstOrDefault() as FoldoutContainer;
                                    if (foldout != null)
                                    {
                                        foldout.AddEntity(child);
                                        pair.entities.RemoveAt(j);
                                        goto IL_02;
                                    }

                                    foldout = new FoldoutContainer(attribute.Name, attribute.Style, new List<VisualEntity>() {child}) {Repaint = repaint};
                                    foldout.SetOrder(child.GetOrder());
                                    pair.entities[j] = foldout;
                                    goto IL_02;
                                }
                            }
                        }
                    }
                }
                else if (visualElement is GroupContainer boxGroup)
                {
                    IL_03:
                    for (int k = 0; k < boxGroup.GetEntityCount(); k++)
                    {
                        VisualEntity child = boxGroup.GetEntity(k);
                        if (child is SerializedMember childMember)
                        {
                            FoldoutAttribute attribute = childMember.GetAttribute<FoldoutAttribute>();
                            if (attribute != null)
                            {
                                FoldoutContainer foldout =
                                    boxGroup.GetEntities().Where(e => e is FoldoutContainer f && f.GetName() == attribute.Name).FirstOrDefault() as FoldoutContainer;
                                if (foldout != null)
                                {
                                    foldout.AddEntity(child);
                                    boxGroup.RemoveEntity(k);
                                    goto IL_03;
                                }

                                foldout = new FoldoutContainer(attribute.Name, attribute.Style, new List<VisualEntity>() {child}) {Repaint = repaint};
                                foldout.SetOrder(child.GetOrder());
                                boxGroup.SetEntity(k, foldout);
                                goto IL_03;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find horizontal groups attributes in visual entities and layout all of them.
        /// </summary>
        /// <param name="visualEntities">Visual entities list passed by reference.</param>
        /// <param name="repaint">Repaint unity action delegate passed by reference.</param>
        internal static void LayoutHorizontalGroup(ref List<VisualEntity> visualEntities, in UnityAction repaint)
        {
            IL_01:
            for (int i = 0; i < visualEntities.Count; i++)
            {
                VisualEntity visualElement = visualEntities[i];
                if (visualElement is SerializedMember serializedMember)
                {
                    HorizontalGroupAttribute attribute = serializedMember.GetAttribute<HorizontalGroupAttribute>();
                    if (attribute != null)
                    {
                        HorizontalContainer horizontalGroup =
                            visualEntities.Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name).FirstOrDefault() as HorizontalContainer;
                        if (horizontalGroup != null)
                        {
                            horizontalGroup.AddEntity(visualElement);
                            visualEntities.RemoveAt(i);
                            goto IL_01;
                        }

                        horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {visualElement}) {Repaint = repaint};
                        horizontalGroup.SetOrder(visualElement.GetOrder());
                        visualEntities[i] = horizontalGroup;
                        goto IL_01;
                    }
                }
                else if (visualElement is TabContainer tabGroup)
                {
                    IL_02:
                    foreach (TabContainer.Tab pair in tabGroup.GetTabs())
                    {
                        for (int j = 0; j < pair.entities.Count; j++)
                        {
                            VisualEntity child = pair.entities[j];
                            if (child is SerializedMember childMember)
                            {
                                HorizontalGroupAttribute attribute = childMember.GetAttribute<HorizontalGroupAttribute>();
                                if (attribute != null)
                                {
                                    HorizontalContainer horizontalGroup =
                                        tabGroup.GetTabs()
                                            .Where(d => d.name == pair.name)
                                            .SelectMany(m => m.entities)
                                            .Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name)
                                            .FirstOrDefault() as HorizontalContainer;
                                    if (horizontalGroup != null)
                                    {
                                        horizontalGroup.AddEntity(child);
                                        pair.entities.RemoveAt(j);
                                        goto IL_02;
                                    }

                                    horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {child}) {Repaint = repaint};
                                    horizontalGroup.SetOrder(child.GetOrder());
                                    pair.entities[j] = horizontalGroup;
                                    goto IL_02;
                                }
                            }
                            else if (child is FoldoutContainer foldout)
                            {
                                IL_0201:
                                for (int k = 0; k < foldout.GetEntityCount(); k++)
                                {
                                    VisualEntity fChild = foldout.GetEntity(k);
                                    if (fChild is SerializedMember fChildMember)
                                    {
                                        HorizontalGroupAttribute attribute = fChildMember.GetAttribute<HorizontalGroupAttribute>();
                                        if (attribute != null)
                                        {
                                            HorizontalContainer horizontalGroup =
                                                foldout.GetEntities().Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name).FirstOrDefault() as
                                                    HorizontalContainer;
                                            if (horizontalGroup != null)
                                            {
                                                horizontalGroup.AddEntity(fChild);
                                                foldout.RemoveEntity(k);
                                                goto IL_0201;
                                            }

                                            horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {fChild}) {Repaint = repaint};
                                            horizontalGroup.SetOrder(fChild.GetOrder());
                                            foldout.SetEntity(k, horizontalGroup);
                                            goto IL_0201;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (visualElement is GroupContainer boxGroup)
                {
                    IL_03:
                    for (int j = 0; j < boxGroup.GetEntityCount(); j++)
                    {
                        VisualEntity bgChild = boxGroup.GetEntity(j);
                        if (bgChild is SerializedMember bgChildMember)
                        {
                            HorizontalGroupAttribute attribute = bgChildMember.GetAttribute<HorizontalGroupAttribute>();
                            if (attribute != null)
                            {
                                HorizontalContainer horizontalGroup =
                                    boxGroup.GetEntities()
                                        .Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name)
                                        .FirstOrDefault() as HorizontalContainer;
                                if (horizontalGroup != null)
                                {
                                    horizontalGroup.AddEntity(bgChild);
                                    boxGroup.RemoveEntity(j);
                                    goto IL_03;
                                }

                                horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {bgChild}) {Repaint = repaint};
                                horizontalGroup.SetOrder(bgChild.GetOrder());
                                boxGroup.SetEntity(j, horizontalGroup);
                                goto IL_03;
                            }
                        }
                        else if (bgChild is FoldoutContainer foldout)
                        {
                            IL_0301:
                            for (int k = 0; k < foldout.GetEntityCount(); k++)
                            {
                                VisualEntity fChild = foldout.GetEntity(k);
                                if (fChild is SerializedMember fChildMember)
                                {
                                    HorizontalGroupAttribute attribute = fChildMember.GetAttribute<HorizontalGroupAttribute>();
                                    if (attribute != null)
                                    {
                                        HorizontalContainer horizontalGroup =
                                            foldout.GetEntities().Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name).FirstOrDefault() as
                                                HorizontalContainer;
                                        if (horizontalGroup != null)
                                        {
                                            horizontalGroup.AddEntity(fChild);
                                            foldout.RemoveEntity(k);
                                            goto IL_0301;
                                        }

                                        horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {fChild}) {Repaint = repaint};
                                        horizontalGroup.SetOrder(fChild.GetOrder());
                                        foldout.SetEntity(k, horizontalGroup);
                                        goto IL_0301;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (visualElement is FoldoutContainer foldout)
                {
                    IL_04:
                    for (int j = 0; j < foldout.GetEntityCount(); j++)
                    {
                        VisualEntity fChild = foldout.GetEntity(j);
                        if (fChild is SerializedMember fChildMember)
                        {
                            HorizontalGroupAttribute attribute = fChildMember.GetAttribute<HorizontalGroupAttribute>();
                            if (attribute != null)
                            {
                                HorizontalContainer horizontalGroup =
                                    foldout.GetEntities().Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name).FirstOrDefault() as HorizontalContainer;
                                if (horizontalGroup != null)
                                {
                                    horizontalGroup.AddEntity(fChild);
                                    foldout.RemoveEntity(j);
                                    goto IL_04;
                                }

                                horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {fChild}) {Repaint = repaint};
                                horizontalGroup.SetOrder(fChild.GetOrder());
                                foldout.SetEntity(j, horizontalGroup);
                                goto IL_04;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region [Getter / Setter]

        public SerializedObject GetSerializedObject() { return serializedObject; }

        /// <summary>
        /// Check that target object has been changed.
        /// </summary>
        public bool HasObjectChanged() { return objectChanged; }

        /// <summary>
        /// Check that target object GUI has been changed.
        /// </summary>
        public bool HasGUIChanged() { return guiChanged; }

        #endregion
    }
}