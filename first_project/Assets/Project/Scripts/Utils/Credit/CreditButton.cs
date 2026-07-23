using UnityEngine;




public class CreditButton : MonoBehaviour
{
    [Header("Credit Ui")]
    [SerializeField] private GameObject _creditUI;
    [SerializeField] private GameObject _mainUI;


    public void OnClickCreditButton() 
    {
        SoundManager.Instance.PlayBGM("CreditBGM");
        _creditUI.SetActive(true);
        _mainUI.SetActive(false);
    }

    public void OnDisableStartSceneCreditUI()
    {
        SoundManager.Instance.PlayBGM("StartSceneBGM");
        _mainUI.SetActive(true);
    }

}
