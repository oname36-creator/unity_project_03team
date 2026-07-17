using UnityEngine;




public class CreditButton : MonoBehaviour
{
    [Header("Credit Ui")]
    [SerializeField] private GameObject _creditUI;


    public void OnClickCreditButton() 
    {
        _creditUI.SetActive(true);
    }
}
