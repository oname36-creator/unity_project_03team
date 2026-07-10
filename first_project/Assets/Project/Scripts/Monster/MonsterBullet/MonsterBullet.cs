using System.Collections;
using UnityEngine;


public class MonsterBullet : MonoBehaviour
{
    private Vector2 _startPos;
    private Vector2 _moveDir;
    private float _speed;

    private Coroutine _moveCoroutine;

    public void Launch(Vector2 startPosition, Vector2 direction, float speed)
    {
        _startPos = startPosition;
        _moveDir = direction.normalized; 
        _speed = speed;

        transform.position = _startPos;

        _moveCoroutine = StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        // 화면 밖으로 무한히 날아가는 것을 방지하기 위해 최대 5초 뒤 자동 소멸 
        float liveTime = 5f;
        float timer = 0f;

        while (timer < liveTime)
        {
            transform.Translate(_moveDir * _speed * Time.deltaTime, Space.World);

            timer += Time.deltaTime;
            yield return null;
        }


        ObjectPoolManager.Instance.MonsterBulletPush(this.gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 발사한 몬스터 자신이나 다른 총알에 부딪히는 예외 처리
        if (collision.CompareTag("Monster") || collision.CompareTag("MonsterBullet"))
        {
            return;
        }

        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }

        ObjectPoolManager.Instance.MonsterBulletPush(this.gameObject);
    }
}