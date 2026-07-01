using System;
using UnityEngine;

public enum ItemEffectType
{
    None,
    HealHP,
    Stealth,    // 은신
    Invincible  // 무적
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public int itemNumber;
    public string itemName;

    [Header("Effect Settings")]
    public ItemEffectType effectType; // 어떤 효과인가?
    public float effectValue;         // 효과 수치 (회복량 등)
    public float duration;            // 지속 시간 (은신 시간 등)
}