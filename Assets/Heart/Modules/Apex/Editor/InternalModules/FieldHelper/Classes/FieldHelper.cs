using Pancake.ExLib.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Pancake.ApexEditor
{
    [InitializeOnLoad]
    public static class FieldHelper
    {
        public readonly static Dictionary<Type, FieldDrawer> Drawers;
        public readonly static Dictionary<Type, FieldView> Views;
        public readonly static Dictionary<Type, FieldValidator> Validators;
        public readonly static Dictionary<Type, FieldDecorator> Decorators;
        public readonly static Dictionary<Type, FieldInlineDecorator> InlineDecorators;
        public readonly static Dictionary<Type, MemberManipulator> Manipulators;

        static FieldHelper()
        {
            Drawers = new Dictionary<Type, FieldDrawer>();
            Views = new Dictionary<Type, FieldView>();
            Validators = new Dictionary<Type, FieldValidator>();
            Decorators = new Dictionary<Type, FieldDecorator>();
            InlineDecorators = new Dictionary<Type, FieldInlineDecorator>();
            Manipulators = new Dictionary<Type, MemberManipulator>();

            TypeCache.TypeCollection drawerTargets = TypeCache.GetTypesWithAttribute<DrawerTarget>();
            for (int i = 0; i < drawerTargets.Count; i++)
            {
                Type type = drawerTargets[i];
                if (Activator.CreateInstance(type) is FieldDrawer drawer)
                {
                    DrawerTarget attribute = type.GetCustomAttribute<DrawerTarget>();
                    Drawers.Add(attribute.type, drawer);
                    if (attribute.Subclasses)
                    {
                        foreach (Type subType in attribute.type.Subclasses())
                        {
                            Drawers.Add(subType, drawer);
                        }
                    }
                }
            }

            Drawers.TrimExcess();

            TypeCache.TypeCollection viewTargets = TypeCache.GetTypesWithAttribute<ViewTarget>();
            for (int i = 0; i < viewTargets.Count; i++)
            {
                Type type = viewTargets[i];
                if (Activator.CreateInstance(type) is FieldView view)
                {
                    ViewTarget attribute = type.GetCustomAttribute<ViewTarget>();
                    Views.Add(attribute.target, view);
                }
            }

            Views.TrimExcess();

            TypeCache.TypeCollection validatorTargets = TypeCache.GetTypesWithAttribute<ValidatorTarget>();
            for (int i = 0; i < validatorTargets.Count; i++)
            {
                Type type = validatorTargets[i];
                if (Activator.CreateInstance(type) is FieldValidator validator)
                {
                    ValidatorTarget attribute = type.GetCustomAttribute<ValidatorTarget>();
                    Validators.Add(attribute.target, validator);
                }
            }

            Validators.TrimExcess();

            TypeCache.TypeCollection decoratorTargets = TypeCache.GetTypesWithAttribute<DecoratorTarget>();
            for (int i = 0; i < decoratorTargets.Count; i++)
            {
                Type type = decoratorTargets[i];
                if (Activator.CreateInstance(type) is FieldDecorator decorator)
                {
                    DecoratorTarget attribute = type.GetCustomAttribute<DecoratorTarget>();
                    Decorators.Add(attribute.target, decorator);
                }
            }

            Decorators.TrimExcess();

            TypeCache.TypeCollection inlineDecoratorTargets = TypeCache.GetTypesWithAttribute<InlineDecoratorTarget>();
            for (int i = 0; i < inlineDecoratorTargets.Count; i++)
            {
                Type type = inlineDecoratorTargets[i];
                if (Activator.CreateInstance(type) is FieldInlineDecorator inlineDecorator)
                {
                    InlineDecoratorTarget attribute = type.GetCustomAttribute<InlineDecoratorTarget>();
                    InlineDecorators.Add(attribute.target, inlineDecorator);
                }
            }

            InlineDecorators.TrimExcess();

            TypeCache.TypeCollection manipulatorsTargets = TypeCache.GetTypesWithAttribute<ManipulatorTarget>();
            for (int i = 0; i < manipulatorsTargets.Count; i++)
            {
                Type type = manipulatorsTargets[i];
                if (Activator.CreateInstance(type) is MemberManipulator manipulator)
                {
                    ManipulatorTarget attribute = type.GetCustomAttribute<ManipulatorTarget>();
                    Manipulators.Add(attribute.target, manipulator);
                }
            }

            Manipulators.TrimExcess();
        }
    }
}