using UnityEngine;

public class SmokeBurstEffect : MonoBehaviour
{
    public void SmokeBurstEffectPush()
    {
        ObjectPoolManager.Instance.SmokeBurstEffectPush(this.gameObject);
    }
}
