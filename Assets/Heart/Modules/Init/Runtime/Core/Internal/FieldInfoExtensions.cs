#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Sisus.Init.EditorOnly.Internal
{
	/// <summary>
	/// Editor-only extension methods for <seealso cref="FieldInfo"/>.
	/// </summary>
	public static class FieldInfoExtensions
	{
		public static void GetGetter<TDelegate>(this FieldInfo field, out TDelegate @delegate) where TDelegate : Delegate => @delegate = (TDelegate)GetGetter(field, typeof(TDelegate), new[] { field.DeclaringType });
		public static Delegate GetGetter(this FieldInfo field) => GetGetter(field, typeof(Func<,>).MakeGenericType(field.DeclaringType, field.FieldType), new[] { field.DeclaringType });
		public static TDelegate GetGetter<TDelegate>(this FieldInfo field) where TDelegate : Delegate => (TDelegate)GetGetter(field, typeof(TDelegate), new[] { field.DeclaringType });
		public static TDelegate GetGetter<TDelegate>(this FieldInfo field, Type[] parameterTypes) where TDelegate : Delegate => (TDelegate)GetGetter(field, typeof(TDelegate), parameterTypes);

		private static Delegate GetGetter(FieldInfo field, Type delegateType, Type[] parameterTypes)
		{
			var declaringType = field.DeclaringType;
			if (declaringType == null)
			{
				throw new InvalidOperationException("Field " + field.Name + " does not have a declaring type.");
			}

			// Create a method to hold the generated IL.
			var method = new DynamicMethod(
										   field.Name + "Get",
										   field.FieldType,
										   parameterTypes,
										   typeof(FieldInfoExtensions).Module,
										   true);

			// Emit IL to return the value of the Transaction property.
			var emitter = method.GetILGenerator();
			emitter.Emit(OpCodes.Ldarg_0);
			emitter.Emit(OpCodes.Ldfld, field);
			emitter.Emit(OpCodes.Ret);

			return method.CreateDelegate(delegateType);
		}
    }
}
#endif