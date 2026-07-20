using UnityEngine;

public class FlyMonsterSound : MonoBehaviour
{

    [Header("Audio Clip")]
    [SerializeField] private AudioClip _attackClip;
    [SerializeField] private AudioClip _hurtClip;
    [SerializeField] private AudioClip _deadClip;



    void Start()
    {
        SoundManager.Instance.AddSfx("BirdAttack", _attackClip, 0.1f, 1f);        
        SoundManager.Instance.AddSfx("BirdHurt", _hurtClip, 0.3f, 1f);        
        SoundManager.Instance.AddSfx("BirdDead", _deadClip, 0.2f, 1f);        
    }


}
