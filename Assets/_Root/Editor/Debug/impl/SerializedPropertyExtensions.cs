using System;
using UnityEditor;
using System.Reflection;
using System.Collections;

namespace Pancake.Debugging
{
	public static class SerializedPropertyExtensions
	{
		public static void GetMemberInfoAndOwner(this SerializedProperty prop, out MemberInfo memberInfo, out object owner)
		{
			var path = prop.propertyPath.Replace(".Array.data[", "[");
			owner = prop.serializedObject.targetObject;
			var pathParts = path.Split('.');
			memberInfo = null;
			
			int last = pathParts.Length - 1;
			int previousToLast = last - 1;
			for(int n = 0; n <= last; n++)
			{
				var pathPart = pathParts[n];
				int arrayElementStartIndex = pathPart.IndexOf('[');
				bool isCollectionMember = arrayElementStartIndex != -1;
				string name = isCollectionMember ? pathPart.Substring(0, arrayElementStartIndex) : pathPart;
				var type = owner.GetType();
				var fieldInfo = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if(fieldInfo != null)
				{
					memberInfo = fieldInfo;

					//if it's the last part of the path, we are done!
					if(n == last)
					{
						return;
					}

					//otherwise continue on to the next part
					//with the owner updated to the value of this part
					owner = fieldInfo.GetValue(owner);
				}
				else
				{
					var propertyInfo = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
					if(propertyInfo == null)
					{
						//Unity seems to often add "m_" prefix to its properties when serialized. So property "avatar" becomes "m_Avatar".
						if(name.StartsWith("m_"))
						{
							propertyInfo = type.GetProperty(name.Substring(2), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
						}

						if(propertyInfo == null)
						{
							#if DEV_MODE
							Debug.LogWarning("GetMemberInfoAndOwner(\"" + prop.propertyPath + "\") - pathPart #" + (n + 1) + "/" + (previousToLast + 2) + " \"" + pathPart + "\" could not get FieldInfo nor PropertyInfo for owner of type " + type.Name + "...");
							#endif

							memberInfo = null;
							return;
						}
					}

					memberInfo = propertyInfo;

					//if it's the last part of the path, we are done!
					if(n == last)
					{
						return;
					}

					//otherwise continue on to the next part
					//with the owner updated to the value of this part
					owner = propertyInfo.GetValue(owner, null);
				}

				//if it's an array member, we still need to get the correct Instance (unless it's the last index, then we don't need it)
				if(isCollectionMember && n < previousToLast)
				{
					var index = Convert.ToInt32(pathPart.Substring(pathPart.IndexOf('[')).Replace("[","").Replace("]",""));
					owner = GetCollectionMemberValue(owner, name, index);
				}
			}
		}

		private static object GetCollectionMemberValue(object source, string name, int index)
		{
			var collection = GetFieldValue(source, name) as IList;
			return collection[index];
		}

		private static object GetFieldValue(object source, string name)
		{
			if(source == null)
			{
				return null;
			}
			
			var type = source.GetType();
			
			var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			
			if(f == null)
			{
				var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if(p == null)
				{
					return null;
				}
				
				return p.GetValue(source, null);
			}
			return f.GetValue(source);
		}
	}
}