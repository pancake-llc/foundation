using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class DictionaryType : CustomType
	{
		public CustomType keyType;
		public CustomType valueType;

		protected Reflection.ReflectedMethod readMethod = null;
		protected Reflection.ReflectedMethod readIntoMethod = null;

		public DictionaryType(Type type) : base(type)
		{
			var types = Reflection.GetElementTypes(type);
			keyType = TypeManager.GetOrCreateCustomType(types[0], false);
			valueType = TypeManager.GetOrCreateCustomType(types[1], false);

			// If either the key or value type is unsupported, make this type NULL.
			if(keyType == null || valueType == null)
				isUnsupported = true;;

			isDictionary = true;
		}

        public DictionaryType(Type type, CustomType keyType, CustomType valueType) : base(type)
        {
            this.keyType = keyType;
            this.valueType = valueType;

            // If either the key or value type is unsupported, make this type NULL.
            if (keyType == null || valueType == null)
                isUnsupported = true; ;

            isDictionary = true;
        }

        public override void Write(object obj, Writer writer)
		{
			var dict = (IDictionary)obj;

			//writer.StartWriteDictionary(dict.Count);

			int i=0;
			foreach(System.Collections.DictionaryEntry kvp in dict)
			{
				writer.StartWriteDictionaryKey(i);
				writer.Write(kvp.Key, keyType);
				writer.EndWriteDictionaryKey(i);
				writer.StartWriteDictionaryValue(i);
				writer.Write(kvp.Value, valueType);
				writer.EndWriteDictionaryValue(i);
				i++;
			}

			//writer.EndWriteDictionary();
		}

		public override object Read<T>(Reader reader)
		{
			return Read(reader);
		}

		public override void ReadInto<T>(Reader reader, object obj)
		{
            ReadInto(reader, obj);
		}

		/*
		 * 	Allows us to call the generic Read method using Reflection so we can define the generic parameter at runtime.
		 * 	It also caches the method to improve performance in later calls.
		 */
		public object Read(Reader reader)
		{
			if(reader.StartReadDictionary())
				return null;

			var dict = (IDictionary)Reflection.CreateInstance(type);

			// Iterate through each character until we reach the end of the array.
			while(true)
			{
				if(!reader.StartReadDictionaryKey())
					return dict;
				var key = reader.Read<object>(keyType);
				reader.EndReadDictionaryKey();

				reader.StartReadDictionaryValue();
				var value = reader.Read<object>(valueType);

				dict.Add(key,value);

				if(reader.EndReadDictionaryValue())
					break;
			}

			reader.EndReadDictionary();

			return dict;
		}

		public void ReadInto(Reader reader, object obj)
		{
			if(reader.StartReadDictionary())
				throw new NullReferenceException("The Dictionary we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

			var dict = (IDictionary)obj;

			// Iterate through each character until we reach the end of the array.
			while(true)
			{
				if(!reader.StartReadDictionaryKey())
					return;
				var key = reader.Read<object>(keyType);

				if(!dict.Contains(key))
					throw new KeyNotFoundException("The key \"" + key + "\" in the Dictionary we are loading does not exist in the Dictionary we are loading into");
				var value = dict[key];
				reader.EndReadDictionaryKey();

				reader.StartReadDictionaryValue();

				reader.ReadInto<object>(value, valueType);

				if(reader.EndReadDictionaryValue())
					break;
			}

			reader.EndReadDictionary();
		}
	}
}