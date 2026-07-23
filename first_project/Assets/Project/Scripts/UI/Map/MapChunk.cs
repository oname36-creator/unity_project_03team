using UnityEngine;
using System.Collections.Generic;
using System;


public class MapChunk : MonoBehaviour
{
    #region DataAttribute
    [Header("시작/끝 앵커")]
    public Transform startPosition;
    public Transform endPosition;

    [Header("카메라 제한 영역")]
    public Collider2D cameraBoundaryCollider;

    [Header("플레이어 안전 스폰 거리")]
    [SerializeField] private float minPlayerDistance = 4f;

    [Header("진입 영역 마진")]
    public float margin = 2f;

    [Header("카메라 Y축 옵션")]
    public bool enableYTracking = false;

    private MapChunkSpawnController _spawnController;
    
    #endregion

    private void Awake()
    {
        _spawnController = GetComponent<MapChunkSpawnController>();

        if(_spawnController == null)
        {
            _spawnController = gameObject.AddComponent<MapChunkSpawnController>();
        }
    }

#region Event
    private void OnEnable()
    {

        if (_spawnController != null)
        {
            _spawnController.SpawnAll();
        }
    }
    private void OnDisable()
    {
        if(_spawnController != null)
        {
            _spawnController.RecycleAll();
        }
    }


    #endregion

}
