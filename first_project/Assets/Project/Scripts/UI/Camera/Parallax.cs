using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cam;

    [Header("Parallax Rates (0 = Follow Camera, 1 = Fixed in World)")]
    [Tooltip("0에 가까울수록 카메라를 똑같이 따라가며(원경), 1에 가까울수록 월드에 고정됩니다(근경). 1보다 크면 전경 효과를 냅니다.")]
    [Range(0f, 2f)][SerializeField] private float parallaxEffectX = 0.5f;
    [Range(0f, 2f)][SerializeField] private float parallaxEffectY = 0.5f;

    [Header("Loop Settings")]
    [SerializeField] private bool loopHorizontal = true;
    [SerializeField] private bool loopVertical = false;

    [Header("Atmospheric Perspective (Color Tint)")]
    [SerializeField] private bool applyAtmosphereTint = false;
    [SerializeField] private Color atmosphereColor = Color.white;
    [Range(0f, 1f)][SerializeField] private float tintStrength = 0.2f;

    private Vector3 startPos;
    private Vector3 startCamPos;
    private float textureUnitSizeX;
    private float textureUnitSizeY;
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
            textureUnitSizeX = spriteRenderer.sprite.rect.width / spriteRenderer.sprite.pixelsPerUnit;
            textureUnitSizeY = spriteRenderer.sprite.rect.height / spriteRenderer.sprite.pixelsPerUnit;

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

        // 1. [X축 무한 루핑 검사 및 startPos 보정]
        if (loopHorizontal && textureUnitSizeX > 0)
        {
            float tempX = camMoveDistance.x * parallaxEffectX;
            if (tempX > startPos.x - startCamPos.x + textureUnitSizeX)
            {
                startPos.x += textureUnitSizeX;
            }
            else if (tempX < startPos.x - startCamPos.x - textureUnitSizeX)
            {
                startPos.x -= textureUnitSizeX;
            }
        }

        // 2. [Y축 무한 루핑 검사 및 startPos 보정]
        if (isYParallaxActive && loopVertical && textureUnitSizeY > 0)
        {
            float tempY = camMoveDistance.y * parallaxEffectY;
            if (tempY > startPos.y - startCamPos.y + textureUnitSizeY)
            {
                startPos.y += textureUnitSizeY;
            }
            else if (tempY < startPos.y - startCamPos.y - textureUnitSizeY)
            {
                startPos.y -= textureUnitSizeY;
            }
        }

        // 3. [보정이 끝난 startPos를 기반으로 최종 오프셋 계산]
        float distX = camMoveDistance.x * (1 - parallaxEffectX);
        float distY = 0f;
        if (isYParallaxActive)
        {
            distY = camMoveDistance.y * (1 - parallaxEffectY);
        }

        float targetX = startPos.x + distX;
        float targetY = startPos.y + distY;

        // 4. [최종 좌표 대입] (튀는 현상 완벽 방지)
        transform.position = new Vector3(targetX, targetY, transform.position.z);
    }
}