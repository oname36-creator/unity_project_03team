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

    private AudioSource _sfxSource;



    public override void Awake()
    {

        base.Awake();

        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;


        _sfxSource = gameObject.AddComponent<AudioSource>();

        SetBGM();
        SetSfx();

        masterBgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
        masterSfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        if (_sfxSource != null) _sfxSource.volume = masterSfxVolume;
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

    public void StopBGM() 
    {
        if (_bgmSource == null) { return; }
        _bgmSource.Stop();
    }


    public void PlaySFX(string key)
    {
        if (_sfxDic.TryGetValue(key, out SoundData data))
        {
            if (_sfxSource != null)
            {
                _sfxSource.pitch = data.Pitch;
                _sfxSource.PlayOneShot(data.Clip, data.Volume);
            }
        }
    }

    public void PauseSFX()
    {
        if (_sfxSource != null)
        {
            _sfxSource.Pause();
        }
    }

    public void ResumeSFX()
    {
        if (_sfxSource != null)
        {
            _sfxSource.UnPause();
        }
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

        if (_sfxSource != null)
        {
            _sfxSource.volume = masterSfxVolume;
        }

        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}