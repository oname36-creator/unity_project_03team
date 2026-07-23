using UnityEngine;
using Unity.Cinemachine;
using Unity.VisualScripting;
using System.Collections;
using System;
public class CameraConfinerManager : Singleton<CameraConfinerManager>
{
    #region Class Attribute
    [Header("시네머신 카메라 설정")]
    [SerializeField] private CinemachineCamera virtualCameraA;
    [SerializeField] private CinemachineCamera virtualCameraB;

    [Header("Orthographic Size 설정")]
    [SerializeField] private float normalOrthoSize = 5f;
    [SerializeField] private float yTrackingOrthoSize = 7.5f;
    [Header("DeadZone Y 설정")]
    [SerializeField] private float normalDeadZoneY = 0.8f;
    [SerializeField] private float yTrackingDeadZoneY = 0.2f;

    private CinemachineConfiner2D confinerA;
    private CinemachineConfiner2D confinerB;

    private CinemachinePositionComposer positionComposerA;
    private CinemachinePositionComposer positionComposerB;

    private Coroutine _yTrackingCoroutineA;  // 처음 y축 보정 제어용 변수
    private Coroutine _yTrackingCoroutineB;

    // 현재 카메라를 화면 출력용 메인으로 쓰고 있는지 기록(NextCamera와 교차 활성화하기 위함)
    private bool _isUsingCameraA = true;

    // 직전에 사용하던 전체 카메라 유형 기록
    // private bool _wasUsingYCameraGroup = false;

    // 현재 설정된 바운더리 콜라이더를 캐싱하여 중복 처리를 방지합니다.
    private Collider2D _currentBoundary;

    public bool IsYTrackingActive { get; private set; } = false;
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
        if (virtualCameraA == null || virtualCameraB == null)
        {
            FindVirtualCamerasInScene();
        }

        if (virtualCameraA != null)
        {
            confinerA = virtualCameraA.GetComponent<CinemachineConfiner2D>();
            positionComposerA = virtualCameraA.GetComponent<CinemachinePositionComposer>();
        }

        if (virtualCameraB != null)
        {
            confinerB = virtualCameraB.GetComponent<CinemachineConfiner2D>();
            positionComposerB = virtualCameraB.GetComponent<CinemachinePositionComposer>();
        }
    }

    private void FindVirtualCamerasInScene()
    {
        CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        foreach (var cam in cameras)
        {
            if (virtualCameraA == null && (cam.name.Contains("CameraA") || cam.name.Contains("vcam1") || cam.name.Contains("VirtualCameraA")))
            {
                virtualCameraA = cam;
            }
            else if (virtualCameraB == null && (cam.name.Contains("CameraB") || cam.name.Contains("vcam2") || cam.name.Contains("VirtualCameraB")))
            {
                virtualCameraB = cam;
            }
        }
    }
    #endregion

    #region UpdateBoundary
    /// <summary>
    ///  카메라 제한 영역 콜라이더 변경
    /// </summary>
    public void UpdateBoundary(Collider2D newBoundary, bool enableYTracking)
    {
        if (virtualCameraA == null || virtualCameraB == null || confinerA == null || confinerB == null)
        {
            InitCameraReferences();
        }
        if (newBoundary == null)
        {
            Debug.LogWarning("전달된 새로운 카메라 바운더리 콜라이더가 null입니다.");
            return;
        }
        if (newBoundary == _currentBoundary) return;
        _currentBoundary = newBoundary;
        IsYTrackingActive = enableYTracking;
        // Y축 옵션 여부에 따라 Ortho Size와 DeadZone Y 결정
        float targetOrthoSize = enableYTracking ? yTrackingOrthoSize : normalOrthoSize;
        float targetDeadZoneY = enableYTracking ? yTrackingDeadZoneY : normalDeadZoneY;
        // A/B 핑퐁 전환
        if (_isUsingCameraA)
        {
            ActivateCameraAndDeactivateOthers(virtualCameraA, confinerA, newBoundary, targetOrthoSize, targetDeadZoneY, positionComposerA);
            _isUsingCameraA = false;
        }
        else
        {
            ActivateCameraAndDeactivateOthers(virtualCameraB, confinerB, newBoundary, targetOrthoSize, targetDeadZoneY, positionComposerB);
            _isUsingCameraA = true;
        }
    }

    #endregion

    #region ActivateCameraAndDeactivateOthers
    /// <summary>
    /// 대상 카메라의 우선수위를 높이고 바운더리를 할당하며, 나머지 모든 가상 카메라의
    /// 우선 순위를 낮춥니다.
    /// </summary>

    private void ActivateCameraAndDeactivateOthers(CinemachineCamera targetCamera, CinemachineConfiner2D targetConfiner, Collider2D boundary, float targetOrthoSize, float targetDeadZoneY, CinemachinePositionComposer composer)
    {
        if (targetCamera == null || targetConfiner == null) return;
        // 1. 바운더리 할당
        targetConfiner.BoundingShape2D = boundary;
        targetConfiner.InvalidateBoundingShapeCache();
        // 2. Lens Orthographic Size 설정 (Cinemachine 3.x)
        var lens = targetCamera.Lens;
        lens.OrthographicSize = targetOrthoSize;
        targetCamera.Lens = lens;
        // 3. Y축 데드존 설정
        if (composer != null)
        {
            SetDeadZoneY(composer, targetDeadZoneY);
        }
        // 4. 우선순위 교차 전환 (targetCamera는 15, 비활성은 10)
        targetCamera.Priority = 15;
        if (virtualCameraA != null && virtualCameraA != targetCamera)
        {
            virtualCameraA.Priority = 10;
            if (confinerA != null)
            {
                confinerA.BoundingShape2D = null;
                confinerA.InvalidateBoundingShapeCache();
            }
        }
        if (virtualCameraB != null && virtualCameraB != targetCamera)
        {
            virtualCameraB.Priority = 10;
            if (confinerB != null)
            {
                confinerB.BoundingShape2D = null;
                confinerB.InvalidateBoundingShapeCache();
            }
        }
    }
    #endregion

    #region InitizlizeYTracking
    ///<summary>
    /// 게임 시작 시 가상 카메라들의 Y축을 부드럽게 고정 상태로 정렬하는 연출을 시작
    /// </summary>
    public void InitizlizeYTracking()
    {
        if (positionComposerA == null || positionComposerB == null)
        {
            InitCameraReferences();
        }

        // 실행 중이던 기존 보정 코루틴들을 정리
        if(_yTrackingCoroutineA != null)
        {
            StopCoroutine(_yTrackingCoroutineA);
            _yTrackingCoroutineA = null;
        }
        if(_yTrackingCoroutineB != null)
        {
            StopCoroutine(_yTrackingCoroutineB);
            _yTrackingCoroutineB = null;
        }

        if(positionComposerA != null)
        {
            _yTrackingCoroutineA = StartCoroutine(CoInitialYCorrection(positionComposerA, 1.5f));
        }
        if (positionComposerB != null)
        {
            _yTrackingCoroutineB = StartCoroutine(CoInitialYCorrection(positionComposerB, 1.5f));
        }
        
    }
    #endregion

    #region SetDeadZoneY
    private void SetDeadZoneY(CinemachinePositionComposer composer, float deadZoneY)
    {
        if(composer == null)
        {
            return;
        }
        var composition = composer.Composition;
        var deadZoneSettings = composition.DeadZone;
        deadZoneSettings.Size = new Vector2(deadZoneSettings.Size.x, deadZoneY);
        composition.DeadZone = deadZoneSettings;
        composer.Composition = composition;
    }
    #endregion
   
    #region Corutin
    ///<summary>
    /// 1페이즈 시작 시 카메라 Y축 위치를 부드럽게 보정하기 위한 코루틴
    /// </summary>
    private IEnumerator CoInitialYCorrection(CinemachinePositionComposer composer, float duration)
    {
        SetDeadZoneY(composer, 0.2f);
        yield return new WaitForSeconds(duration);
        SetDeadZoneY(composer, 1.0f);
    }
    #endregion
}
