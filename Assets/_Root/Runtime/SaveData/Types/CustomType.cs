using System;
using System.Collections;
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Pancake.SaveData
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [UnityEngine.Scripting.Preserve]
    public abstract class CustomType
    {
        public const string TYPE_FIELD_NAME = "__type";

        public Member[] members;
        public Type type;
        public bool isPrimitive = false;
        public bool isValueType = false;
        public bool isCollection = false;
        public bool isDictionary = false;
        public bool isEnum = false;
        public bool isTypeUnityObject = false;
        public bool isReflectedType = false;
        public bool isUnsupported = false;
        public int priority = 0;

        protected CustomType(Type type)
        {
            TypeManager.Add(type, this);
            this.type = type;
            this.isValueType = Reflection.IsValueType(type);
        }

        public abstract void Write(object obj, Writer writer);
        public abstract object Read<T>(Reader reader);

        public virtual void ReadInto<T>(Reader reader, object obj)
        {
            throw new NotImplementedException("Self-assigning Read is not implemented or supported on this type.");
        }

        protected bool WriteUsingDerivedType(object obj, Writer writer)
        {
            var objType = obj.GetType();

            if (objType != this.type)
            {
                writer.WriteType(objType);
                TypeManager.GetOrCreateCustomType(objType).Write(obj, writer);
                return true;
            }

            return false;
        }

        protected void ReadUsingDerivedType<T>(Reader reader, object obj) { TypeManager.GetOrCreateCustomType(reader.ReadType()).ReadInto<T>(reader, obj); }

        internal string ReadPropertyName(Reader reader)
        {
            if (reader.overridePropertiesName != null)
            {
                string propertyName = reader.overridePropertiesName;
                reader.overridePropertiesName = null;
                return propertyName;
            }

            return reader.ReadPropertyName();
        }

        #region Reflection Methods

        protected void WriteProperties(object obj, Writer writer)
        {
            if (members == null) GetMembers(writer.metadata.safeReflection);
            for (int i = 0; i < members.Length; i++)
            {
                var property = members[i];
                writer.WriteProperty(property.name, property.reflectedMember.GetValue(obj), TypeManager.GetOrCreateCustomType(property.type));
            }
        }

        protected object ReadProperties(Reader reader, object obj)
        {
            // Iterate through each property in the file and try to load it using the appropriate
            // Member in the members array.
            foreach (string propertyName in reader.Properties)
            {
                // Find the property.
                Member property = null;
                for (int i = 0; i < members.Length; i++)
                {
                    if (members[i].name == propertyName)
                    {
                        property = members[i];
                        break;
                    }
                }

                // If this is a class which derives directly from a Collection, we need to load it's dictionary first.
                if (propertyName == "_Values")
                {
                    var baseType = TypeManager.GetOrCreateCustomType(Reflection.BaseType(obj.GetType()));
                    if (baseType.isDictionary)
                    {
                        var dict = (IDictionary) obj;
                        var loaded = (IDictionary) baseType.Read<IDictionary>(reader);
                        foreach (DictionaryEntry kvp in loaded)
                            dict[kvp.Key] = kvp.Value;
                    }
                    else if (baseType.isCollection)
                    {
                        var loaded = (IEnumerable) baseType.Read<IEnumerable>(reader);

                        var t = baseType.GetType();

                        if (t == typeof(CustomListType))
                            foreach (var item in loaded)
                                ((IList) obj).Add(item);
                        else if (t == typeof(CustomQueueType))
                        {
                            var method = baseType.type.GetMethod("Enqueue");
                            foreach (var item in loaded)
                                method.Invoke(obj, new object[] {item});
                        }
                        else if (t == typeof(CustomStackType))
                        {
                            var method = baseType.type.GetMethod("Push");
                            foreach (var item in loaded)
                                method.Invoke(obj, new object[] {item});
                        }
                        else if (t == typeof(CustomHashSetType))
                        {
                            var method = baseType.type.GetMethod("Add");
                            foreach (var item in loaded)
                                method.Invoke(obj, new object[] {item});
                        }
                    }
                }

                if (property == null) reader.Skip();
                else
                {
                    var t = TypeManager.GetOrCreateCustomType(property.type);

                    if (Reflection.IsAssignableFrom(typeof(DictionaryType), t.GetType()))
                        property.reflectedMember.SetValue(obj, ((DictionaryType) t).Read(reader));
                    else if (Reflection.IsAssignableFrom(typeof(CustomCollectionType), t.GetType()))
                        property.reflectedMember.SetValue(obj, ((CustomCollectionType) t).Read(reader));
                    else
                    {
                        object readObj = reader.Read<object>(t);
                        property.reflectedMember.SetValue(obj, readObj);
                    }
                }
            }

            return obj;
        }

        protected void GetMembers(bool safe) { GetMembers(safe, null); }

        protected void GetMembers(bool safe, string[] memberNames)
        {
            var serializedMembers = Reflection.GetSerializableMembers(type, safe, memberNames);
            members = new Member[serializedMembers.Length];
            for (int i = 0; i < serializedMembers.Length; i++)
                members[i] = new Member(serializedMembers[i]);
        }

        #endregion
    }

    
}