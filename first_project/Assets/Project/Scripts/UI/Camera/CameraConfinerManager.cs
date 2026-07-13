using UnityEngine;
using Unity.Cinemachine;
using Unity.VisualScripting;
using System.Collections;
public class CameraConfinerManager : Singleton<CameraConfinerManager>
{
    #region Class Attribute
    [Header("시네머신 카메라 설정")]
    [SerializeField] private CinemachineCamera virtualCamera;

    private CinemachineConfiner2D confiner;
    private CinemachinePositionComposer positionComposer;
    private Coroutine _yTrackingCoroutine;  // 처음 y축 보정 제어용 변수
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

            positionComposer = virtualCamera.GetComponent<CinemachinePositionComposer>();
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

    #region SetCameraYTrackingByPhase
    ///<summary>
    ///페이즈 번호에 따라 카메라의 Y축 고정 여부를 결정합니다.
    /// </summary>
    public void SetCameraYTrackingByPhase(int phaseindex)
    {
        if (positionComposer == null)
        {
            InitCameraReferences();
        }

        if(positionComposer != null)
        {
            // 진행 중이던 기존 보정 코루틴이 있다면 중지
            if(_yTrackingCoroutine != null)
            {
                StopCoroutine(_yTrackingCoroutine);
                _yTrackingCoroutine = null;
            }

            var composition = positionComposer.Composition;
            var deadZoneSettings = composition.DeadZone;
            if(phaseindex == 0)
            {
                _yTrackingCoroutine = StartCoroutine(CoInitialYCorrection(1.5f));
            }
            else
            {
                // 2,3페이즈
                deadZoneSettings.Size = new Vector2(deadZoneSettings.Size.x, 0.2f);
                composition.DeadZone = deadZoneSettings;
                positionComposer.Composition = composition;
            }
        }
    }
    #endregion

    #region CoInitialYCorrection
    ///<summary>
    /// 1페이즈 시작 시 카메라 Y축 위치를 부드럽게 보정하기 위한 코루틴
    /// </summary>
    private IEnumerator CoInitialYCorrection(float duration)
    {
        var composition = positionComposer.Composition;
        var deadZoneSettings = composition.DeadZone;

        // 처음 메인 씬에 진입 시 보정
        deadZoneSettings.Size = new Vector2(deadZoneSettings.Size.x, 0.2f);
        composition.DeadZone = deadZoneSettings;
        positionComposer.Composition = composition;

        // 카메라가 플레이어 Y축 위치를 충분히 정렬할 수 있도록 설정된 시간 대기
        yield return new WaitForSeconds(duration);

        // 정렬 후 Y축 고정상태로 변경
        composition = positionComposer.Composition;
        deadZoneSettings = composition.DeadZone;
        deadZoneSettings.Size = new Vector2(deadZoneSettings.Size.x, 1.0f);
        composition.DeadZone = deadZoneSettings;
        positionComposer.Composition = composition;

        _yTrackingCoroutine = null;
    }
    #endregion
}
