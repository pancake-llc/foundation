using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.UIQuery
{
    public class UICache : MonoBehaviour, IMapper
    {
        [SerializeField]
        public CachedObject[] Elements;

        private IMapper cachedMapper;

        public void CreateCache()
        {
            var elements = new List<CachedObject>();
            CreateCacheInternal(elements, transform, new List<uint>());
            Elements = elements.ToArray();
            cachedMapper = null;
        }

        private void CreateCacheInternal(List<CachedObject> elements, Transform t, List<uint> basePath)
        {
            elements.Add(new CachedObject { GameObject = t.gameObject, Path = basePath.ToArray() });
            foreach (Transform child in t)
            {
                basePath.Insert(0, FastHash.CalculateHash(child.name));
                CreateCacheInternal(elements, child, basePath);
                basePath.RemoveAt(0);
            }
        }

        public GameObject Get()
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            return cachedMapper.Get();
        }

        public T Get<T>() where T : Component
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            return cachedMapper.Get<T>();
        }

        public GameObject Get(string objectPath)
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            return cachedMapper.Get(objectPath);
        }

        public GameObject Get(uint[] objectPath)
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            return cachedMapper.Get(objectPath);
        }

        public GameObject[] GetAll(string objectPath)
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            return cachedMapper.GetAll(objectPath);
        }

        public GameObject[] GetAll(uint[] objectPath)
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            return cachedMapper.GetAll(objectPath);
        }

        public T Get<T>(string objectPath) where T : Component
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            return cachedMapper.Get<T>(objectPath);
        }

        public T Get<T>(uint[] objectPath) where T : Component
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            return cachedMapper.Get<T>(objectPath);
        }

        public IMapper GetMapper(string rootObjectPath)
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            return cachedMapper.GetMapper(rootObjectPath);
        }

        public IMapper GetMapper(uint[] rootObjectPath)
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            return cachedMapper.GetMapper(rootObjectPath);
        }

        public T GetChild<T>(string rootObjectPath) where T : IMappedObject, new()
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            return cachedMapper.GetChild<T>(rootObjectPath);
        }

        public T GetChild<T>(uint[] rootObjectPath) where T : IMappedObject, new()
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            return cachedMapper.GetChild<T>(rootObjectPath);
        }

        public CachedObject[] GetRawElements()
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            return cachedMapper.GetRawElements();
        }

        public void Copy(IMapper other)
        {
            if (cachedMapper == null) cachedMapper = new Mapper(gameObject, Elements);
            cachedMapper.Copy(other);
            Elements = cachedMapper.GetRawElements();
        }
    }

    [Serializable]
    public class CachedObject
    {
        public GameObject GameObject;
        public uint[] Path;
    }
}
