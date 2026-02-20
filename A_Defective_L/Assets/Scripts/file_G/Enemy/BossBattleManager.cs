using System.Collections;
using UnityEngine;

public class BossBattleManager : MonoBehaviour
{
    [Header("보스 식별 코드")]
    public string bossID = "Stage1_Boss";

    [Header("연결할 오브젝트")]
    public BossController bossScript; 
    public Transform entryDoor;    
    public Transform exitDoor;     
    public GameObject abilityPanel; // 클리어 보상 UI (스킬 해금용)
    public GameObject deathEffect;

    [Header("무기 보상")]
    public Weapon dropWeapon; // 보스가 드랍할 무기 데이터 (없으면 지급 X)

    [Header("설정값")]
    public float doorMoveDistance = 3.0f;
    public float doorMoveSpeed = 2.0f;
    public float bossCameraSize = 8f; 
    
    // 내부 변수
    private Vector3 entryClosedPos, entryOpenPos;
    private Vector3 exitClosedPos, exitOpenPos;
    private float defaultCameraSize;
    private bool isBattleStarted = false;

    [Header("스킬 해금 설정")]
    public string unlockAbilityName = "Sprint"; //질주 능력

    public Vector2 deathEffectOffset = new Vector2(0, 1.0f);
    private bool isRewardActive = false;

    void Start()
    {
        if (entryDoor != null) {
            entryClosedPos = entryDoor.position;
            entryOpenPos = entryClosedPos + Vector3.up * doorMoveDistance;
            entryDoor.position = entryOpenPos; 
        }
        if (exitDoor != null) {
            exitClosedPos = exitDoor.position;
            exitOpenPos = exitClosedPos + Vector3.up * doorMoveDistance;
            exitDoor.position = exitClosedPos; 
        }

        if (DataManager.Instance.IsBossDefeated(bossID))
        {
            CleanupBossRoom();
        }
        else
        {
            if (Camera.main != null) defaultCameraSize = Camera.main.orthographicSize;
            if (abilityPanel != null) abilityPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (isRewardActive && Input.anyKeyDown)
        {
            CloseAbilityPanel();
        }
    }

    void CleanupBossRoom()
    {
        if (bossScript != null) bossScript.gameObject.SetActive(false);
        if (entryDoor != null) entryDoor.position = entryOpenPos;
        if (exitDoor != null) exitDoor.position = exitOpenPos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBattleStarted || DataManager.Instance.IsBossDefeated(bossID)) return;

        if (collision.CompareTag("Player"))
        {
            StartCoroutine(StartBattleSequence());
        }
    }

    IEnumerator StartBattleSequence()
    {
        isBattleStarted = true;
        Debug.Log("🚩 보스전 트리거 발동!");

        yield return StartCoroutine(MoveDoor(entryDoor, entryOpenPos, entryClosedPos));
        StartCoroutine(ChangeCameraSize(bossCameraSize));

        if (bossScript != null)
        {
            yield return StartCoroutine(bossScript.StartBossIntro());
        }
    }

    // 보스가 죽었을 때 BossController가 호출
    public void OnBossDefeated()
    {
        DataManager.Instance.RegisterBossKill(bossID);
        
        // 스킬 해금 (벽타기, 대시 등)
        UnlockAbility();

        // 무기 보상 지급 - GameManager에게 요청
        if (dropWeapon != null && GameManager.Instance != null)
        {
            GameManager.Instance.GetWeaponReward(dropWeapon);
            Debug.Log($"보스 처치 보상: {dropWeapon.weaponName}");
        }

        StartCoroutine(VictorySequence());
    }

    void UnlockAbility()
    {
        if (unlockAbilityName == "Sprint") DataManager.Instance.currentData.hasSprint = true;
        else if (unlockAbilityName == "WallCling") DataManager.Instance.currentData.hasWallCling = true;
    }

    IEnumerator VictorySequence()
    {
        if (deathEffect != null && bossScript != null)
        {
            Vector3 spawnPos = bossScript.transform.position + (Vector3)deathEffectOffset;
            Instantiate(deathEffect, spawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(1.5f);

        if (bossScript != null) bossScript.gameObject.SetActive(false);

        // 스킬 해금 UI 표시
        if (abilityPanel != null)
        {

            abilityPanel.SetActive(true);

            // 입력 잠금
            isRewardActive = false;

            Time.timeScale = 0;

            // 1초 대기: imeScale이 0이므로 Realtime 사용이 필수적임
            yield return new WaitForSecondsRealtime(1.0f);

            // 재입력 받아짐
            isRewardActive = true;
        }
        else
        {
            // 스킬 UI가 없으면 바로 문 염
            StartCoroutine(EndBattleSequence());
        }
    }

    public void CloseAbilityPanel()
    {
        if (!isRewardActive && abilityPanel != null && abilityPanel.activeSelf) 
        {
             // 혹시 켜져 있으면 끄기
        }
        else if (!isRewardActive) return;

        isRewardActive = false;
        Time.timeScale = 1;
        
        if (abilityPanel != null) abilityPanel.SetActive(false);
        StartCoroutine(EndBattleSequence());
    }

    IEnumerator EndBattleSequence()
    {
        yield return StartCoroutine(MoveDoor(exitDoor, exitClosedPos, exitOpenPos));
        StartCoroutine(ChangeCameraSize(defaultCameraSize));
    }

    IEnumerator MoveDoor(Transform door, Vector3 startPos, Vector3 endPos)
    {
        float elapsed = 0f;
        while (elapsed < 1.0f)
        {
            elapsed += Time.deltaTime * doorMoveSpeed;
            door.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, elapsed));
            yield return null;
        }
        door.position = endPos;
    }

    IEnumerator ChangeCameraSize(float targetSize)
    {
        if (Camera.main == null) yield break;
        float startSize = Camera.main.orthographicSize;
        float duration = 1.0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Camera.main.orthographicSize = Mathf.Lerp(startSize, targetSize, Mathf.SmoothStep(0, 1, elapsed / duration));
            yield return null;
        }
        Camera.main.orthographicSize = targetSize;
    }
}