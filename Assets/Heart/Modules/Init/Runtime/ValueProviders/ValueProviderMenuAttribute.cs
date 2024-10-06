using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static Sisus.Init.FlagsValues;

namespace Sisus.Init
{
	/// <summary>
	/// The <see cref="ValueProviderMenuAttribute"/> allows you to add menu items to the dropdown menu
	/// for <see cref="IInitializable{}.Init"/> arguments of an <see cref="Initializer{,}"/>.
	/// <para>
	/// When the attribute is added to a <see cref="ScriptableObject"/>-derived class that implements
	/// either <see cref="IValueByTypeProvider"/> or <see cref="IValueProvider{}"/>, then the
	/// init argument will be retrieved at runtime using the <see cref="IValueByTypeProvider.TryGetFor"/> method
	/// or the <see cref="IValueProvider.Value"/> property and used to initialize the client object.
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class ValueProviderMenuAttribute : Attribute
	{
		private const float DefaultOrder = 100f;

		/// <summary>
		/// The text on the menu item that appears in the dropdown menu for the targeted Init parameters.
		/// </summary>
		public string ItemName { get; set; }

		/// <summary>
		/// Tooltip shown when the value provider tag is mouseovered in the Inspector.
		/// </summary>
		public string Tooltip { get; set; }

		/// <summary>
		/// Specifies zero or more type constraints, at least one of which a parameter type must fulfill
		/// for this menu item to be included in its dropdown menu.
		/// >para>
		/// </summary>
		public Is Where { get => WhereAny; set => WhereAny = value; }

		/// <summary>
		/// Specifies zero or more type constraints, at least one of which a parameter type must fulfill
		/// for this menu item to be included in its dropdown menu.
		/// </summary>
		public Is WhereAny { get; set; }

		/// <summary>
		/// Specifies zero or more type constraints, none of which a parameter type can match,
		/// for this menu item to be included in its dropdown menu.
		/// </summary>
		public Is WhereNone { get; set; }

		/// <summary>
		/// Specifies zero or more type constraints, all of which a parameter type must fulfill
		/// for this menu item to be included in its dropdown menu.
		/// </summary>
		public Is WhereAll { get; set; }

		/// <summary>
		/// Specifies zero or more parameter types that the value provider with this attribute targets.
		/// <para>
		/// The menu item will appear in the dropdown menu of all Init parameters whose type matches any of the specified types.
		/// </para>
		/// <para>
		/// The menu item will also appear in the dropdown menu of all Init parameters whose type derives from any of the specified types.
		/// </para>
		/// <para>
		/// The menu item will also appear in the dropdown menu of all Init parameters whose type implements any of the specified types, when they are interfaces.
		/// </para>
		/// </summary>
		public Type[] IsAny { get; set; }

		/// <summary>
		/// Specifies zero or more parameter types that the value provider with this attribute does not target.
		/// <para>
		/// The menu item will not appear in the dropdown menu of any Init parameters whose type matches any of the specified types.
		/// </para>
		/// <para>
		/// The menu item will also not appear in the dropdown menu of any Init parameters whose type derives from any of the specified types.
		/// </para>
		/// <para>
		/// The menu item will also not be added in the dropdown menu of any Init parameters whose type implements any of the specified types, when they are interfaces.
		/// </para>
		/// </summary>
		public Type[] NotAny { get; set; }

		/// <summary>
		/// Specifies a parameter type that the value provider with this attribute targets.
		/// <para>
		/// The menu item will appear in the dropdown menu of all Init parameters whose type matches the specified type.
		/// </para>
		/// <para>
		/// The menu item will also appear in the dropdown menu of all Init parameters whose type derives from the specified type.
		/// </para>
		/// <para>
		/// The menu item will also appear in the dropdown menu of all Init parameters whose type implements
		/// the specified type, when is it an interface.
		/// </para>
		/// </summary>
		public Type Is
		{
			get => IsAny.Length == 0 ? null : IsAny[0];
			set => IsAny = value == null ? Array.Empty<Type>() : new[]{ value };
		}

		/// <summary>
		/// Specifies type of Init parameter that the value provider with this attribute does not target.
		/// <para>
		/// The menu item will not appear in the dropdown menu of any Init parameters whose type matches the specified type.
		/// </para>
		/// <para>
		/// The menu item will not appear added in the dropdown menu of any Init parameters whose type derives from the specified type.
		/// </para>
		/// <para>
		/// The menu item will also not appear in the dropdown menu of any Init parameters whose type implements
		/// the specified type, when is it an interface.
		/// </para>
		/// </summary>
		public Type Not
		{
			get => NotAny.Length == 0 ? null : NotAny[0];
			set => NotAny = value == null ? Array.Empty<Type>() : new[]{ value };
		}

		/// <summary>
		/// The position of this item in the dropdown menu, relative to other items.
		/// <para>
		/// E.g. if ItemName is "Get Component/In Children", then an Order value of 5.3 would give the "Get Component"
		/// item the order value of 5 in the root, and the "In Children" item the order number of 3 in the "Get Component" group.
		/// </para>
		/// <para>
		/// Items with a lower <see cref="Order"/> value are shown higher up, before items with a lower <see cref="Order"/> value.
		/// </para>
		/// </summary>
		public float Order { get; set; }

		public ValueProviderMenuAttribute(string itemName, Is whereAny, params Type[] isAny)
		{
			ItemName = itemName ?? "";
			Tooltip = "";
			WhereAny = whereAny;
			WhereAll = Init.Is.Unconstrained;
			WhereNone = Init.Is.Unconstrained;
			IsAny = isAny ?? Type.EmptyTypes;
			NotAny = Type.EmptyTypes;
			Order = DefaultOrder;
		}

		public ValueProviderMenuAttribute(string itemName, params Type[] isAny) : this(itemName, Init.Is.Unconstrained, isAny) { }
		public ValueProviderMenuAttribute(Is whereAny, params Type[] isAny) : this("", whereAny, isAny) { }
		public ValueProviderMenuAttribute(params Type[] isAny) : this("", Init.Is.Unconstrained, isAny) { }
	}

	/// <summary>
	/// Specifies constraint options for types of the types of Init parameters that a value provider with
	/// the <see cref="ValueProviderMenuAttribute"/> targets.
	/// <para>
	/// The menu item to assign the value provider will only be shown in the dropdown menus of Init parameters matching the constraints.
	/// </para>
	/// </summary>
	[Flags]
	public enum Is
	{
		/// <summary>
		/// No constraints applied to the types of Init parameters targeted.
		/// </summary>
		Unconstrained = 0,
		
		/// <summary>
		/// Shown in dropdown menus of reference type Init parameters.
		/// </summary>
		Class = _1,

		/// <summary>
		/// Shown in dropdown menus of struct type Init parameters.
		/// </summary>
		ValueType = _2,

		/// <summary>
		/// Shown in dropdown menus of concrete type Init parameters.
		/// </summary>
		Concrete = _3,

		/// <summary>
		/// Shown in dropdown menus of abstract type Init parameters.
		/// </summary>
		Abstract = _4,

		/// <summary>
		/// Shown in dropdown menus of interface type parameters.
		/// </summary>
		Interface = _5,

		/// <summary>
		/// Shown in dropdown menus of Init parameters of built-in C# type,
		/// such as <see cref="bool"/>, <see cref="int"/>, <see cref="float"/> and <see cref="string"/>.
		/// </summary>
		BuiltIn = _6,

		/// <summary>
		/// Shown in dropdown menus of component type Init parameters.
		/// <para>
		/// This includes both <see cref="Component"/>-derived types, as well as any interface types
		/// which are implemented by at least one <see cref="Component"/>-derived type.
		/// </para>
		/// </summary>
		Component = _7,

		/// <summary>
		/// Shown in dropdown menus of Init parameters of plain old C# types that have a <see cref="Wrapper{}"/> component,
		/// making it possible to attach them to <see cref="GameObject"/>s.
		/// </summary>
		WrappedObject = _8,

		/// <summary>
		/// Shown in dropdown menus of Init parameters of types that can exist as part of a scene.
		/// <para>
		/// This includes all <see cref="Component"/>-derived types,
		/// all plain old C# types that have a <see cref="Wrapper{}"/> class,
		/// all interface types that are implemented by at least one <see cref="Component"/>-derived type,
		/// and the <see cref="GameObject"/> type.
		/// </para>
		/// </summary>
		SceneObject = _9,

		/// <summary>
		/// Shown in dropdown menus of Init parameters of types that can exist as a standalone asset, or as part of a prefab.
		/// <para>
		/// This includes all <see cref="Object"/>-derived types,
		/// all plain old C# types that have a <see cref="Wrapper{}"/> class,
		/// all interface types that are implemented by at least one <see cref="Object"/>-derived type,
		/// and the <see cref="GameObject"/> type.
		/// </para>
		/// </summary>
		Asset = _10,

		/// <summary>
		/// Shown in dropdown menus of Init parameters that implement <see cref="IEnumerable"/>,
		/// such as <see cref="List{T}"/> and <see cref="Array"/>.
		/// </summary>
		Collection = _11,

		/// <summary>
		/// Shown in dropdown menus of Init parameters whose type is the defining type of a Service
		/// (registered using the ServiceAttribute).
		/// </summary>
		Service = _12,
	}
}