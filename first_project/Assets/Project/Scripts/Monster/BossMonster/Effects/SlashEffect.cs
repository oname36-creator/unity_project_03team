using UnityEngine;

public class SlashEffect : MonoBehaviour
{


    public void SlashEffectPush()
    {
        ObjectPoolManager.Instance.SlashEffectPush(this.gameObject);
    }



}
