using UnityEngine;
using System.Collections;

public class TentacleArchUp : IMonsterState
{
    private TentacleController _owner;
    private Camera _camera;
    private SpriteRenderer _warningEffect;

    private float _timer;
    private float _duration = 5f; // 기획과 동일한 5초

    public TentacleArchUp(TentacleController owner)
    {
        _owner = owner;
        _camera = owner.Boss.Camera;
        _warningEffect = owner.warningEffectRenderer_Arch;
    }

    public void Enter()
    {
        Debug.Log("TentacleArchUP");
        _owner.segmentDistance = 0.5f;
        
        // 포물선 제어 모드 활성화 (isArch 유지)
        _owner.isArch = true;
        _owner.isParabola = true;
        _owner.parabolaA = 0.03f; // 완만한 곡선
        _owner.parabolaAngle = 30f; // 약간 뒤로 젖힌 상태에서 시작
        
        _owner.GroundLimitY = null;
        _owner.Target = null;
        _owner.Attack = false;

        _timer = 0f;

        // 1. 카메라 가장 오른쪽 Ground 찾기
        Vector3 viewportTopRight = _camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        Vector2 rayStart = new Vector2(viewportTopRight.x, viewportTopRight.y + 5f);
        
        RaycastHit2D[] hits = Physics2D.RaycastAll(rayStart, Vector2.down, 100f);
        Vector2 targetPos = _owner.Boss.Player.transform.position; // 기본값
        
        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Ground"))
            {
                targetPos = hit.point + new Vector2(0, 2f);
                break;
            }
        }

        // 2. 촉수 길이 맞추기 (루트에서 타겟까지의 거리를 기반으로 여유분 1.3배)
        float dist = Vector2.Distance(_owner.tentacleRoot.position, targetPos);
        int requiredSegments = Mathf.CeilToInt((dist * 1.3f) / _owner.segmentDistance);
        _owner.UpdateSegmentLength(Mathf.Max(requiredSegments, 35));

        // 3. 경고 이펙트 위치 설정
        if (_warningEffect != null)
        {
            _warningEffect.transform.position = targetPos;
            
            _warningEffect.gameObject.SetActive(true);
            Color color = _warningEffect.color;
            color.a = 0f;
            _warningEffect.color = color;
        }
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        float t = Mathf.Clamp01(_timer / _duration);
        float easeOutT = 1f - Mathf.Pow(1f - t, 3f); // 45도(뒤로 많이 젖혀짐)에서 -20도(앞으로 더 기울어짐)로 연출
        _owner.parabolaAngle = Mathf.Lerp(45f, -20f, easeOutT);

        if (_warningEffect != null && _warningEffect.gameObject.activeSelf)
        {
            Color color = _warningEffect.color;
            color.a = Mathf.Lerp(0f, 0.75f, t);
            _warningEffect.color = color;
        }

        if (_timer >= _duration)
        {
            _owner.Attack = true; // 5초 후 ArchAttack으로 전이
        }
    }

    public void Exit()
    {
        if (_warningEffect != null)
        {
            _warningEffect.gameObject.SetActive(false);
        }
    }
}