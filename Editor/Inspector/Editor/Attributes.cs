using System;

namespace Pancake.Editor
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterValueDrawerAttribute : Attribute
    {
        public RegisterValueDrawerAttribute(Type drawerType, int order)
        {
            DrawerType = drawerType;
            Order = order;
        }

        public Type DrawerType { get; }
        public int Order { get; }
        public bool ApplyOnArrayElement { get; set; } = true;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterAttributeDrawerAttribute : Attribute
    {
        public RegisterAttributeDrawerAttribute(Type drawerType, int order)
        {
            DrawerType = drawerType;
            Order = order;
        }

        public Type DrawerType { get; }
        public int Order { get; }
        public bool ApplyOnArrayElement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterGroupDrawerAttribute : Attribute
    {
        public RegisterGroupDrawerAttribute(Type drawerType) { DrawerType = drawerType; }

        public Type DrawerType { get; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterPropertyHideProcessor : Attribute
    {
        public RegisterPropertyHideProcessor(Type processorType) { ProcessorType = processorType; }

        public Type ProcessorType { get; }
        public bool ApplyOnArrayElement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterPropertyDisableProcessor : Attribute
    {
        public RegisterPropertyDisableProcessor(Type processorType) { ProcessorType = processorType; }

        public Type ProcessorType { get; }
        public bool ApplyOnArrayElement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterValueValidatorAttribute : Attribute
    {
        public RegisterValueValidatorAttribute(Type validatorType) { ValidatorType = validatorType; }

        public Type ValidatorType { get; }
        public bool ApplyOnArrayElement { get; set; } = true;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterAttributeValidatorAttribute : Attribute
    {
        public RegisterAttributeValidatorAttribute(Type validatorType) { ValidatorType = validatorType; }

        public Type ValidatorType { get; }
        public bool ApplyOnArrayElement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterTypeProcessorAttribute : Attribute
    {
        public RegisterTypeProcessorAttribute(Type processorType, int order)
        {
            ProcessorType = processorType;
            Order = order;
        }

        public Type ProcessorType { get; }
        public int Order { get; }
    }
}