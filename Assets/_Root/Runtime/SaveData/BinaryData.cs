using System;

// ReSharper disable FieldCanBeMadeReadOnly.Global
namespace Pancake.SaveData
{
    public struct BinaryData
    {
        public CustomType type;
        public byte[] bytes;

        public BinaryData(Type type, byte[] bytes)
        {
            this.type = type == null ? null : TypeManager.GetOrCreateCustomType(type);
            this.bytes = bytes;
        }

        public BinaryData(CustomType type, byte[] bytes)
        {
            this.type = type;
            this.bytes = bytes;
        }
    }
}