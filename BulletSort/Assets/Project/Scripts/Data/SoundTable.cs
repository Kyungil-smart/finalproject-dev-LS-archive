using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;

namespace Data.Table
{
    [CreateAssetMenu(fileName = "SoundTable", menuName = "Scriptable Objects/SoundTable")]
    public class SoundTable : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public EAudioClipEnum audioEnum;
            public AudioClip audioClip;
        }
        
        [SerializeField] private Entry[] _entries;
        
        private Dictionary<EAudioClipEnum, AudioClip> _lookup;
        
        public AudioClip GetbyEnum(EAudioClipEnum audioEnum)
        {
            BuildLookupIfNeeded();
            if (Enum.IsDefined(typeof(EAudioClipEnum), audioEnum)) return null;
            return _lookup.TryGetValue(audioEnum, out var clip) ? clip : null;
        }
        
        private void BuildLookupIfNeeded()
        {
            if (_lookup != null) return;
            _lookup = new Dictionary<EAudioClipEnum, AudioClip>(_entries.Length);
            foreach (var e in _entries)
            {
                if (e.audioClip == null || Enum.IsDefined(typeof(EAudioClipEnum), e.audioEnum)) continue;
                
                if (!_lookup.ContainsKey(e.audioEnum))
                    _lookup[e.audioEnum] = e.audioClip;
                else
                    Debug.LogWarning($"PieceSpriteTable : 중복 이름 {e.audioEnum}");
            }
        }
    }
}
