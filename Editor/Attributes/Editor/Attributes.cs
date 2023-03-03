using System;

namespace PancakeEditor.Attribute
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterValueDrawerAttribute : System.Attribute
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
    public class RegisterAttributeDrawerAttribute : System.Attribute
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
    public class RegisterGroupDrawerAttribute : System.Attribute
    {
        public RegisterGroupDrawerAttribute(Type drawerType) { DrawerType = drawerType; }

        public Type DrawerType { get; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterPropertyHideProcessor : System.Attribute
    {
        public RegisterPropertyHideProcessor(Type processorType) { ProcessorType = processorType; }

        public Type ProcessorType { get; }
        public bool ApplyOnArrayElement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterPropertyDisableProcessor : System.Attribute
    {
        public RegisterPropertyDisableProcessor(Type processorType) { ProcessorType = processorType; }

        public Type ProcessorType { get; }
        public bool ApplyOnArrayElement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterValueValidatorAttribute : System.Attribute
    {
        public RegisterValueValidatorAttribute(Type validatorType) { ValidatorType = validatorType; }

        public Type ValidatorType { get; }
        public bool ApplyOnArrayElement { get; set; } = true;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterAttributeValidatorAttribute : System.Attribute
    {
        public RegisterAttributeValidatorAttribute(Type validatorType) { ValidatorType = validatorType; }

        public Type ValidatorType { get; }
        public bool ApplyOnArrayElement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterTypeProcessorAttribute : System.Attribute
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