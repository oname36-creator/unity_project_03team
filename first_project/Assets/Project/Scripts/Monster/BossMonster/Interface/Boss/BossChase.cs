using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class BossChase : IMonsterState
{
    private BossController _owner;

    private Transform _ownerTransform;
    private Transform _playerTransform;

    private MonsterRespawn _monsterRespawn;

    private Camera _camera;

    private Coroutine _chaseCoroutine;
    private Coroutine _phaseCoroutine;


    public GameObject _distanceUi;
    public RectTransform _bossDistanceTransform;
    public TextMeshProUGUI _bossDistanceText;
    public Canvas _canvas; // 타겟 UI가 포함된 Canvas

    private float _time;

    private float _CameraWidth;
    private float _orthoSize;


    // 생성자에서 owner를 직접 받도록 셋업
    public BossChase(BossController owner)
    {
        this._owner = owner;
        _ownerTransform = _owner.GetComponent<Transform>();
        _monsterRespawn = _owner.MonsterRespawner.GetComponent<MonsterRespawn>();
        _playerTransform = _owner.Player.transform;
        _bossDistanceTransform = _owner.BossDistanceTransform;
        _bossDistanceText = _owner.BossDistanceText;

        _distanceUi = _bossDistanceTransform.gameObject;
        _canvas = _owner.Canvas;

        _camera = _owner.Camera;
        _orthoSize = _camera.orthographicSize;
        _CameraWidth = _orthoSize * _camera.aspect;
    }


    public void Enter()
    {
        _time = 0f;

        _owner.Attack = false;
        _owner.gameObject.tag = "Boss";
        _owner.gameObject.layer = LayerMask.NameToLayer("Boss");

        SoundManager.Instance.PlaySFX("BossSound");

        // 상태 진입 시 코루틴을 시작하도록 변경 (재진입 시 안전함)
        _chaseCoroutine = _owner.StartCoroutine(Chase());
        _phaseCoroutine = _owner.StartCoroutine(Phase());
    }

    public void Update()
    {
   
    }

    public void Exit()
    {
        if (_chaseCoroutine != null)
        {
            _owner.StopCoroutine(_chaseCoroutine);
            _chaseCoroutine = null; // 참조 초기화
        }
        if (_phaseCoroutine != null)
        {
            _owner.StopCoroutine(_phaseCoroutine);
            _phaseCoroutine = null;
        }
    }

    private float EffectPosition(float x)
    {
        float gradient = (_playerTransform.position.y - _ownerTransform.position.y) / (_playerTransform.position.x - _ownerTransform.position.x);
        return gradient * (x - _ownerTransform.position.x) + _ownerTransform.position.y;
    }


    IEnumerator Chase()
    {
        while (true)
        {
            if (_distanceUi.activeSelf)
            {
                float directionX = _playerTransform.position.x - _ownerTransform.position.x;
                _bossDistanceText.text = directionX.ToString("F2") + "m";

                Camera canvasCam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
                Vector2 uiScreenPos = RectTransformUtility.WorldToScreenPoint(canvasCam, _bossDistanceTransform.position);

                Vector3 uiWorldPoint = _camera.ScreenToWorldPoint(new Vector3(uiScreenPos.x, uiScreenPos.y, 0f));
                float targetWorldX = uiWorldPoint.x;

                float targetWorldY = EffectPosition(targetWorldX);

                Vector3 targetGlobalPos = new Vector3(targetWorldX, targetWorldY, _ownerTransform.position.z);
                Vector3 targetScreenPos = _camera.WorldToScreenPoint(targetGlobalPos);

                RectTransform parentRect = _bossDistanceTransform.parent as RectTransform;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, targetScreenPos, canvasCam, out Vector2 localPos))
                {
                    Vector2 currentAnchoredPos = _bossDistanceTransform.anchoredPosition;
                    _bossDistanceTransform.anchoredPosition = new Vector2(currentAnchoredPos.x, localPos.y);
                }
            }

            yield return null;
        }
    }

    IEnumerator Phase()
    {
        WaitForSeconds waitTime = new WaitForSeconds(61f);

        while (true)
        {
            yield return waitTime; 

            SoundManager.Instance.PlaySFX("BossScreech");
            if(_owner.Phase == 3) 
            {
                SoundManager.Instance.PlaySFX("BossSound");
            }


            _owner.MoveSpeed += 2.5f;

            ++_owner.Phase;
        }
    }
}