using UnityEngine;

public class IMonster : MonoBehaviour
{
 

    [Header("Interface")] // 인스펙터에 제목 표시
    [SerializeField] public MonsterAttack _monsterAttack;
    [SerializeField] public MonsterSearch _monsterSearch;
    [SerializeField] public MonsterChase _monsterChase;



    private Animator _mAnimator;
    private MonsterController _monsterController;


    // 애니메이션 해시 값을 저장할 변수
    private int _hurtHash;
    private int _deadHash;

    private void Start()
    {

        _hurtHash = Animator.StringToHash("beAttacked");
        _deadHash = Animator.StringToHash("isDead");


        _mAnimator = GetComponent<Animator>();
        _monsterController = GetComponent<MonsterController>();

        // animator 할당 해주기
        _monsterAttack.GetComponent(_mAnimator, _monsterController);
        _monsterSearch.GetComponent(_mAnimator, _monsterController);
        _monsterChase.GetComponent(_mAnimator, _monsterController);
    }

    public void Attack() 
    {
        _monsterAttack.Attack();
    }
    public void Search()
    {
        _monsterSearch.Search();
    }
    public void Chase() 
    {
        _monsterChase.Chase();
    }

    public void Hurt() 
    {
        // 피격 애니메이션 재생
        _mAnimator.SetTrigger(_hurtHash);
    }
    public void Dead()
    {
        // 죽음 애니메이션 재생
        _mAnimator.SetBool(_deadHash, true);
    }


}
