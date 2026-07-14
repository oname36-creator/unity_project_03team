using UnityEngine;
using Unity.Cinemachine;
using Unity.VisualScripting;
using System.Collections;
public class CameraConfinerManager : Singleton<CameraConfinerManager>
{
    #region Class Attribute
    [Header("시네머신 카메라 설정")]
    [SerializeField] private CinemachineCamera virtualCameraA;
    [SerializeField] private CinemachineCamera virtualCameraB;

    private CinemachineConfiner2D confinerA;
    private CinemachineConfiner2D confinerB;

    private CinemachinePositionComposer positionComposerA;
    private CinemachinePositionComposer positionComposerB;

    private Coroutine _yTrackingCoroutineA;  // 처음 y축 보정 제어용 변수
    private Coroutine _yTrackingCoroutineB;

    // 현재 카메라를 화면 출력용 메인으로 쓰고 있는지 기록(NextCamera와 교차 활성화하기 위함)
    private bool _isUsingCameraA = true;

    // 현재 설정된 바운더리 콜라이더를 캐싱하여 중복 처리를 방지합니다.
    private Collider2D _currentBoundary;
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
    }
    #endregion

    #region UpdateBoundary
    /// <summary>
    ///  카메라 제한 영역 콜라이더 변경
    /// </summary>
    public void UpdateBoundary(Collider2D newBoundary)
    {
        if(confinerA == null || confinerB == null)
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

        Debug.Log($"[UpdateBoundary] 호출됨. 새로운 바운더리: {newBoundary.gameObject.name}, 현재 사용중인 카메라: {(_isUsingCameraA ? "Camera A" : "Camera B")}");

        if(_isUsingCameraA)
        {
            if(confinerB != null && virtualCameraB != null && virtualCameraA != null)
            {
                // 대기 중인 카메라의 바운더리만 업데이트
                confinerB.BoundingShape2D = newBoundary;
                confinerB.InvalidateBoundingShapeCache();

                // 우선순위를 전환하여 CinemachineBrain이 두 가상 카메라 간의 Blending을 수행하도록 유도
                virtualCameraA.Priority = 10;
                virtualCameraB.Priority = 15;

                _isUsingCameraA = false;
                Debug.Log($"[UpdateBoundary] Camera A -> Camera B로 전환 시도. A Priority: {virtualCameraA.Priority}, B Priority: {virtualCameraB.Priority}");
            }
            else
            {
                Debug.LogError($"[UpdateBoundary] Camera A -> B 전환 실패. confinerB: {confinerB}, virtualCameraB: {virtualCameraB}, virtualCameraA: {virtualCameraA}");
            }
        }
        else
        {
            if(confinerA != null && virtualCameraA != null && virtualCameraB != null)
            {
                // 대기 중인 A 카메라의 바운더리만 업데이트
                confinerA.BoundingShape2D = newBoundary;
                confinerA.InvalidateBoundingShapeCache();

                // 우선순위를 전환하여 A로 Blending
                virtualCameraA.Priority = 15;
                virtualCameraB.Priority = 10;

                _isUsingCameraA = true;
                Debug.Log($"[UpdateBoundary] Camera B -> Camera A로 전환 시도. A Priority: {virtualCameraA.Priority}, B Priority: {virtualCameraB.Priority}");
            }
            else
            {
                Debug.LogError($"[UpdateBoundary] Camera B -> A 전환 실패. confinerA: {confinerA}, virtualCameraA: {virtualCameraA}, virtualCameraB: {virtualCameraB}");
            }
        }
    }
    #endregion

    #region SetCameraYTrackingByPhase
    ///<summary>
    ///페이즈 번호에 따라 카메라의 Y축 고정 여부를 결정합니다.
    /// </summary>
    public void SetCameraYTrackingByPhase(int phaseindex)
    {
        if (positionComposerA == null)
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

        if(phaseindex == 0)
        {
            if(positionComposerA != null)
            {
                _yTrackingCoroutineA = StartCoroutine(CoInitialYCorrection(positionComposerA, 1.5f));
            }
            if (positionComposerB != null)
            {
                _yTrackingCoroutineB = StartCoroutine(CoInitialYCorrection(positionComposerB, 1.5f));
            }
        }
        else
        {
            SetDeadZoneY(positionComposerA, 0.2f);
            SetDeadZoneY(positionComposerB, 0.2f);
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

    #region CoInitialYCorrection
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
