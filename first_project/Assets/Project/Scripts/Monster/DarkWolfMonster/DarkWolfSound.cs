using UnityEngine;

public class DarkWolfSound : MonoBehaviour
{
    [Header("Audio Clip")]
    [SerializeField] private AudioClip _idleClip;
    [SerializeField] private AudioClip _attackClip;
    [SerializeField] private AudioClip _hurtClip;
    [SerializeField] private AudioClip _deadClip;


    void Start()
    {
        SoundManager.Instance.AddSfx("DarkWolfIdle", _idleClip, 0.2f, 1f);
        SoundManager.Instance.AddSfx("DarkWolfAttack", _attackClip, 0.3f, 1f);
        SoundManager.Instance.AddSfx("DarkWolfHurt", _hurtClip, 0.3f, 1f);
        SoundManager.Instance.AddSfx("DarkWolfDead", _deadClip, 0.2f, 1f);
    }


}
