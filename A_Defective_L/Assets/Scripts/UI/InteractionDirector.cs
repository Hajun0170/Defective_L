using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractionDirector : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private KeyCode interactKey = KeyCode.F; // 상호작용 키
    [SerializeField] private KeyCode skipKey = KeyCode.Z;     // 대화 넘기기 키
    [SerializeField] private float defaultDuration = 2.5f;

    [Header("대사 순서 (ID 입력)")]
    [SerializeField] private string[] dialogueSequence; // 예: NPC_01, NPC_02...

    // [중요] CutsceneDirector와 똑같은 구조체 사용
    [System.Serializable]
    public struct ActorMapping
    {
        public string csvName;           // CSV 이름 (예: Robot)
        public Transform targetObj;      // 말풍선 띄울 위치 (DialoguePoint)
        public DialogueBubble myBubble;  // 사용할 말풍선 UI
    }

    [Header("배우 연결")]
    [SerializeField] private List<ActorMapping> actors;

    private bool isPlayerNearby = false;
    private bool isPlaying = false; // 현재 대화 중인지 확인

    private void Update()
    {
        // 플레이어가 근처에 있고 + 대화 중이 아니고 + F키를 눌렀을 때
        if (isPlayerNearby && !isPlaying && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(PlaySequence());
        }
    }

    private IEnumerator PlaySequence()
    {
        isPlaying = true; // 중복 실행 방지

        // 1. 조작 차단 (GameManager)
        if (GameManager.Instance != null) GameManager.Instance.StartCutscene();

        // 2. 대사 순차 재생 (CutsceneDirector와 동일한 로직)
        foreach (string id in dialogueSequence)
        {
            DialogueEntry data = DialogueManager.Instance.GetDialogue(id);

            if (data != null)
            {
                // 화자 찾기
                ActorMapping actor = GetActor(data.Speaker);

                // 말풍선 띄우기
                if (actor.myBubble != null && actor.targetObj != null)
                {
                    actor.myBubble.Show(data.Text, actor.targetObj, defaultDuration);
                }
            }

            // [스킵 로직] 시간 대기 or 키 입력 대기
            float timer = 0f;
            float waitTime = defaultDuration + 0.5f;

            while (timer < waitTime)
            {
                timer += Time.deltaTime;
                
                // Z키 누르면 즉시 다음 대사로 (GetKeyDown 사용!)
                if (Input.GetKeyDown(skipKey)) 
                {
                    break; 
                }
                yield return null;
            }
            yield return null; // 프레임 씹힘 방지
        }

        // 3. 종료 처리
        // 켜져 있는 모든 말풍선 끄기
        foreach (var actor in actors)
        {
            if (actor.myBubble != null) actor.myBubble.Hide();
        }

        // 조작 해제
        if (GameManager.Instance != null) GameManager.Instance.EndCutscene();
        
        isPlaying = false; // 다시 말 걸 수 있게 상태 변경
        Debug.Log("상호작용 대화 종료");
    }

    // 이름으로 배우 정보 찾기
    private ActorMapping GetActor(string name)
    {
        foreach (var actor in actors)
        {
            if (actor.csvName == name) return actor;
        }
        return new ActorMapping();
    }

    // 플레이어 감지 (Trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNearby = false;
    }
}