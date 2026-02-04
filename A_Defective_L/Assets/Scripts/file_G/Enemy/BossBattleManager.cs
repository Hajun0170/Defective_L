using System.Collections;
using UnityEngine;

public class BossBattleManager : MonoBehaviour
{
    [Header("보스 식별자")]
    public string bossID = "Stage1_Boss";

    [Header("1. 연결할 오브젝트")]
    // ★ GameObject가 아니라 스크립트를 직접 연결합니다.
    public BossController bossScript; 
    public Transform entryDoor;    
    public Transform exitDoor;     
    public GameObject abilityPanel; // 클리어 보상 UI
    public GameObject deathEffect;

    [Header("2. 설정값")]
    public float doorMoveDistance = 3.0f;
    public float doorMoveSpeed = 2.0f;
    public float bossCameraSize = 8f; 
    
    // 내부 변수
    private Vector3 entryClosedPos, entryOpenPos;
    private Vector3 exitClosedPos, exitOpenPos;
    private float defaultCameraSize;
    private bool isBattleStarted = false;

    [Header("★ 보상 설정")]
    public string unlockAbilityName = "Sprint";

    // ★ [추가] 이펙트 위치 미세 조정용 변수 (X, Y)
    public Vector2 deathEffectOffset = new Vector2(0, 1.0f);

    // ★ [추가] 보상 패널이 떠있는지 확인하는 플래그
    private bool isRewardActive = false;

    void Start()
    {
        // 문 좌표 계산 (생략 가능하나 안전하게 초기화)
        if (entryDoor != null) {
            entryClosedPos = entryDoor.position;
            entryOpenPos = entryClosedPos + Vector3.up * doorMoveDistance;
            entryDoor.position = entryOpenPos; // 시작할 땐 열려있음
        }
        if (exitDoor != null) {
            exitClosedPos = exitDoor.position;
            exitOpenPos = exitClosedPos + Vector3.up * doorMoveDistance;
            exitDoor.position = exitClosedPos; // 출구는 닫혀있음
        }

        // 보스 처치 여부 확인
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

    // ★ [추가] Update에서 키 입력을 감지합니다.
    void Update()
    {
        // 보상 패널이 켜져 있고(isRewardActive), 아무 키나 눌렀다면?
        if (isRewardActive && Input.anyKeyDown)
        {
            CloseAbilityPanel();
        }
    }
    // 이미 깬 보스면 정리
    void CleanupBossRoom()
    {
        if (bossScript != null) bossScript.gameObject.SetActive(false);
        if (entryDoor != null) entryDoor.position = entryOpenPos;
        if (exitDoor != null) exitDoor.position = exitOpenPos;
    }

    // ★ [핵심] 플레이어가 입장하면 전투 시작
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

        // 1. 문 닫기
        yield return StartCoroutine(MoveDoor(entryDoor, entryOpenPos, entryClosedPos));

        // 2. 카메라 줌 아웃 (시야 넓게)
        StartCoroutine(ChangeCameraSize(bossCameraSize));

        // 3. ★ [중요] 보스에게 "등장 연출 시작해!" 명령
        if (bossScript != null)
        {
            // 보스 스크립트 안의 Intro 함수를 실행
            yield return StartCoroutine(bossScript.StartBossIntro());
        }
    }

    // 보스가 죽었을 때 (BossController가 호출)
    public void OnBossDefeated()
    {
        DataManager.Instance.RegisterBossKill(bossID);
        UnlockAbility();
        StartCoroutine(VictorySequence());
    }

    void UnlockAbility()
    {
        if (unlockAbilityName == "Sprint") DataManager.Instance.currentData.hasSprint = true;
        else if (unlockAbilityName == "WallCling") DataManager.Instance.currentData.hasWallCling = true;
    }

    IEnumerator VictorySequence()
    {
       // yield return new WaitForSeconds(1.0f);

        // ★ [수정] 이펙트 위치 보정 (Offset 적용)
        if (deathEffect != null && bossScript != null)
        {
            // 보스 위치 + 오프셋 값
            Vector3 spawnPos = bossScript.transform.position + (Vector3)deathEffectOffset;
            Instantiate(deathEffect, spawnPos, Quaternion.identity);
        }

        // ★ [핵심 수정] 이펙트가 생성되고 퍼질 시간을 아주 조금 줍니다.
        // 이 시간이 없으면 생성되자마자 시간이 멈춰서 이펙트가 안 보입니다.
        yield return new WaitForSeconds(1.5f);

        // 보스 끄기
        if (bossScript != null) bossScript.gameObject.SetActive(false);

        // 보상 UI 표시
        if (abilityPanel != null)
        {

            // ★ [추가] 키 입력 대기 상태 활성화
            isRewardActive = true;

            abilityPanel.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            CloseAbilityPanel();
        }
    }

    public void CloseAbilityPanel()
    {
        // ★ [추가] 중복 실행 방지
        if (!isRewardActive) return; 
        isRewardActive = false;

        Time.timeScale = 1;
        if (abilityPanel != null) abilityPanel.SetActive(false);
        StartCoroutine(EndBattleSequence());
    }

    IEnumerator EndBattleSequence()
    {
        // 출구 열기
        yield return StartCoroutine(MoveDoor(exitDoor, exitClosedPos, exitOpenPos));
        // 카메라 원상복구
        StartCoroutine(ChangeCameraSize(defaultCameraSize));
    }

    // --- 유틸리티 (문 이동, 카메라 줌) ---
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