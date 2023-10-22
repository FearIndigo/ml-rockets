using UnityEngine;

namespace FearIndigo.Singleton
{
    public abstract class SingletonSO<T> : ScriptableObject where T : SingletonSO<T>
    {
        protected bool initialized { get; private set; }

        private static T _inst;
        public static T instance
        {
            get
            {
                if (_inst) return _inst;
                
                var objs = Resources.FindObjectsOfTypeAll<T>();
                if (objs.Length > 0)
                {
                    _inst = objs[0];
                    if(!_inst.initialized) _inst.Initialize();

                    return _inst;
                }
                
                _inst = CreateInstance<T>();
                _inst.Initialize();
                
                return _inst;
            }
        }
        public static bool hasInstance => _inst;
        
        public static void DestroyInstance()
        {
            if(_inst == null) return;
            
            Destroy(_inst);

            _inst = null;
        }
        
        protected virtual void Awake()
        {
            if (initialized) return;
            
            Initialize();
        }

        protected virtual void OnDestroy()
        {
            if(_inst != this) return;
            _inst = null;
        }

        protected virtual void Initialize()
        {
            var thisInstance = this as T;
            if (_inst == null) _inst = thisInstance;
            else if (_inst != thisInstance)
            {
                Debug.LogWarning($"SingletonSO already exits of type {typeof(T)}. Destroying.");
                Destroy(this);
                return;
            }
            
            initialized = true;
        }
    }
}