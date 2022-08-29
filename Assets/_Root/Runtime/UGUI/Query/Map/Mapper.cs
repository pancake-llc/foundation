using System;
using System.Collections.Generic;
using Pancake.Linq;
using UnityEngine;

namespace Pancake.UIQuery
{
    public class Mapper : IMapper
    {
        private GameObject root;
        private CachedObject[] elements;

        public Mapper(GameObject root, CachedObject[] elements)
        {
            this.root = root;
            this.elements = elements;
        }

        public GameObject Get()
        {
            return root;
        }

        public T Get<T>() where T : Component
        {
            return root.GetComponent<T>();
        }

        public GameObject Get(string objectPath)
        {
            var hash = ToHash(objectPath);
            try
            {
                return Get(hash);
            }
            catch (UINotFoundException e)
            {
                if (e.PathHash != null && e.PathHash.SequenceEqual(hash)) throw new UINotFoundException(objectPath, e.Type);
                throw;
            }
            catch (UINotUniqueException e)
            {
                if (e.PathHash != null && e.PathHash.SequenceEqual(hash)) throw new UINotUniqueException(objectPath, e.Type);
                throw;
            }
        }

        public GameObject Get(uint[] objectPath)
        {
            var target = GetInternal(objectPath);
            if (target.Length == 0) throw new UINotFoundException(objectPath, null);
            if (target.Length > 1) throw new UINotUniqueException(objectPath, null);
            return target.Length > 0 ? target[0].GameObject : null;
        }

        public GameObject[] GetAll(string objectPath)
        {
            return GetAll(ToHash(objectPath));
        }

        public GameObject[] GetAll(uint[] objectPath)
        {
            var target = GetInternal(objectPath);
            return target.Map(x => x.GameObject);
        }

        public T Get<T>(string objectPath) where T : Component
        {
            var hash = ToHash(objectPath);
            try
            {
                return Get<T>(hash);
            }
            catch (UINotFoundException e)
            {
                if (e.PathHash != null && e.PathHash.SequenceEqual(hash)) throw new UINotFoundException(objectPath, e.Type);
                throw;
            }
            catch (UINotUniqueException e)
            {
                if (e.PathHash != null && e.PathHash.SequenceEqual(hash)) throw new UINotUniqueException(objectPath, e.Type);
                throw;
            }
        }

        public T Get<T>(uint[] objectPath) where T : Component
        {
            var target = GetInternal(objectPath);
            if (target.Length == 0) throw new UINotFoundException(objectPath, typeof(T));
            if (target.Length > 1) throw new UINotUniqueException(objectPath, typeof(T));

            var component = target[0].GameObject.GetComponent<T>();
            if (component == null) throw new UINotFoundException(objectPath, typeof(T));
            return component;
        }

        public T GetChild<T>(string rootObjectPath) where T : IMappedObject, new()
        {
            var hash = ToHash(rootObjectPath);
            try
            {
                return GetChild<T>(hash);
            }
            catch (UINotFoundException e)
            {
                if (e.PathHash != null && e.PathHash.SequenceEqual(hash)) throw new UINotFoundException(rootObjectPath, e.Type);
                throw;
            }
            catch (UINotUniqueException e)
            {
                if (e.PathHash != null && e.PathHash.SequenceEqual(hash)) throw new UINotUniqueException(rootObjectPath, e.Type);
                throw;
            }
        }

        public T GetChild<T>(uint[] rootObjectPath) where T : IMappedObject, new()
        {
            IMapper newMapper;
            try
            {
                newMapper = GetMapper(rootObjectPath);
            }
            catch (UINotFoundException e)
            {
                if (e.PathHash != null && e.PathHash.SequenceEqual(rootObjectPath)) throw new UINotFoundException(rootObjectPath, typeof(T));
                throw;
            }
            catch (UINotUniqueException e)
            {
                if (e.PathHash != null && e.PathHash.SequenceEqual(rootObjectPath)) throw new UINotUniqueException(rootObjectPath, typeof(T));
                throw;
            }

            var newObject = new T();
            newObject.Initialize(newMapper);
            return newObject;
        }

        public IMapper GetMapper(string rootObjectPath)
        {
            var hash = ToHash(rootObjectPath);
            try
            {
                return GetMapper(hash);
            }
            catch (UINotFoundException e)
            {
                if (e.PathHash != null && e.PathHash.SequenceEqual(hash)) throw new UINotFoundException(rootObjectPath, e.Type);
                throw;
            }
            catch (UINotUniqueException e)
            {
                if (e.PathHash != null && e.PathHash.SequenceEqual(hash)) throw new UINotUniqueException(rootObjectPath, e.Type);
                throw;
            }
        }

        public IMapper GetMapper(uint[] rootObjectPath)
        {
            var target = GetInternal(rootObjectPath);
            if (target.Length == 0) throw new UINotFoundException(rootObjectPath, null);
            if (target.Length > 1) throw new UINotUniqueException(rootObjectPath, null);

            var pathElements = target[0].Path.Reverse();
            var result = new List<CachedObject>();
            foreach (var e in elements)
            {
                if (e.Path.Length < pathElements.Length) continue;

                var pass = true;
                for (var i = 0; i < pathElements.Length; ++i)
                {
                    if (e.Path[e.Path.Length - i - 1] == pathElements[i]) continue;
                    pass = false;
                    break;
                }
                if (pass)
                {
                    result.Add(new CachedObject { GameObject = e.GameObject, Path = e.Path.Take(e.Path.Length - pathElements.Length) });
                }
            }
            return new Mapper(target[0].GameObject, result.ToArray());
        }

        public CachedObject[] GetRawElements()
        {
            return elements;
        }

        public void Copy(IMapper other)
        {
            elements = other.GetRawElements();
        }

        private uint[] ToHash(string stringPath)
        {
            if (string.IsNullOrEmpty(stringPath)) return new uint[] { };
            return stringPath.Split('/').Map(FastHash.CalculateHash);
        }

        private static readonly uint CachedHashDot = FastHash.CalculateHash(".");

        private CachedObject[] GetInternal(uint[] path)
        {
            if (path.Length == 0)
            {
                foreach (var e in elements)
                {
                    if (e.Path.Length == 0) return new[] { e };
                }
                return Array.Empty<CachedObject>();
            }

            var result = new List<CachedObject>();
            var start = false;
            if (path[0] == CachedHashDot)
            {
                start = true;
                path = path.Skip(1);
            }

            path = path.Reverse();
            foreach (var e in elements)
            {
                if (e.Path.Length < path.Length) continue;
                if (start && e.Path.Length != path.Length) continue;

                var pass = true;
                for (var i = 0; i < path.Length; ++i)
                {
                    if (e.Path[i] == path[i]) continue;
                    pass = false;
                    break;
                }
                if (pass)
                {
                    result.Add(e);
                }
            }
            return result.ToArray();
        }
    }
}
