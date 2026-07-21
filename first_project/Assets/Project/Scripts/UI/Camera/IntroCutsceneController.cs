using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using Unity.VisualScripting;
using System;
public class IntroCutsceneController : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera virtualCameraC;
    [SerializeField] private float cutsceneDuration = 3f;

    [Header("Targets (Auto Search if empty)")]
    [SerializeField] private PlayerControll playerController;
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private BossController bossController;

    private void Awake()
    {
        #region Excepting Hiding
        if(playerController == null)
        {
            FindAnyObjectByType<PlayerControll>();
        }
        if (playerStatus == null)
        {
            FindAnyObjectByType<PlayerStatus>();
        }
        if(bossController == null)
        {
            FindAnyObjectByType<BossController>();
        }
        #endregion
    }

    private void OnEnable()
    {
        MapManager.OnMapReady += StartIntroCutscene;
    }
    private void OnDisable()
    {
        MapManager.OnMapReady -= StartIntroCutscene;
    }

    private void StartIntroCutscene()
    {
        StartCoroutine(CoPlayIntroCutscene());
    }

    private IEnumerator CoPlayIntroCutscene()
    {
        if(virtualCameraC != null)
        {
            virtualCameraC.Priority = 20;
        }

        // 인트로 중 플레이어 이동 차단 및 무적 부여
        if(playerController != null)
        {
            playerController.SetInputActive(false);
        }
        if(playerStatus != null)
        {
            playerStatus.isInvincible = true;
        }

        //  보스 인트로 감속 부여
        if(bossController != null)
        {
            bossController.isIntro = true;
        }

        // 3초간 몬스터가 기어오는 모습 비추기
        yield return new WaitForSeconds(cutsceneDuration);

        if(virtualCameraC != null)
        {
            virtualCameraC.Priority = 0;
        }

        if (playerController != null)
        {
            playerController.SetInputActive(true);
        }
        if (playerStatus != null)
        {
            playerStatus.isInvincible = false;
        }
        if (bossController != null)
        {
            bossController.isIntro = false;
        }

    }
}
