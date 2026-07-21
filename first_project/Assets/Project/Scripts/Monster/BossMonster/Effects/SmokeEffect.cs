using UnityEngine;

public class SmokeEffect : MonoBehaviour
{
    public void SmokeEffectPush()
    {
        ObjectPoolManager.Instance.SmokeEffectPush(this.gameObject);
    }
}
