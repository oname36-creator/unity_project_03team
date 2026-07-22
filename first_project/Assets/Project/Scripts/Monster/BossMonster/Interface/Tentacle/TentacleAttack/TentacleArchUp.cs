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

        // _owner.tag = "Boss"; // GC 방지를 위해 태그 할당 제거 (레이어 사용 권장)
        _owner.SetLayer(true);



        _owner.segmentDistance = 0.5f;
        
        // 포물선 제어 모드 활성화 (isArch 유지)
        _owner.isArch = true;
        _owner.isParabola = true;
        _owner.parabolaA = 0.03f; // 완만한 곡선
        _owner.parabolaAngle = 30f; // 약간 뒤로 젖힌 상태에서 시작
        
        _owner.Target = null;
        _owner.Attack = false;

        _timer = 0f;


        Vector3 viewportTopRight = _camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        Vector2 rayStart = new Vector2(viewportTopRight.x, viewportTopRight.y + 5f);
        
        int groundLayer = LayerMask.GetMask("Ground");
        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 100f, groundLayer);
        Vector2 targetPos = _owner.Boss.Player.transform.position; // 기본값
        
        if (hit.collider != null)
        {
            targetPos = hit.point + new Vector2(0, 2f);
        }

        _owner.UpdateSegmentLength(20);

        float dist = Vector2.Distance(_owner.tentacleRoot.position, targetPos);
        _owner.segmentDistance = (dist * 1.3f) / 20f;

        if (_warningEffect != null)
        {

            Vector2 effectPos = (targetPos + (Vector2)_owner.tentacleRoot.position) / 2f;
            effectPos.x += 10;
            _warningEffect.transform.position = effectPos;
            
            float spriteWidth = _warningEffect.sprite.bounds.size.x;
            if (spriteWidth > 0f)
            {
                Vector3 currentScale = _warningEffect.transform.localScale;
                currentScale.x = (dist)/ spriteWidth;
                _warningEffect.transform.localScale = currentScale;
            }

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
        float invT = 1f - t;
        float easeOutT = 1f - (invT * invT * invT); 
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