using System;
using Pancake.BakingSheet.Internal;


namespace Pancake.BakingSheet
{
    public interface ISheetReference
    {
        void Map(SheetConvertingContext context, ISheet sheet);

        object Id { get; set; }
        Type IdType { get; }

        ISheetRow Ref { get; }
        bool IsValid();
    }

    public partial class Sheet<TKey, TValue>
    {
        /// <summary>
        /// Cross-sheet reference column to this Sheet.
        /// </summary>
        public partial struct Reference : ISheetReference
        {
            [Preserve] public TKey Id { get; private set; }

            [Preserve] private TValue reference;

            [Preserve] public TValue Ref
            {
                get
                {
                    EnsureLoadReference();
                    return reference;
                }
                private set => reference = value;
            }

            object ISheetReference.Id { get => Id; set => Id = (TKey) value; }

            public Type IdType => typeof(TKey);

            ISheetRow ISheetReference.Ref => Ref;

            public Reference(TKey id)
                : this()
            {
                Id = id;
            }

            void ISheetReference.Map(SheetConvertingContext context, ISheet sheet)
            {
                EnsureLoadReference();

                if (sheet is ISheet<TKey, TValue> referSheet)
                {
                    if (Ref == null)
                    {
                        Ref = referSheet[Id];
                    }
                    else if (Ref != referSheet[Id])
                    {
                        UnityEngine.Debug.LogError($"Found different reference than originally set for \"{Id}\"");
                    }
                }

                if (Id != null && Ref == null)
                {
                    UnityEngine.Debug.LogError($"Failed to find reference \"{Id}\" on {sheet.Name}");
                }
            }

            public static implicit operator TKey(Reference origin) { return origin.Id; }

            public override bool Equals(object obj)
            {
                if (!(obj is Reference other))
                    return false;

                if (Id == null)
                    return other.Id == null;

                return Id.Equals(other.Id);
            }

            public override int GetHashCode() { return Id == null ? 0 : Id.GetHashCode(); }

            public override string ToString() { return Id == null ? "(null)" : Id.ToString(); }

            public bool IsValid() { return Ref != null; }

            /// <summary>
            /// Each Engine counterpart implements this
            /// </summary>
            partial void EnsureLoadReference();
        }
    }
}