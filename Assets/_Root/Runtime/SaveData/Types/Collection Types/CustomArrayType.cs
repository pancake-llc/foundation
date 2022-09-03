using System;
using System.Collections;
using System.Collections.Generic;

namespace Pancake.SaveData
{
    [UnityEngine.Scripting.Preserve]
    public class CustomArrayType : CustomCollectionType
    {
        public CustomArrayType(Type type)
            : base(type)
        {
        }

        public CustomArrayType(Type type, CustomType elementType)
            : base(type, elementType)
        {
        }

        public override void Write(object obj, Writer writer)
        {
            var array = (System.Array) obj;

            if (elementType == null)
                throw new ArgumentNullException(nameof(obj));

            //writer.StartWriteCollection();

            for (int i = 0; i < array.Length; i++)
            {
                writer.StartWriteCollectionItem(i);
                writer.Write(array.GetValue(i), elementType);
                writer.EndWriteCollectionItem(i);
            }

            //writer.EndWriteCollection();
        }

        public override object Read(Reader reader)
        {
            var list = new List<object>();
            if (!ReadICollection(reader, list, elementType))
                return null;

            var array = Reflection.ArrayCreateInstance(elementType.type, list.Count);
            int i = 0;
            foreach (var item in list)
            {
                array.SetValue(item, i);
                i++;
            }

            return array;

            /*var Instance = new List<object>();

			if(reader.StartReadCollection())
				return null;

			// Iterate through each character until we reach the end of the array.
			while(true)
			{
				if(!reader.StartReadCollectionItem())
					break;
				Instance.Add(reader.Read<object>(elementType));

				if(reader.EndReadCollectionItem())
					break;
			}

			reader.EndReadCollection();

			var array = Reflection.ArrayCreateInstance(elementType.type, Instance.Count);
			int i = 0;
			foreach(var item in Instance)
			{
				array.SetValue(item, i);
				i++;
			}

			return array;*/
        }

        public override object Read<T>(Reader reader)
        {
            return Read(reader);
            /*var list = new List<object>();
			if(!ReadICollection(reader, list, elementType))
				return null;

            var array = Reflection.ArrayCreateInstance(elementType.type, list.Count);
            int i = 0;
            foreach (var item in list)
            {
                array.SetValue(item, i);
                i++;
            }

            return array;*/
        }

        public override void ReadInto<T>(Reader reader, object obj) { ReadICollectionInto(reader, (ICollection) obj, elementType); }

        public override void ReadInto(Reader reader, object obj)
        {
            var collection = (IList) obj;

            if (collection.Count == 0)
                Console.LogWarning("LoadInto/ReadInto expects a collection containing instances to load data in to, but the collection is empty.");

            if (reader.StartReadCollection())
                throw new NullReferenceException("The Collection we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

            int itemsLoaded = 0;

            // Iterate through each item in the collection and try to load it.
            foreach (var item in collection)
            {
                itemsLoaded++;

                if (!reader.StartReadCollectionItem())
                    break;

                reader.ReadInto<object>(item, elementType);

                // If we find a ']', we reached the end of the array.
                if (reader.EndReadCollectionItem())
                    break;

                // If there's still items to load, but we've reached the end of the collection we're loading into, throw an error.
                if (itemsLoaded == collection.Count)
                    throw new IndexOutOfRangeException("The collection we are loading is longer than the collection provided as a parameter.");
            }

            // If we loaded fewer items than the parameter collection, throw index out of range exception.
            if (itemsLoaded != collection.Count)
                throw new IndexOutOfRangeException("The collection we are loading is shorter than the collection provided as a parameter.");

            reader.EndReadCollection();
        }
    }
}