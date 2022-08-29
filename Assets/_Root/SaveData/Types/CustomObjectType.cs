using System;

namespace Pancake.SaveData
{
    [UnityEngine.Scripting.Preserve]
    public abstract class CustomObjectType : CustomType
    {
        public CustomObjectType(Type type)
            : base(type)
        {
        }

        protected abstract void WriteObject(object obj, Writer writer);
        protected abstract object ReadObject<T>(Reader reader);

        protected virtual void ReadObject<T>(Reader reader, object obj) { throw new NotSupportedException("ReadInto is not supported for type " + type); }

        public override void Write(object obj, Writer writer)
        {
            if (!WriteUsingDerivedType(obj, writer))
            {
                var baseType = Reflection.BaseType(obj.GetType());
                if (baseType != typeof(object))
                {
                    var customType = TypeManager.GetOrCreateCustomType(baseType);
                    // If it's a Dictionary, we need to write it as a field with a property name.
                    if (customType.isDictionary || customType.isCollection)
                        writer.WriteProperty("_Values", obj, customType);
                }

                WriteObject(obj, writer);
            }
        }

        public override object Read<T>(Reader reader)
        {
            string propertyName;
            while (true)
            {
                propertyName = ReadPropertyName(reader);

                if (propertyName == CustomType.TYPE_FIELD_NAME)
                    return TypeManager.GetOrCreateCustomType(reader.ReadType()).Read<T>(reader);
                else if (propertyName == null)
                    return null;
                else
                {
                    reader.overridePropertiesName = propertyName;

                    return ReadObject<T>(reader);
                }
            }
        }

        public override void ReadInto<T>(Reader reader, object obj)
        {
            string propertyName;
            while (true)
            {
                propertyName = ReadPropertyName(reader);

                if (propertyName == CustomType.TYPE_FIELD_NAME)
                {
                    TypeManager.GetOrCreateCustomType(reader.ReadType()).ReadInto<T>(reader, obj);
                    return;
                }
                // This is important we return if the enumerator returns null, otherwise we will encounter an endless cycle.
                else if (propertyName == null)
                    return;
                else
                {
                    reader.overridePropertiesName = propertyName;
                    ReadObject<T>(reader, obj);
                }
            }
        }
    }
}