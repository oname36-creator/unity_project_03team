using UnityEngine;
using Unity.Cinemachine;
using Unity.VisualScripting;
public class CameraConfinerManager : Singleton<CameraConfinerManager>
{
    #region Class Attribute
    [Header("시네머신 카메라 설정")]
    [SerializeField] private CinemachineCamera virtualCamera;

    private CinemachineConfiner2D confiner;
    #endregion
    public override void Awake()
    {
        base.Awake();   // 싱글톤의 DontDestoryOnLoad 및 인스턴스 할당 수행

        InitCameraReferences();
    }
    void Start()
    {
        InitCameraReferences();
    }

    #region InitCameraReferences
    /// <summary>
    /// Cinemachine 카메라 및 Confiner 컴포넌트 참조를 안전하게 연결합니다.
    /// </summary>

    private void InitCameraReferences()
    {
        if(virtualCamera == null)
        {
            virtualCamera = FindAnyObjectByType<CinemachineCamera>();
        }

        if (virtualCamera != null)
        {
            confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
        }
    }
    #endregion

    #region UpdateBoundary
    /// <summary>
    ///  카메라 제한 영역 콜라이더 변경
    /// </summary>
    public void UpdateBoundary(Collider2D newBoundary)
    {
        // 2. 씬 전환 등으로 인해 레퍼런스가 비어버렸을 경우를 대비해 실시간 예외 검사를 수행
        if(confiner == null || virtualCamera == null)
        {
            InitCameraReferences();
        }

        if(confiner != null)
        {
            if (newBoundary != null)
            {
                confiner.BoundingShape2D = newBoundary;

                // 변경사항 강제 반영하는 함수
                confiner.InvalidateBoundingShapeCache();
            }
            else
            {
                Debug.LogWarning("전달된 새로운 카메라 바운더리 콜라이더가 null입니다.");
            }

        }
        
        else
        {
            Debug.LogError("CineamachineConfiner2D 컴포넌트를 찾을 수 없습니다. 카메라 설정을 확인해 주세요.");
        }
    }
#endregion

}
