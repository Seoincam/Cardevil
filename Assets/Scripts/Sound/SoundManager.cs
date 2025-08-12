using Cardevil.Core;
using Cardevil.DataStructure;
using Cardevil.Pools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Cardevil.Sound
{
    [Serializable]
    public class SoundManager : IClearable
    {
        [Header("Settings")] 
        [SerializeField] private Transform root;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] public AudioMixerGroup[] audioMixerGroups;
        [SerializeField] private float defaultVolume = 0.75f;
        [FormerlySerializedAs("_backgroundAudioConfiguration")]
        [Header("Config")]
        [SerializeField] private AudioConfigurationSO _defaultBackgroundAudioConfiguration; // 사용하지 않음
        [FormerlySerializedAs("_soundEffectAudioConfiguration")] [SerializeField] private AudioConfigurationSO _defaultSoundEffectAudioConfiguration;
        [Header("Audio Clips")]
        private SerializableDict<string, AudioClip> _cachedAudioClips = new ();
        [Header("ETC")]
        [SerializeField] private SoundEmitter _backgroundEmitter;
        [SerializeField] private List<SoundEmitter> _sfxEmitters = new List<SoundEmitter>();
        [SerializeField] private SerializableDict<string, SoundEmitter> _cachedBackgroundSoundEmitters = new SerializableDict<string, SoundEmitter>();
        [SerializeField] private string _currentPlayingBackgroundMusicName = string.Empty;

        public AudioMixerGroup MasterGroup
        {
            get => audioMixerGroups[(int)Define.Sound.Master];
            private set => audioMixerGroups[(int)Define.Sound.Master] = value;
        }
        public AudioMixerGroup MusicGroup
        {
            get => audioMixerGroups[(int)Define.Sound.BGM];
            private set => audioMixerGroups[(int)Define.Sound.BGM] = value;
        }
        public AudioMixerGroup SfxGroup
        {
            get => audioMixerGroups[(int)Define.Sound.SFX];
            private set => audioMixerGroups[(int)Define.Sound.SFX] = value;
        }
        

        public void Init()
        {
            GameObject root = GameObject.Find("@Sound");
            if (root == null)
            {
                root = new GameObject { name = "@Sound" };
                Object.DontDestroyOnLoad(root);
                if (_backgroundEmitter == null)
                {
                    SoundEmitter bgmEmitter =
                        Managers.Pool.Get<SoundEmitter>(PoolManager.Poolables.SoundEmitter, root.transform);
                    bgmEmitter.name = "BGMEmitter";
                }
            }
            // 사운드 루트 초기화
            this.root = root.transform;
            
            // ConfigurationSo 초기화
            if (_defaultBackgroundAudioConfiguration == null)
            {
                _defaultBackgroundAudioConfiguration = Resources.Load<AudioConfigurationSO>("Audio/DefaultBgmConfig");
            }
            if (_defaultSoundEffectAudioConfiguration == null)
            {
                _defaultSoundEffectAudioConfiguration = Managers.Resource.Load<AudioConfigurationSO>("Audio/DefaultSfxConfig");
            }

            // 믹서 그룹 초기화
            audioMixerGroups = new AudioMixerGroup[(int)Define.Sound.MaxCount];
            audioMixer = Resources.Load<AudioMixer>("Audio/MainMixer");
            MasterGroup = audioMixer.FindMatchingGroups("Master")[0];
            MusicGroup = audioMixer.FindMatchingGroups("Background")[0];
            SfxGroup = audioMixer.FindMatchingGroups("SFX")[0];

            // 기본 볼륨 설정
            float savedMasterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
            float savedMusicVolume = PlayerPrefs.GetFloat("BackgroundVolume", defaultVolume);
            float savedSfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultVolume);
            SetMasterVolume(savedMasterVolume);
            SetMusicVolume(savedMusicVolume);
            SetSfxVolume(savedSfxVolume);
        }
        
        public void Clear()
        {
            foreach (var emitter in _sfxEmitters)
            {
                if (emitter != null)
                {
                    emitter.ReleaseOrDestroy();
                }
            }
            _sfxEmitters.Clear();
            _cachedAudioClips.Clear();
            foreach (var kvp in _cachedBackgroundSoundEmitters)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.ReleaseOrDestroy();
                }
            }
            _cachedBackgroundSoundEmitters.Clear();
            _currentPlayingBackgroundMusicName = string.Empty;
        }
        
        /// <param name="percent"> 0.0001 ~ 1의 값.</param>
        public void SetMasterVolume(float percent, bool save = true)
        {
            var clampedPercent = Mathf.Clamp(percent, 0.0001f, 1f);
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(clampedPercent) * 20);
            if (save)
            {
                PlayerPrefs.SetFloat("MasterVolume", clampedPercent);
                PlayerPrefs.Save();
            }
        }
        /// <param name="percent"> 0.0001 ~ 1의 값.</param>
        public void SetMusicVolume(float percent, bool save = true)
        {
            var clampedPercent = Mathf.Clamp(percent, 0.0001f, 1f);
            audioMixer.SetFloat("BackgroundVolume", Mathf.Log10(clampedPercent) * 20);
            if (save)
            {
                PlayerPrefs.SetFloat("BackgroundVolume", clampedPercent);
                PlayerPrefs.Save();
            }
        }
        /// <param name="percent"> 0.0001 ~ 1의 값.</param>
        public void SetSfxVolume(float percent, bool save = true)
        {
            var clampedPercent = Mathf.Clamp(percent, 0.0001f, 1f);
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(clampedPercent) * 20);
            if (save)
            {
                PlayerPrefs.SetFloat("SFXVolume", clampedPercent);
                PlayerPrefs.Save();
            }
        }
        
        public SoundEmitter Play(string path, Define.Sound type = Define.Sound.SFX)
        {
            AudioClip audioClip = GetOrAddAudioClip(path, type);
            return Play(audioClip, type);
        }
        
        public SoundEmitter Play(AudioClip audioClip, Define.Sound type = Define.Sound.SFX)
        {
            if (audioClip == null)
            {
                return null;
            }
            if (type == Define.Sound.BGM)
            {
                PlayBackgroundMusic(audioClip);
                return _cachedBackgroundSoundEmitters.TryGetValue(audioClip.name, out SoundEmitter emitter) 
                    ? emitter 
                    : null;
            }
            // SFX 출력
            return PlaySfxAt(audioClip, Vector3.zero, false, _defaultSoundEffectAudioConfiguration);
        }
        
        /// <summary>
        /// 해당 Transform을 따라다니는 사운드 재생
        /// </summary>
        /// <param name="path"></param>
        /// <param name="target"></param>
        /// <param name="stopOnTargetNull"></param>
        /// <param name="audioConfiguration"></param>
        public SoundEmitter PlaySfxTo(string path, Transform target, bool stopOnTargetNull = true, AudioConfigurationSO audioConfiguration = null)
        {
            AudioClip clip = GetOrAddAudioClip(path);
            if (clip == null)
            {
                Debug.LogWarning($"AudioClip not found at path: {path}");
                return null;
            }
            return PlaySfxTo(clip, target, stopOnTargetNull, audioConfiguration);
        }
        /// <summary>
        /// 해당 Transform을 따라다니는 사운드 재생
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="target"></param>
        /// <param name="stopOnTargetNull"> 해당 타겟 transform이 사라지면 재생 멈춤</param>
        public SoundEmitter PlaySfxTo(AudioClip clip, Transform target, bool stopOnTargetNull = true, AudioConfigurationSO audioConfiguration = null)
        {

            if (clip == null)
                return null;
            SoundEmitter soundEmitter = Managers.Pool.Get<SoundEmitter>(PoolManager.Poolables.SoundEmitter);
            soundEmitter.transform.SetParent(root);
            soundEmitter.SetTarget(target);
            soundEmitter.doStopOnTargetNull = stopOnTargetNull;
            soundEmitter.PlayAudioClip(clip, audioConfiguration ?? _defaultSoundEffectAudioConfiguration, false);
            return soundEmitter;
        }
        
        /// <summary>
        /// 해당 위치에서 사운드 재생
        /// </summary>
        /// <param name="path"></param>
        /// <param name="position"></param>
        /// <param name="isLoop"></param>
        /// <param name="audioConfiguration"></param>
        public SoundEmitter PlaySfxAt(string path, Vector3 position, bool isLoop = false, AudioConfigurationSO audioConfiguration = null)
        {
            AudioClip clip = GetOrAddAudioClip(path);
            if (clip == null)
            {
                Debug.LogWarning($"AudioClip not found at path: {path}");
                return null;
            }
            return PlaySfxAt(clip, position, isLoop, audioConfiguration);
        }

        /// <summary>
        /// 해당 위치에서 사운드 재생
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="position"></param>
        /// <param name="isLoop"></param>
        /// <param name="audioConfiguration"></param>
        public SoundEmitter PlaySfxAt(AudioClip clip, Vector3 position, bool isLoop = false, AudioConfigurationSO audioConfiguration = null)
        {
            if (clip == null)
                return null;
            SoundEmitter soundEmitter = Managers.Pool.Get<SoundEmitter>(PoolManager.Poolables.SoundEmitter);
            soundEmitter.transform.SetParent(root);
            soundEmitter.transform.position = position;
            soundEmitter.PlayAudioClip(clip, audioConfiguration ?? _defaultSoundEffectAudioConfiguration, isLoop);
            _sfxEmitters.Add(soundEmitter);
            return soundEmitter;
        }
        
        /// <summary>
        /// 배경음악 재생.
        /// 이미 재생중인 배경음악이 있다면 해당 음악을 일시정지한다.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ifPausedResume">이전에 일시정지 되었다면, 이어서 재생</param>
        public void PlayBackgroundMusic(string path, bool ifPausedResume = false)
        {
            AudioClip clip = GetOrAddAudioClip(path);
            if (clip == null)
            {
                Debug.LogWarning($"AudioClip not found at path: {path}");
                return;
            }
            PlayBackgroundMusic(clip, ifPausedResume);
        }

        /// <summary>
        /// 배경음악 재생.
        /// 이미 재생중인 배경음악이 있다면 해당 음악을 일시정지한다.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ifPausedResume">이전에 일시정지 되었다면, 이어서 재생</param>
        public void PlayBackgroundMusic(AudioClip clip, bool ifPausedResume = false)
        {
            if (clip == null)
                return;
            SoundEmitter soundEmitter;
            
            // 이미 재생중인 배경음악이 있다면 일시정지한다.
            if(string.IsNullOrEmpty(_currentPlayingBackgroundMusicName) == false)
            {
                if (_cachedBackgroundSoundEmitters.TryGetValue(_currentPlayingBackgroundMusicName, out soundEmitter) && soundEmitter)
                {
                    AudioSource audioSource = soundEmitter.AudioSource;
                    if (audioSource.isPlaying)
                    {
                        audioSource.Pause();
                    }
                }
            }
            _currentPlayingBackgroundMusicName = clip.name;
            
            // 배경음악이 이전에 일시정지 된 적 있는지 확인.
            if (_cachedBackgroundSoundEmitters.TryGetValue(clip.name, out soundEmitter) && soundEmitter)
            {
                Debug.Log("Checking if sound emitter is playing");
                AudioSource audioSource = soundEmitter.AudioSource;
                if (soundEmitter.IsPaused && ifPausedResume)
                {
                    audioSource.UnPause();
                }
                else
                {
                    soundEmitter.transform.SetParent(root);
                    soundEmitter.transform.position = Vector3.zero;
                    audioSource.clip = clip;
                    audioSource.loop = true;
                    audioSource.time = 0;
                    audioSource.Play();
                }
            }
            else
            {
                soundEmitter = Managers.Pool.Get<SoundEmitter>(PoolManager.Poolables.SoundEmitter);
                soundEmitter.transform.SetParent(root);
                soundEmitter.transform.position = Vector3.zero;
                AudioSource audioSource = soundEmitter.GetComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = MusicGroup;
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.Play();
                audioSource.spatialize = false;
                
                _cachedBackgroundSoundEmitters.Add(clip.name, soundEmitter);
            }
            
        }




        AudioClip GetOrAddAudioClip(string path, Define.Sound type = Define.Sound.SFX)
        {
            if (path.Contains("Sounds/") == false)
            {
                path = $"Sounds/{path}";
            }
            AudioClip audioClip = null;
            if (type == Define.Sound.BGM)
            {
                audioClip = Managers.Resource.Load<AudioClip>(path);
            }
            else
            {
                if (_cachedAudioClips.TryGetValue(path, out audioClip) == false)
                {
                    audioClip = Managers.Resource.Load<AudioClip>(path);
                    _cachedAudioClips.Add(path, audioClip);
                }
            }
            if (audioClip == null)
            {
                Debug.Log($"AudioClip Missing : {path}");
            }
            return audioClip;
        }


    }

}
