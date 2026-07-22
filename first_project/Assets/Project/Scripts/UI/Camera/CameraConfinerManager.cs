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

    [Header("수직 전용")]
    [SerializeField] private CinemachineCamera virtualCameraC;
    [SerializeField] private CinemachineCamera virtualCameraD;

    private CinemachineConfiner2D confinerA;
    private CinemachineConfiner2D confinerB;
    private CinemachineConfiner2D confinerC;
    private CinemachineConfiner2D confinerD;

    private CinemachinePositionComposer positionComposerA;
    private CinemachinePositionComposer positionComposerB;
    private CinemachinePositionComposer positionComposerC;
    private CinemachinePositionComposer positionComposerD;

    private Coroutine _yTrackingCoroutineA;  // 처음 y축 보정 제어용 변수
    private Coroutine _yTrackingCoroutineB;

    // 현재 카메라를 화면 출력용 메인으로 쓰고 있는지 기록(NextCamera와 교차 활성화하기 위함)
    private bool _isUsingCameraA = true;
    private bool _isUsingCameraC = true;

    // 직전에 사용하던 전체 카메라 유형 기록
    private bool _wasUsingYCameraGroup = false;

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
        if(virtualCameraA != null)
        {
            confinerA = virtualCameraA.GetComponent<CinemachineConfiner2D>();
            positionComposerA = virtualCameraA.GetComponent<CinemachinePositionComposer>();
        }

        if(virtualCameraB != null)
        {
            confinerB = virtualCameraB.GetComponent<CinemachineConfiner2D>();
            positionComposerB = virtualCameraB.GetComponent<CinemachinePositionComposer>();
        }
        if (virtualCameraC != null)
        {
            confinerC = virtualCameraC.GetComponent<CinemachineConfiner2D>();
            positionComposerC = virtualCameraC.GetComponent<CinemachinePositionComposer>();
        }

        if (virtualCameraD != null)
        {
            confinerD = virtualCameraD.GetComponent<CinemachineConfiner2D>();
            positionComposerD = virtualCameraD.GetComponent<CinemachinePositionComposer>();
        }
    }
    #endregion

    #region UpdateBoundary
    /// <summary>
    ///  카메라 제한 영역 콜라이더 변경
    /// </summary>
    public void UpdateBoundary(Collider2D newBoundary, bool enableYTracking)
    {
        if(confinerA == null || confinerB == null || confinerC == null || confinerD == null)
        {
            InitCameraReferences();
        }

        if(newBoundary == null)
        {
            Debug.LogWarning("전달된 새로운 카메라 바운더리 콜라이더가 null입니다.");
            return;
        }

        // 중복 호출 방지: 이미 적용된 바운더리와 같다면 처리를 무시합니다.
        if (newBoundary == _currentBoundary)
        {
            return;
        }

        _currentBoundary = newBoundary;

        IsYTrackingActive = enableYTracking;

        Debug.Log($"[UpdateBoundary] 호출됨. 새로운 바운더리: {newBoundary.gameObject.name}, 현재 사용중인 카메라: {(_isUsingCameraA ? "Camera A" : "Camera B")}");

        if(enableYTracking)
        {
            TransitionToYCameraGroup(newBoundary);
        }
        else
        {
            TransitionToNormalCameraGroup(newBoundary);
        }
        
        
    }


    #endregion

    #region ActivateCameraAndDeactivateOthers
    /// <summary>
    /// 대상 카메라의 우선수위를 높이고 바운더리를 할당하며, 나머지 모든 가상 카메라의
    /// 우선 순위를 낮춥니다.
    /// </summary>

    private void ActivateCameraAndDeactivateOthers(CinemachineCamera targetCamera, CinemachineConfiner2D targetConfiner, Collider2D boundary, float targetDeadZoneY, CinemachinePositionComposer composer)
    {
        if (targetCamera == null || targetConfiner == null) return;
        // 대기 중인 카메라의 바운더리만 업데이트
        targetConfiner.BoundingShape2D = boundary;
        targetConfiner.InvalidateBoundingShapeCache();

        // 전환되어 사용할 카메라 B의 Y축 데드존을 항상 영구 고정 상태로 지정
        if (composer != null)
        {
            SetDeadZoneY(composer, targetDeadZoneY);
        }

        // 우선순위를 전환하여 CinemachineBrain이 두 가상 카메라 간의 Blending을 수행하도록 유도
        targetCamera.Priority = 15;

        // 우선순위가 낮은 카메라의 바운더리를 해제
        if (virtualCameraA != null && virtualCameraA != targetCamera)
        {
            virtualCameraA.Priority = 15;
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
        if (virtualCameraC != null && virtualCameraC != targetCamera)
        {
            virtualCameraC.Priority = 10;
            if (confinerC != null)
            {
                confinerC.BoundingShape2D = null;
                confinerC.InvalidateBoundingShapeCache();
            }
        }
        if (virtualCameraD != null && virtualCameraD != targetCamera)
        {
            virtualCameraD.Priority = 10;
            if (confinerD != null)
            {
                confinerD.BoundingShape2D = null;
                confinerD.InvalidateBoundingShapeCache();
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

    #region Camera A,B
    /// <summary>
    /// 일반 수평 카메라 그룹(A, B)으로 전환하거나 내부에서 블렌딩합니다.
    /// </summary>
    private void TransitionToNormalCameraGroup(Collider2D newBoundary)
    {
        if (_isUsingCameraA)
        {
            // A 카메라 활성화
            ActivateCameraAndDeactivateOthers(virtualCameraA, confinerA, newBoundary, 0.8f, positionComposerA);
            _isUsingCameraA = false;
        }
        else
        {
            // A 카메라 활성화
            ActivateCameraAndDeactivateOthers(virtualCameraB, confinerB, newBoundary, 0.8f, positionComposerB);
            _isUsingCameraA = true;
        }
        _wasUsingYCameraGroup = false;
    }
    #endregion

    #region Camera C,D
    private void TransitionToYCameraGroup(Collider2D newBoundary)
    {
        if(_isUsingCameraC)
        {
            ActivateCameraAndDeactivateOthers(virtualCameraC, confinerC, newBoundary, 0.2f, positionComposerC);
            _isUsingCameraC = false;
        }
        else
        {
            ActivateCameraAndDeactivateOthers(virtualCameraD, confinerD, newBoundary, 0.2f, positionComposerD);
            _isUsingCameraC = true;
        }
        _wasUsingYCameraGroup = true;
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
