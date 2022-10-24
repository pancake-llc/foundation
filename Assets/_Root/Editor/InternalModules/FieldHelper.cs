using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Pancake.Editor
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

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                Type[] types = assembly.GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    Type type = types[j];

                    IEnumerable<DrawerTarget> drawerTargets = type.GetCustomAttributes<DrawerTarget>();
                    if (drawerTargets != null)
                    {
                        foreach (DrawerTarget drawerTarget in drawerTargets)
                        {
                            if (Activator.CreateInstance(type) is FieldDrawer drawer)
                            {
                                Drawers.Add(drawerTarget.type, drawer);
                                if (drawerTarget.Subclasses)
                                {
                                    foreach (Type subType in drawerTarget.type.GetAllSubClass())
                                    {
                                        Drawers.Add(subType, drawer);
                                    }
                                }
                            }
                        }
                    }

                    ViewTarget viewTarget = type.GetCustomAttribute<ViewTarget>();
                    if (viewTarget != null)
                    {
                        if (Activator.CreateInstance(type) is FieldView view)
                        {
                            Views.Add(viewTarget.target, view);
                        }
                    }

                    ValidatorTarget validatorTarget = type.GetCustomAttribute<ValidatorTarget>();
                    if (validatorTarget != null)
                    {
                        if (Activator.CreateInstance(type) is FieldValidator validator)
                        {
                            Validators.Add(validatorTarget.target, validator);
                        }
                    }

                    DecoratorTarget decoratorTarget = type.GetCustomAttribute<DecoratorTarget>();
                    if (decoratorTarget != null)
                    {
                        if (Activator.CreateInstance(type) is FieldDecorator decorator)
                        {
                            Decorators.Add(decoratorTarget.target, decorator);
                        }
                    }

                    InlineDecoratorTarget inlineDecoratorTarget = type.GetCustomAttribute<InlineDecoratorTarget>();
                    if (inlineDecoratorTarget != null)
                    {
                        if (Activator.CreateInstance(type) is FieldInlineDecorator inlineDecorator)
                        {
                            InlineDecorators.Add(inlineDecoratorTarget.target, inlineDecorator);
                        }
                    }

                    ManipulatorTarget ManipulatorTarget = type.GetCustomAttribute<ManipulatorTarget>();
                    if (ManipulatorTarget != null)
                    {
                        if (Activator.CreateInstance(type) is MemberManipulator manipulator)
                        {
                            Manipulators.Add(ManipulatorTarget.target, manipulator);
                        }
                    }
                }
            }
        }
    }
}