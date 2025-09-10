using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.Collections
{
    [System.Serializable]
    public abstract class BatchObjectBase<T>
    {
        public IReadOnlyList<T> Objects => this.objects;
        [SerializeField] protected List<T> objects;
        public int ObjectsCount => this.objects == null ? 0 : this.objects.Count;

        public void SetObjects(List<T> objects)
        {
            this.objects = objects;
        }

        protected void IterateObjects<TValue>(TValue value, System.Action<T, TValue> obj)
        {
            int count = this.objects.Count;
            for (int i = 0; i < count; i++)
                obj.Invoke(this.objects[i], value);
        }
        protected void IterateObjects(System.Action<T> obj)
        {
            int count = this.objects.Count;
            for (int i = 0; i < count; i++)
                obj.Invoke(this.objects[i]);
        }
    }
    [System.Serializable]
    public abstract class BatchComponentBase<T> : BatchObjectBase<T> where T : Component
    {
        public void BatchGameObjectSetActive(bool value)
            => IterateObjects(value, static (T obj, bool value) => obj.gameObject.SetActive(value));
    }
    [System.Serializable]
    public abstract class BatchBehaviourBase<T> : BatchComponentBase<T> where T : Behaviour
    {
        public void BatchBehaviourSetEnabled(bool value)
            => IterateObjects(value, static (T obj, bool value) => obj.enabled = value);
    }
}