using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using FearIndigo.Singleton;

namespace FearIndigo.Audio
{
    public class AudioSourcePool : SingletonGO<AudioSourcePool>
    {
        public int maxPoolSize = 10;
        
        public static AudioSource Get() => instance.Pool.Get();

        private IObjectPool<AudioSource> _pool;
        public IObjectPool<AudioSource> Pool
        {
            get
            {
                if (_pool == null)
                {
                    _pool = new ObjectPool<AudioSource>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 10, maxPoolSize);
                }
                return _pool;
            }
        }

        public static void Release(AudioSource element) => instance.Pool.Release(element);
        
        public static void Release(AudioSource element, float delay)
        {
            instance.StartCoroutine(ReleaseDelayed(element, delay));
        }

        public static IEnumerator ReleaseDelayed(AudioSource element, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            Release(element);
        }
        
        private AudioSource CreatePooledItem()
        {
            var go = new GameObject("Pooled AudioSource");
            go.transform.parent = transform;
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }
        
        private void OnReturnedToPool(AudioSource element)
        {
            element.Stop();
            element.gameObject.SetActive(false);
        }
        
        private void OnTakeFromPool(AudioSource element)
        {
            element.gameObject.SetActive(true);
        }
        
        private void OnDestroyPoolObject(AudioSource element)
        {
            Destroy(element.gameObject);
        }
    }
}