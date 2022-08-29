using System;
using System.Collections;
using System.Collections.Generic;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomHashSetType : CustomCollectionType
	{
		public CustomHashSetType(Type type) : base(type){}

        public override void Write(object obj, Writer writer)
        {
            if (obj == null) { writer.WriteNull(); return; };

            var list = (IEnumerable)obj;

            if (elementType == null)
                throw new ArgumentNullException("CustomType argument cannot be null.");

            int count = 0;
            foreach (var item in list)
                count++;

            //writer.StartWriteCollection(count);

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
            var val = Read(reader);
            if (val == null)
                return default(T);
            return (T)val;
        }


        public override object Read(Reader reader)
		{
            /*var method = typeof(CustomCollectionType).GetMethod("ReadICollection", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(elementType.type);
            if(!(bool)method.Invoke(this, new object[] { reader, list, elementType }))
                return null;*/

            var genericParam = Reflection.GetGenericArguments(type)[0];
            var listType = Reflection.MakeGenericType(typeof(List<>), genericParam);
            var list = (IList)Reflection.CreateInstance(listType);

            if (!reader.StartReadCollection())
            {
                // Iterate through each character until we reach the end of the array.
                while (true)
                {
                    if (!reader.StartReadCollectionItem())
                        break;
                    list.Add(reader.Read<object>(elementType));

                    if (reader.EndReadCollectionItem())
                        break;
                }

                reader.EndReadCollection();
            }

            return Reflection.CreateInstance(type, list);
        }

        public override void ReadInto<T>(Reader reader, object obj)
        {
            ReadInto(reader, obj);
        }

        public override void ReadInto(Reader reader, object obj)
		{
            throw new NotImplementedException("Cannot use LoadInto/ReadInto with HashSet because HashSets do not maintain the order of elements");
		}
    }
}