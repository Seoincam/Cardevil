using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SoundManager
{
    public AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];
    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
    [SerializeField]
    public AudioMixer audioMixer;
    public AudioMixerGroup[] audioMixerGroups;
    public AudioSource BGM;
    public AudioSource SFX;
    

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            audioMixer = Resources.Load<AudioMixer>("Sounds/SoundSetting");
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            for (int i = 0; i < soundNames.Length - 1; i++)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>();
                _audioSources[i].outputAudioMixerGroup = audioMixer.FindMatchingGroups($"{soundNames[i]}")[0];
                go.transform.parent = root.transform;
            }
        }
        if (audioMixer == null) { Debug.Log("audioMixer is Null"); }
      
    }
    public void Play(AudioClip audioClip, Define.Sound type = Define.Sound.SFX, float pitch = 1.0f)
    {
        if (audioClip == null)
        {
            return;
        }
        if (type == Define.Sound.BGM)
        {
            AudioSource audioSource = _audioSources[(int)Define.Sound.BGM];
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            AudioSource audioSource = _audioSources[(int)Define.Sound.SFX];
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
    }
    public void PlayDelayed(AudioClip audioClip, float delay, Define.Sound type = Define.Sound.BGM, float pitch = 1.0f)
    {
        //audioMixer = Managers.Resource.Load<AudioMixer>("AudioMixer/SoundSetting");
        if (audioClip == null)
        {
            return;
        }
        if (type == Define.Sound.BGM)
        {
            AudioSource audioSource = _audioSources[(int)Define.Sound.BGM];
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            //audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("BGM")[0];
            audioSource.PlayDelayed(3.0f);
        }
        else
        {
            AudioSource audioSource = _audioSources[(int)Define.Sound.SFX];
            audioSource.pitch = pitch;
            //audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
            audioSource.PlayOneShot(audioClip);
        }
    }
    public void Play(string path, Define.Sound type = Define.Sound.SFX, float pitch = 1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        Play(audioClip, type, pitch);
    }
    public void PlayDelayed(string path, float delay, Define.Sound type = Define.Sound.BGM, float pitch = 1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        PlayDelayed(audioClip, delay, type, pitch);
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
            if (_audioClips.TryGetValue(path, out audioClip) == false)
            {
                audioClip = Managers.Resource.Load<AudioClip>(path);
                _audioClips.Add(path, audioClip);
            }
        }
        if (audioClip == null)
        {
            Debug.Log($"AudioClip Missing : {path}");
        }
        return audioClip;
    }
    public void Stop(AudioSource audioSource)
    {
        audioSource.Stop();
    }
    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
        _audioClips.Clear();
    }
    public void Audiorate(int volume)
    {
        AudioSource audioSource1 = _audioSources[(int)Define.Sound.BGM];
        audioSource1.pitch = volume;
        AudioSource audioSource2 = _audioSources[(int)Define.Sound.SFX];
        audioSource2.pitch = volume;

    }
    public void AudiorateBGM(int volume)
    {
       
    }
    public void AudiorateSFX(int volume)
    {
        AudioSource audioSource2 = _audioSources[(int)Define.Sound.SFX];
        audioSource2.pitch = volume;

    }
}
