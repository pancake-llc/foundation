using System;

// ReSharper disable FieldCanBeMadeReadOnly.Global
namespace Pancake.SaveData
{
	public class Member
	{
		public string name;
		public Type type;
		public bool isProperty;
		public Reflection.ReflectedMember reflectedMember;
		public bool useReflection;

		public Member(string name, Type type, bool isProperty)
		{
			this.name = name;
			this.type = type;
			this.isProperty = isProperty;
	 	}

		public Member(Reflection.ReflectedMember reflectedMember)
		{
			this.reflectedMember = reflectedMember;
			this.name = reflectedMember.Name;
			this.type = reflectedMember.MemberType;
			this.isProperty = reflectedMember.isProperty;
			this.useReflection = true;
		}
	}
}
