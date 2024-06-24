using System;

namespace PancakeEditor.Sound
{
    public abstract class ReflectedClass
    {
        private readonly object _instance;
        private readonly Type _instanceType;

        public ReflectedClass(Type type)
        {
            _instanceType = type;
            _instance = ReflectionExtension.CreateNewObjectWithReflection(_instanceType, ReflectionExtension.Void);
        }

        public ReflectedClass(object instance)
        {
            _instanceType = instance.GetType();
            _instance = instance;
        }

        protected void SetInstanceProperty(string propertyName, object value) { ReflectionExtension.SetProperty(propertyName, _instanceType, _instance, value); }

        protected T GetInstanceProperty<T>(string propertyName) { return ReflectionExtension.GetProperty<T>(propertyName, _instanceType, _instance); }

        protected void SetInstanceField(string fieldName, object value) { ReflectionExtension.SetField(fieldName, _instanceType, _instance, value); }

        protected T GetInstanceField<T>(string fieldName) { return ReflectionExtension.GetField<T>(fieldName, _instanceType, _instance); }

        protected object ExecuteInstanceMethod(string methodName, object[] parameters)
        {
            return ReflectionExtension.ExecuteMethod(methodName, parameters, _instanceType, _instance);
        }

        public object Instance => _instance;
    }
}