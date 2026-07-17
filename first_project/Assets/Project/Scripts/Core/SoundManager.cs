using System.Collections.Generic;
using UnityEngine;

// 1. 개별 사운드의 설정값을 담을 컨테이너 클래스 생성
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

public class SoundManager : Singleton<SoundManager>
{
    [Header("BGM")]
    [SerializeField] private AudioClip _startSceneBGM;
    [SerializeField] private AudioClip _gameSceneBGM;
    [SerializeField] private AudioClip _endingSceneBGM;





    private Dictionary<string, SoundData> _bgmDic = new Dictionary<string, SoundData>();
    private Dictionary<string, SoundData> _sfxDic = new Dictionary<string, SoundData>();

    private AudioSource _bgmSource;
    private AudioSource _sfxSource;

    [Header("Master Volume")]
    [Range(0f, 1f)] public float masterBgmVolume = 1.0f;
    [Range(0f, 1f)] public float masterSfxVolume = 1.0f;



    public const string StartSceneBGM = "StartSceneBGM";
    public const string GameSceneBGM = "GameSceneBGM";
    public const string EndingSceneBGM = "EndingSceneBGM";




    private void Start()
    {
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;

        _sfxSource = gameObject.AddComponent<AudioSource>();

        SetBGM();
        SetSfx();
    }

    private void SetBGM()
    {
        // BGM은 기본 볼륨 1.0f로 세팅
        _bgmDic = new Dictionary<string, SoundData>
        {
            { StartSceneBGM, new SoundData(_startSceneBGM) },
            { GameSceneBGM, new SoundData(_gameSceneBGM) },
            { EndingSceneBGM, new SoundData(_endingSceneBGM) }
        };
    }

    private void SetSfx()
    {
        _sfxDic = new Dictionary<string, SoundData>
        {
            //{ ButtonClickSfx, new SoundData(_buttonClickSFX, 0.8f) }
        };
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

    public void PlaySFX(string key)
    {
        if (_sfxDic.TryGetValue(key, out SoundData data))
        {
            _sfxSource.pitch = data.Pitch;
            _sfxSource.PlayOneShot(data.Clip, masterSfxVolume * data.Volume);
        }
    }
}