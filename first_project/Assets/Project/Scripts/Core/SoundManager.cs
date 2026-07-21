using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SoundData
{
    public AudioClip Clip;
    public float Volume;
    public float Pitch;

    public SoundData(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        Clip = clip;
        Volume = volume;
        Pitch = pitch;
    }
}

[Serializable]
public struct AudioClipPair 
{
    public string Key;
    public SoundData SoundData;

}

public class SoundManager : Singleton<SoundManager>
{

    [Header("BGM")]
    [SerializeField] private List<AudioClipPair> _audiobgmList;

    [Header("SFX")]
    [SerializeField] private List<AudioClipPair> _audioSfxList;

    [Header("Master Volume")]
    [Range(0f, 1f)] public float masterBgmVolume = 1.0f;
    [Range(0f, 1f)] public float masterSfxVolume = 1.0f;



    public const string StartSceneBGM = "StartSceneBGM";
    public const string GameSceneBGM = "GameSceneBGM";
    public const string EndingSceneBGM = "EndingSceneBGM";
    public const string CreditBGM = "CreditBGM";

    public const string GunAttackSFX = "GunAttackSFX";

    private Dictionary<string, SoundData> _bgmDic = new Dictionary<string, SoundData>();
    private Dictionary<string, SoundData> _sfxDic = new Dictionary<string, SoundData>();

    private AudioSource _bgmSource;

    // playOneShot을 사용하면 적절히 알아서 믹싱이 됨
    // 알아둘것

    private AudioSource[] _sfxAudioSources;



    private void Awake()
    {
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;


        _sfxAudioSources = new AudioSource[3];
        
        for(int i = 0; i < 3; i++)
        {
            AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxAudioSources[i] = sfxSource;
        }

        SetBGM();
        SetSfx();

        masterBgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
        masterSfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
    }

    private void SetBGM()
    {

        foreach (AudioClipPair pair in _audiobgmList) 
        {
            _bgmDic.Add(pair.Key, pair.SoundData);
        }
    }

    private void SetSfx()
    {
        foreach (AudioClipPair pair in _audioSfxList)
        {
            _sfxDic.Add(pair.Key, pair.SoundData);
        }
    }



    public void AddSfx(string key, AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (!_sfxDic.ContainsKey(key))
        {
            _sfxDic.Add(key, new SoundData(clip, volume, pitch));
        }
    }


    public void ModifySfxProperty(string key, float volume, float pitch = 1f)
    {
        if (_sfxDic.TryGetValue(key, out SoundData data))
        {
            data.Volume = volume;
            data.Pitch = pitch;
        }
        else
        {
            Debug.LogWarning($"[SoundManager] 조절하려는 키가 없습니다: {key}");
        }
    }


    public void PlayBGM(string key)
    {
        if (_bgmDic.TryGetValue(key, out SoundData data))
        {

            _bgmSource.clip = data.Clip;
            _bgmSource.pitch = data.Pitch;
            _bgmSource.volume = masterBgmVolume * data.Volume;
            _bgmSource.Play();
        }
    }

    public void PauseBGM() 
    {
        if(_bgmSource == null) { return; }
        _bgmSource.Pause();
    }

    public void ResumeBGM() 
    {
        if(_bgmSource != null) 
        {
            _bgmSource.UnPause();
        }
    }


    public void PlaySFX(string key)
    {
        if (_sfxDic.TryGetValue(key, out SoundData data))
        {
            // 1. 현재 재생 중이지 않은(비어있는) AudioSource를 찾습니다.
            AudioSource availableSource = GetAvailableSfxSource();

            if (availableSource != null)
            {
                availableSource.clip = data.Clip;
                availableSource.pitch = data.Pitch;
                availableSource.volume = masterSfxVolume * data.Volume;

                availableSource.Play();
            }
            else
            {
                Debug.LogWarning("[SoundManager] 모든 SFX 오디오 소스가 사용 중입니다.");
            }
        }
    }

    private AudioSource GetAvailableSfxSource()
    {
        for (int i = 0; i < _sfxAudioSources.Length; i++)
        {
            if (!_sfxAudioSources[i].isPlaying)
            {
                return _sfxAudioSources[i];
            }
        }
        return null; // 모든 소스가 재생 중일 경우
    }


    public void SetMasterBgmVolume(float volume)
    {
        masterBgmVolume = volume;

        // 현재 재생 중인 BGM에도 즉시 볼륨 반영
        if (_bgmSource != null)
        {
            _bgmSource.volume = masterBgmVolume;
        }

        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

 
    public void SetMasterSfxVolume(float volume)
    {
        masterSfxVolume = volume;

        // 현재 재생 중인 모든 SFX 채널에도 즉시 반영
        if (_sfxAudioSources != null)
        {
            foreach (var source in _sfxAudioSources)
            {
                if (source != null)
                {
                    source.volume = masterSfxVolume;
                }
            }
        }

        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}