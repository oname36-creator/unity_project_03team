using UnityEngine;
using TMPro;


public class CreditLineView : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _partTextMesh;


    public string Text
    {
        get { return _partTextMesh.text; }
        set { _partTextMesh.text = value;}
    }

    public void SetFont(TMP_FontAsset tMP_Asset)
    {
        _partTextMesh.font = tMP_Asset;
    }

}
