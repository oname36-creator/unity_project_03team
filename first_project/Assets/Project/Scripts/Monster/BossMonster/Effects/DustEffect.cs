using UnityEngine;

public class DustEffect : MonoBehaviour
{

    public void DustEffectPush() 
    {
        ObjectPoolManager.Instance.DustEffectPush(this.gameObject);
    }

}
