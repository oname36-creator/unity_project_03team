using UnityEngine;

public class ScrollingMap : MonoBehaviour
{
    public MapData scrollSpeed;
    private Material bgMaterial;

    void Start()
    {
        bgMaterial = GetComponent<MeshRenderer>().material;
    }
    void Update()
    {
        bgMaterial.mainTextureOffset += Vector2.left * scrollSpeed.speed * Time.deltaTime;
    }
}
