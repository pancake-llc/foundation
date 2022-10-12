using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pancake.Editor
{
    public static class InspectorReflection
    {
        public readonly static Dictionary<Type, FieldDrawer> Drawers;
        public readonly static Dictionary<Type, FieldView> Views;
        public readonly static Dictionary<Type, FieldValidator> Validators;
        public readonly static Dictionary<Type, FieldDecorator> Decorators;
        public readonly static Dictionary<Type, FieldInlineDecorator> InlineDecorators;
        public readonly static Dictionary<Type, MemberManipulator> Manipulators;

        static InspectorReflection()
        {
            Drawers = new Dictionary<Type, FieldDrawer>();
            Views = new Dictionary<Type, FieldView>();
            Validators = new Dictionary<Type, FieldValidator>();
            Decorators = new Dictionary<Type, FieldDecorator>();
            InlineDecorators = new Dictionary<Type, FieldInlineDecorator>();
            Manipulators = new Dictionary<Type, MemberManipulator>();

            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];

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
                                IEnumerable<Type> subTypes = FindSubclassesOf(drawerTarget.type);
                                foreach (Type subType in subTypes)
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

        public static IEnumerable<Type> FindSubclassesOf(Type type, bool directDescendants = false)
        {
            Assembly assembly = type.Assembly;
            Type[] types = assembly.GetTypes();
            IEnumerable<Type> subclasses = types.Where(t => directDescendants ? t.BaseType == type : t.IsSubclassOf(type));
            return subclasses;
        }

        public static IEnumerable<Type> FindSubclassesOf<T>(bool directDescendants = false)
        {
            Assembly assembly = typeof(T).Assembly;
            Type[] types = assembly.GetTypes();
            IEnumerable<Type> subclasses = types.Where(t => directDescendants ? t.BaseType == typeof(T) : t.IsSubclassOf(typeof(T)));
            return subclasses;
        }

        public static IEnumerable<Type> FindSubclassesOf<T>(Assembly assembly, bool directDescendants = false)
        {
            Type[] types = assembly.GetTypes();
            IEnumerable<Type> subclasses = types.Where(t => directDescendants ? t.BaseType == typeof(T) : t.IsSubclassOf(typeof(T)));
            return subclasses;
        }

        public static IEnumerable<MemberInfo> GetAllMembers(this Type type, BindingFlags flags)
        {
            do
            {
                MemberInfo[] memberInfos = type.GetMembers(flags);
                if (memberInfos != null)
                {
                    for (int i = 0; i < memberInfos.Length; i++)
                    {
                        yield return memberInfos[i];
                    }
                }

                type = type.BaseType;
            } while (type != null);
        }

        public static IEnumerable<MemberInfo> GetAllMembers(this Type type, string name, BindingFlags flags)
        {
            do
            {
                MemberInfo[] memberInfos = type.GetMembers(flags);
                if (memberInfos != null)
                {
                    for (int i = 0; i < memberInfos.Length; i++)
                    {
                        MemberInfo memberInfo = memberInfos[i];
                        if (memberInfo.Name == name)
                        {
                            yield return memberInfo;
                        }
                    }
                }

                type = type.BaseType;
            } while (type != null);
        }
    }
}