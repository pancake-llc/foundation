using System;
using System.Collections;
using System.Collections.Generic;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public abstract class CustomCollectionType : CustomType
	{
		public CustomType elementType;

		/*protected Reflection.ReflectedMethod readMethod = null;
		protected Reflection.ReflectedMethod readIntoMethod = null;*/

        public abstract object Read(Reader reader);
        public abstract void ReadInto(Reader reader, object obj);

        public CustomCollectionType(Type type) : base(type)
		{
			elementType = TypeManager.GetOrCreateCustomType(Reflection.GetElementTypes(type)[0], false);
			isCollection = true;

			// If the element type is null (i.e. unsupported), make this CustomType null.
			if(elementType == null)
				isUnsupported = true;
		}

        public CustomCollectionType(Type type, CustomType elementType) : base(type)
		{
			this.elementType = elementType;
			isCollection = true;
		}

        protected virtual bool ReadICollection<T>(Reader reader, ICollection<T> collection, CustomType elementType)
		{
			if(reader.StartReadCollection())
				return false;

			// Iterate through each character until we reach the end of the array.
			while(true)
			{
				if(!reader.StartReadCollectionItem())
					break;
				collection.Add(reader.Read<T>(elementType));

				if(reader.EndReadCollectionItem())
					break;
			}

			reader.EndReadCollection();

			return true;
		}

        protected virtual void ReadICollectionInto<T>(Reader reader, ICollection<T> collection, CustomType elementType)
        {
            ReadICollectionInto(reader, collection, elementType);
        }

        [UnityEngine.Scripting.Preserve]
        protected virtual void ReadICollectionInto(Reader reader, ICollection collection, CustomType elementType)
		{
			if(reader.StartReadCollection())
				throw new NullReferenceException("The Collection we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

			int itemsLoaded = 0;

			// Iterate through each item in the collection and try to load it.
			foreach(var item in collection)
			{
				itemsLoaded++;

				if(!reader.StartReadCollectionItem())
					break;

				reader.ReadInto<object>(item, elementType);

				// If we find a ']', we reached the end of the array.
				if(reader.EndReadCollectionItem())
					break;

				// If there's still items to load, but we've reached the end of the collection we're loading into, throw an error.
				if(itemsLoaded == collection.Count)
					throw new IndexOutOfRangeException("The collection we are loading is longer than the collection provided as a parameter.");
			}

			// If we loaded fewer items than the parameter collection, throw index out of range exception.
			if(itemsLoaded != collection.Count)
				throw new IndexOutOfRangeException("The collection we are loading is shorter than the collection provided as a parameter.");

			reader.EndReadCollection();
		}
	}
}