using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDG
{
    public class SoundMgr
    {
        public enum Sound
        {
            EatDot,
            EatEnergizer,
            EatGhost,
            EatPacMan,
            ClearLevel
        };

        Dictionary<Sound, AudioClip> _clips;
        private AudioSource _audioSource;

        public SoundMgr ()
        {
            _clips = new Dictionary<Sound, AudioClip> ();
        }

        public void AddEffect (Sound s, AudioClip clip)
        {
            _clips [s] = clip;
        }

        public void SetAudioSource (AudioSource s)
        {
            _audioSource = s;
        }

        public bool IsOn { get; set; }

        public static SoundMgr Singleton { get; set; }

        public void Play (Sound s)
        {
            if ((IsOn) && (_clips.ContainsKey(s))) {
                _audioSource.clip = _clips [s];
                _audioSource.Play();
            }
        }

        internal void ToggleOn ()
        {
            IsOn = !IsOn;
        }
    }
}
