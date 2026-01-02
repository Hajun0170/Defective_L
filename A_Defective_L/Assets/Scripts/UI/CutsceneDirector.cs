using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CutsceneDirector : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private bool playOnStart = true; // 시작하자마자 재생할지?
    [SerializeField] private float defaultDuration = 2.5f; // 대사 기본 지속 시간

    [Header("대사 순서 (ID 입력)")]
    [SerializeField] private string[] dialogueSequence; // S1_01, S1_02, ...

    [Header("배우 연결 (CSV 이름 : 실제 오브젝트)")]
    [SerializeField] private List<ActorMapping> actors; 

    // 인스펙터에서 보일 매핑용 클래스
    [System.Serializable]
    public struct ActorMapping
    {
        public string csvName;  // CSV에 적힌 이름 (예: L)
        public Transform targetObj; // 실제 게임 오브젝트 (Player)
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartCoroutine(PlaySequence());
        }
    }

    // 외부에서(트리거 등) 호출 가능
    public void StartCutscene()
    {
        StartCoroutine(PlaySequence());
    }
    
    private IEnumerator PlaySequence()
    {
        // 1. 조작 차단 (GameManager 연동)
        if (GameManager.Instance != null) GameManager.Instance.StartCutscene();

        // 2. 대사 순차 재생
        foreach (string id in dialogueSequence)
        {
            // ID로 데이터 가져오기
            DialogueEntry data = DialogueManager.Instance.GetDialogue(id);

            if (data != null)
            {
                // 화자 이름으로 실제 오브젝트 찾기
                Transform speakerTransform = GetActorTransform(data.Speaker);

                if (speakerTransform != null)
                {
                    // 말풍선 띄우기
                    // [기존] 꺼져 있으면 못 찾아서 에러 남
                    //FindAnyObjectByType<DialogueBubble>().Show(data.Text, speakerTransform, defaultDuration);

                    // [수정] 꺼져 있어도(Inactive) 포함해서 찾아라! (필수 옵션 추가)
                    FindAnyObjectByType<DialogueBubble>(FindObjectsInactive.Include).Show(data.Text, speakerTransform, defaultDuration);
                }
                else
                {
                    Debug.LogWarning($"배우를 찾을 수 없음: {data.Speaker}");
                }
            }
            
            // 대사가 끝날 때까지 대기 (지속시간 + 약간의 텀 0.5초)
            yield return new WaitForSeconds(defaultDuration + 0.5f);
        }

        // 3. 조작 해제
        if (GameManager.Instance != null) GameManager.Instance.EndCutscene();
        Debug.Log("컷신 종료");
    }

    // CSV 이름("L")으로 실제 Transform 찾기
    private Transform GetActorTransform(string name)
    {
        foreach (var actor in actors)
        {
            if (actor.csvName == name) return actor.targetObj;
        }
        return null; // 못 찾으면 null
    }
}