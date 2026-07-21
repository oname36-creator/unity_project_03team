using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingUI : MonoBehaviour
{
    [Header("UI Reference")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    void Start()
    {
        if (SoundManager.Instance != null)
        {
            // 1. 저장되어 있거나 SoundManager에 설정된 초기값 세팅
            if (bgmSlider != null)
            {
                bgmSlider.value = SoundManager.Instance.masterBgmVolume;
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = SoundManager.Instance.masterSfxVolume;
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }
    }

    // BGM 슬라이더 조절 시 호출
    public void OnBGMVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterBgmVolume(value);
        }
    }

    // SFX 슬라이더 조절 시 호출
    public void OnSFXVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterSfxVolume(value);
        }
    }
}