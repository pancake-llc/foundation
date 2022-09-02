using System;
using System.ComponentModel;

namespace Pancake.Init
{
    /// <summary>
    /// Implementation of <see cref="ITypeDescriptorContext"/> which <see cref="Any{}"/> can
    /// pass to the <see cref="TypeConverter.CanConvertTo(ITypeDescriptorContext, Type)"/> method
    /// to convert its <see cref="Any{}.Value"/> from <see cref="UnityEngine.Object"/> type to its generic type.
    /// any
    /// </summary>
    internal class AnyTypeDescriptorContext : ITypeDescriptorContext
	{
		public object Instance { get; set; }
		public IContainer Container => null;		
		public PropertyDescriptor PropertyDescriptor => null;
		public AnyTypeDescriptorContext(Type valueType) => Instance = valueType;
		public object GetService(Type serviceType) => ServiceUtility.GetService(serviceType);
		public void OnComponentChanged() { }
		public bool OnComponentChanging() => false;
	}
}