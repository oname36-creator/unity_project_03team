using UnityEngine;

public class Parallax : MonoBehaviour
{
    #region Inspector Data
    [Header("References")]
    [SerializeField] private Transform cam;

    [Header("Parallax Rates (0 = Follow Camera, 1 = Fixed in World)")]
    [Tooltip("0에 가까울수록 카메라를 똑같이 따라가며(원경), 1에 가까울수록 월드에 고정됩니다(근경). 1보다 크면 전경 효과를 냅니다.")]
    [Range(0f, 2f)][SerializeField] private float parallaxEffectX = 0.5f;
    [Range(0f, 2f)][SerializeField] private float parallaxEffectY = 0.5f;

    [Header("Atmospheric Perspective (Color Tint)")]
    [SerializeField] private bool applyAtmosphereTint = false;
    [SerializeField] private Color atmosphereColor = Color.white;
    [Range(0f, 1f)][SerializeField] private float tintStrength = 0.2f;
    #endregion

    private Vector3 startPos;
    private Vector3 startCamPos;
    private SpriteRenderer spriteRenderer;
    private MapManager mapManager;

    void Start()
    {
        if (cam == null)
        {
            if (Camera.main != null)
            {
                cam = Camera.main.transform;
            }
            else
            {
                Camera activeCam = FindAnyObjectByType<Camera>();
                if (activeCam != null)
                {
                    cam = activeCam.transform;
                    Debug.LogWarning($"{gameObject.name}: MainCamera 태그 미지정으로 씬 내 활성 카메라를 자동 할당함");
                }
                else
                {
                    Debug.LogError("Main Camera를 찾을 수 없습니다.");
                    enabled = false;
                    return;
                }
            }
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {

            if (applyAtmosphereTint)
            {
                Color originalColor = spriteRenderer.color;
                spriteRenderer.color = Color.Lerp(originalColor, atmosphereColor, tintStrength);
            }
        }

        mapManager = FindAnyObjectByType<MapManager>();
        startPos = transform.position;
        startCamPos = cam.position;
    }

    private void LateUpdate()
    {
        Vector3 camMoveDistance = cam.position - startCamPos;
        bool isYParallaxActive = mapManager != null && mapManager.CurrentPhaseIndex >= 1;

        // 원근감 최종 오프셋 계산
        float distX = camMoveDistance.x * (1 - parallaxEffectX);
        float distY = isYParallaxActive ? camMoveDistance.y * (1 - parallaxEffectY) : 0f;

        float targetX = startPos.x + distX;
        float targetY = startPos.y + distY;

        // 4. [최종 좌표 대입]
        transform.position = new Vector3(targetX, targetY, transform.position.z);
    }
    
}