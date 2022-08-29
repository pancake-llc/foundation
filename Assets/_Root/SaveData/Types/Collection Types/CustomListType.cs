using System;
using System.Collections;


namespace Pancake.SaveData
{
    [UnityEngine.Scripting.Preserve]
    public class CustomListType : CustomCollectionType
    {
        public CustomListType(Type type)
            : base(type)
        {
        }

        public CustomListType(Type type, CustomType elementType)
            : base(type, elementType)
        {
        }

        public override void Write(object obj, Writer writer)
        {
            if (obj == null)
            {
                writer.WriteNull();
                return;
            }

            ;

            var list = (IList) obj;

            if (elementType == null) throw new ArgumentNullException(nameof(obj));

            //writer.StartWriteCollection();

            int i = 0;
            foreach (object item in list)
            {
                writer.StartWriteCollectionItem(i);
                writer.Write(item, elementType);
                writer.EndWriteCollectionItem(i);
                i++;
            }

            //writer.EndWriteCollection();
        }

        public override object Read<T>(Reader reader)
        {
            return Read(reader);

            /*var list = new List<T>();
			if(!ReadICollection<T>(reader, list, elementType))
				return null;
			return list;*/
        }

        public override void ReadInto<T>(Reader reader, object obj) { ReadICollectionInto(reader, (ICollection) obj, elementType); }

        public override object Read(Reader reader)
        {
            var instance = (IList) Reflection.CreateInstance(type);

            if (reader.StartReadCollection())
                return null;

            // Iterate through each character until we reach the end of the array.
            while (true)
            {
                if (!reader.StartReadCollectionItem())
                    break;
                instance.Add(reader.Read<object>(elementType));

                if (reader.EndReadCollectionItem())
                    break;
            }

            reader.EndReadCollection();

            return instance;
        }

        public override void ReadInto(Reader reader, object obj)
        {
            var collection = (IList) obj;

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