using UnityEngine;
using System.Collections.Generic;
using System;

public class BoxObject : MonoBehaviour
{
    private SOBoxSpawnSetting _spawnSetting;
    private Collider2D _triggerCollider;
    private bool _isTriggered = false;

    private void Awake()
    {
        _triggerCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        _isTriggered = false;
        if(_triggerCollider)
        {
            _triggerCollider.enabled = true;
        }
    }
    // 스폰 지점에 보상 아이템 목록을 주입받음
    public void SetSpawnSetting(SOBoxSpawnSetting setting)
    {
        _spawnSetting = setting;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 디버깅용 로그: 어떤 오브젝트와 충돌했는지 콘솔에 출력합니다.
        Debug.Log($"[BoxObject] 충돌 감지됨: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        // 중복 실행 방지를 위해 리턴
        if (_isTriggered) return;
        if(collision.CompareTag("Player"))
        {
            // 플레이어 접촉 즉시 콜라이더를 비활성화
            _isTriggered = true;
            if(_triggerCollider != null)
            {
                _triggerCollider.enabled = false;
            }
            Debug.Log("[BoxObject] 플레이어 충돌 확인 -> 상자 열기 실행");
            OpenBox();
        }
    }

    private void OpenBox()
    {
        #region AddItem
        if(_spawnSetting != null && _spawnSetting.rewardItemsWeights != null &&
            _spawnSetting.rewardItemsWeights.Count > 0)
        {
            // 현재 맵 페이즈 가져오기
            int currentPhase = 0;
            if(MapManager.Instance != null)
            {
                currentPhase = MapManager.Instance.CurrentLogicalPhase;
            }

            // 현재 페이즈의 가중치 총합 계산
            int totalWeight = 0;
           List<ItemWeightData> validItems = _spawnSetting.rewardItemsWeights;

            foreach(var data in validItems)
            {
                if (data.item == null) continue;
                totalWeight += GetWeightForPhase(data, currentPhase);
            }

            if(totalWeight > 0)
            {
                // 0 ~ (totalWeight - 1) 사이의 무작위 값 추출
                int randomValue = UnityEngine.Random.Range(0, totalWeight);
                int accmulatedWeight = 0;
                ItemData selectedItem = null;

                // 가중치 누적 합 기반으로 아이템 선택
                foreach(var data in validItems)
                {
                    if (data.item == null) continue;
                    int weight = GetWeightForPhase(data, currentPhase);
                    accmulatedWeight += weight;

                    if(randomValue < accmulatedWeight)
                    {
                        selectedItem = data.item;
                        break;
                    }
                }

                if(selectedItem != null)
                {
                    // DataManager에 아이템 추가
                    DataManager.Instance.AddItem(selectedItem.itemNumber);
                    Debug.Log("[BoxObject] : 아이템 획득!");
                }
            }
            else
            {
                Debug.LogWarning("[BoxObject] : 아이템의 가중치 합이 0이다.");
            }
        }
        #endregion

        else 
        {
            Debug.LogWarning("보상 아이템 목록이 없음");
        }

        // 획득 후 오브젝트 풀로 반환
        ObjectPoolManager.Instance.BoxPush(gameObject);
    }

    private int GetWeightForPhase(ItemWeightData data, int phase)
    {
        switch(phase)
        {
            case 0: return data.phaseOWeight;
            case 1: return data.phase1Weight;
            case 2: return data.phase2Weight;
            default: return data.phase2Weight;
        }
    }
}
