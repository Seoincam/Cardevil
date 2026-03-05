using Cardevil.Pools;
using Cardevil.Sound;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Cardevil.Pools
{
    [RequireComponent(typeof(AudioSource), typeof(Poolable))]
    public class SoundEmitter : MonoBehaviour
    {
        [SerializeField] private Transform targetTransform;
        
        public bool doStopOnTargetNull = true;
            
        private bool isTargetSet;
        
        [SerializeField] private Poolable poolable;
        [SerializeField] private AudioSource _audioSource;
        
        public bool followTimeScale = true;
        private float _initialPitch = 1f;
        
        public AudioSource AudioSource => _audioSource;
        private void Awake()
        {
            if(poolable == null)
                poolable = GetComponent<Poolable>();
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            // 음악 재생이 멈추고, 루프가 아니면 풀에 반환
            if(!_audioSource.isPlaying && !_audioSource.loop)
            {
                ReleaseOrDestroy();
                return;
            }
            
            if (followTimeScale)
            {
                // 메인 오디오 소스 pitch 조절
                _audioSource.pitch = _initialPitch * Time.timeScale;
                
                // 인트로도 타임스케일 적용
                if (_introAudioSource != null && _introAudioSource.isPlaying)
                {
                    _introAudioSource.pitch = _initialPitch * Time.timeScale;
                }
            }
        }
        public void Resume()
        {
            _audioSource.Play();
        }

        public void Pause()
        {
            _audioSource.Pause();
        }

        public void Stop()
        {
            _audioSource.Stop();
        }
        
        public bool IsPlaying => _audioSource.isPlaying;
        public bool IsLooping => _audioSource.loop;
        public AudioClip CurrentlyPlayingAudio => _audioSource.clip;
        public bool IsPaused => _audioSource.isPlaying == false && _audioSource.time > 0;

        public void PlayAudioResource(AudioResource clip, AudioConfigurationSO setting, bool isLoop, Vector3 pos = default)
        {
            _audioSource.resource = clip;
            
            _audioSource.transform.position = pos;
            _audioSource.loop = isLoop;
            setting.ApplyTo(_audioSource);
            _audioSource.Play();
            _initialPitch = _audioSource.pitch;

            // if (!isLoop)
            // {
            //     StartCoroutine(SoundTimer(clip.length));
            // }
        }
        
        private async UniTask SoundTimer(float time)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(time));
            ReleaseOrDestroy();
        }
        
        
        public void SetTarget(Transform target)
        {
            targetTransform = target;
            isTargetSet = target != null;
        }

        private void LateUpdate()
        {
            if (targetTransform != null)
            {
                // 타겟이 살아있으면 따라감
                transform.position = targetTransform.position;
                transform.rotation = targetTransform.rotation;
            }
            else
            {
                if(doStopOnTargetNull && isTargetSet)
                {
                    // doStopOnTargetNull이 True고 타겟이 죽으면 풀에 반환
                    _audioSource.Stop();
                    isTargetSet = false;
                    ReleaseOrDestroy();
                }
            }
        }
        
        public void ReleaseOrDestroy()
        {
            if (poolable && poolable._pool != null)
            {
                poolable.Release();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}