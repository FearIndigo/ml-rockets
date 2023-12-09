using UnityEngine;

namespace FearIndigo.Audio
{
    public abstract class AudioEvent : ScriptableObject
    {
        public abstract void Play(Vector3 position);
        public abstract void Play(AudioSource source, Vector3 position, float start = 0);
    }
}
