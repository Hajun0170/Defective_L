using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CutsceneDirector2 : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float defaultDuration = 2.5f;
    [SerializeField] private KeyCode skipKey = KeyCode.Z;

    // [삭제] 전역 말풍선 변수는 이제 필요 없습니다.
    // [SerializeField] private DialogueBubble dialogueBubble; 

    [Header("대사 순서 (ID)")]
    [SerializeField] private string[] dialogueSequence;

    // [수정] 배우 정보에 '전용 말풍선' 칸을 추가합니다.
    [System.Serializable]
    public struct ActorMapping
    {
        public string csvName;           // CSV에 적힌 이름 (예: L)
        public Transform targetObj;      // 머리 위 위치 (DialoguePoint)
        public DialogueBubble myBubble;  // ★ 이 배우가 사용할 전용 말풍선 UI
    }

    [Header("배우 및 말풍선 연결")]
    [SerializeField] private List<ActorMapping> actors;

    private IEnumerator PlaySequence()
    {
        if (GameManager.Instance != null) GameManager.Instance.StartCutscene();

        foreach (string id in dialogueSequence)
        {
            DialogueEntry data = DialogueManager.Instance.GetDialogue(id);
            if (data == null) continue;

            // 1. 화자 이름으로 배우 정보(위치 + 말풍선)를 통째로 가져옴
            ActorMapping actor = GetActor(data.Speaker);

            // 배우 정보가 있고, 그 배우에게 연결된 말풍선이 있다면 출력
            if (actor.myBubble != null && actor.targetObj != null)
            {
                // ★ 이 배우의 전용 말풍선 호출!
                actor.myBubble.Show(data.Text, actor.targetObj, defaultDuration);
            }
            else
            {
                Debug.LogWarning($"배우 '{data.Speaker}' 세팅이 안 됐거나 말풍선이 없습니다.");
            }

            // 대기 및 스킵 로직 (기존 유지)
            float timer = 0f;
            float waitTime = defaultDuration + 0.5f;

            while (timer < waitTime)
            {
                timer += Time.deltaTime;
                if (Input.GetKey(skipKey)) break;
                yield return null;
            }
            
            // (선택) 다음 대사 넘어가기 전에 현재 말풍선을 끄고 싶다면:
            // if (actor.myBubble != null) actor.myBubble.Hide();
            // 하지만 보통은 켜둔 채로 다음 사람이 말하는 게 자연스럽습니다.
        }

        // 컷신 종료 시 모든 배우의 말풍선 끄기
        foreach (var actor in actors)
        {
            if (actor.myBubble != null) actor.myBubble.Hide();
        }

        if (GameManager.Instance != null) GameManager.Instance.EndCutscene();
    }

    // 이름으로 ActorMapping 구조체 찾기
    private ActorMapping GetActor(string name)
    {
        foreach (var actor in actors)
        {
            if (actor.csvName == name) return actor;
        }
        // 못 찾으면 빈 껍데기 반환
        return new ActorMapping();
    }
}