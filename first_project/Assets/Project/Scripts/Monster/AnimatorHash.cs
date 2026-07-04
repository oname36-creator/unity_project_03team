using UnityEngine;

public static class AnimatorHash
{
	public static readonly int Idle = Animator.StringToHash("idle");

	public static readonly int IsDead = Animator.StringToHash("isDead");
	
	public static readonly int IsChase = Animator.StringToHash("isChase");
	public static readonly int IsWork = Animator.StringToHash("isWork");
	public static readonly int IsAttack = Animator.StringToHash("isAttack");
	public static readonly int IsHurt = Animator.StringToHash("isHurt");

}
