using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FearIndigo.Audio
{
    [CreateAssetMenu(menuName = "Audio Events/Simple Audio Event", fileName = "New Simple Audio Event")]
    public class SimpleAudioEvent : AudioEvent
    {
        public bool loop;
        [Range(0, 1)]
        public float spatialBlend = 0.5f;
        public float dopplerLevel = 0f;
        public float2 minMaxDistance = new float2(10f, 150f);
        public float2 minMaxVolume = new float2(1f, 1f);
        public float2 minMaxPitch = new float2(1f, 1f);
        public List<AudioClip> clips;

        public override void Play(Vector3 position)
        {
            var source = AudioSourcePool.Get();
            Play(source, position);
            if(!loop)
                AudioSourcePool.Release(source, source.clip.length / source.pitch);
        }
        
        public override void Play(AudioSource source, Vector3 position)
        {
            source.transform.position = position;

            source.loop = loop;
            source.spatialBlend = spatialBlend;
            source.dopplerLevel = dopplerLevel;
            source.minDistance = minMaxDistance.x;
            source.maxDistance = minMaxDistance.y;
            source.clip = clips[Random.Range(0, clips.Count)];
            source.volume = Random.Range(minMaxVolume.x, minMaxVolume.y);
            source.pitch = Random.Range(minMaxPitch.x, minMaxPitch.y);
            
            source.Play();
        }
    }
}
