﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Pancake.BakingSheet.Internal;
using UnityEngine;


namespace Pancake.BakingSheet
{
    /// <summary>
    /// Represents a single page of Sheet.
    /// </summary>
    /// <typeparam name="TKey">Type of id column.</typeparam>
    /// <typeparam name="TValue">Type of Row.</typeparam>
    public abstract partial class Sheet<TKey, TValue> : KeyedCollection<TKey, TValue>, ISheet<TKey, TValue> where TValue : SheetRow<TKey>, new()
    {
        [Preserve] public string Name { get; set; }
        [Preserve] public string HashCode { get; set; }

        private PropertyMap _propertyMap;

        public Type RowType => typeof(TValue);

        public new TValue this[TKey id]
        {
            get
            {
                if (id == null || !Contains(id))
                    return default;
                return base[id];
            }
        }

        public ICollection<TKey> Keys => Dictionary.Keys;
        public TValue Find(TKey id) => this[id];

        bool ISheet.Contains(object key) => Contains((TKey) key);
        void ISheet.Add(object value) => Add((TValue) value);

        public new Enumerator GetEnumerator() => new(this);
        IEnumerator<ISheetRow> ISheet.GetEnumerator() => GetEnumerator();

        protected override TKey GetKeyForItem(TValue item) { return item.Id; }

        private PropertyMap GetPropertyMap(SheetConvertingContext context)
        {
            if (_propertyMap != null)
                return _propertyMap;

            _propertyMap = new PropertyMap(context, GetType());
            return _propertyMap;
        }

        PropertyMap ISheet.GetPropertyMap(SheetConvertingContext context) => GetPropertyMap(context);

        void ISheet.MapReferences(SheetConvertingContext context, Dictionary<Type, ISheet> rowTypeToSheet)
        {
            var propertyMap = GetPropertyMap(context);

            propertyMap.UpdateIndex(this);

            foreach (var (node, indexes) in propertyMap.TraverseLeaf())
            {
                if (!typeof(ISheetReference).IsAssignableFrom(node.ValueType))
                    continue;

                var referenceRowType = node.ValueType.GenericTypeArguments[1];

                if (!rowTypeToSheet.TryGetValue(referenceRowType, out var sheet))
                {
                    Debug.LogError($"Failed to find sheet for {referenceRowType} reference");
                    continue;
                }

                foreach (var row in Items)
                {
                    int verticalCount = node.GetVerticalCount(row, indexes.GetEnumerator());

                    for (int vindex = 0; vindex < verticalCount; ++vindex)
                    {
                        // only proceed when path is valid
                        if (!node.TryGetValue(row, vindex, indexes.GetEnumerator(), out var obj))
                            continue;

                        if (obj is ISheetReference refer)
                        {
                            refer.Map(context, sheet);
                            node.SetValue(row, vindex, indexes.GetEnumerator(), obj);
                        }
                    }
                }
            }
        }

        public virtual void PostLoad(SheetConvertingContext context)
        {
            int index = -1;

            foreach (var row in Items)
            {
                row.Index = ++index;
                row.PostLoad(context);
            }
        }

        public virtual void VerifyAssets(SheetConvertingContext context)
        {
            var propertyMap = GetPropertyMap(context);

            propertyMap.UpdateIndex(this);

            foreach (var (node, indexes) in propertyMap.TraverseLeaf())
            {
                foreach (var verifier in context.Verifiers)
                {
                    if (!verifier.CanVerify(node.PropertyInfo, node.ValueType))
                        continue;

                    foreach (var row in Items)
                    {
                        int verticalCount = node.GetVerticalCount(row, indexes.GetEnumerator());

                        for (int vindex = 0; vindex < verticalCount; ++vindex)
                        {
                            var obj = node.GetValue(row, vindex, indexes.GetEnumerator());
                            var err = verifier.Verify(node.PropertyInfo, obj);

                            if (err != null) Debug.LogError($"Verification: {err}");
                        }
                    }
                }
            }

            foreach (var row in Items)
            {
                row.VerifyAssets(context);
            }
        }

        /// <summary>
        /// Struct enumerator for Sheet.
        /// </summary>
        public struct Enumerator : IEnumerator<TValue>
        {
            private readonly Sheet<TKey, TValue> _sheet;
            private int _index;

            public Enumerator(Sheet<TKey, TValue> sheet)
            {
                _sheet = sheet;
                _index = -1;
            }

            public bool MoveNext() => ++_index < _sheet.Count;
            public TValue Current => _sheet[_index];

            object IEnumerator.Current => Current;
            void IEnumerator.Reset() => _index = -1;

            public void Dispose() { }
        }
    }

    /// <summary>
    /// Represents a single page of Sheet, with string id.
    /// For other type of id, use generic version.
    /// </summary>
    /// <typeparam name="T">Type of Row.</typeparam>
    public abstract class Sheet<T> : Sheet<string, T>, ISheet<T> where T : SheetRow<string>, new()
    {
    }
}