using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CutsceneDirector : MonoBehaviour // 대화 연출 코드
{
    [Header("설정")]
    [SerializeField] private bool playOnStart = true; // 시작하자마자 재생
    [SerializeField] private float defaultDuration = 2.5f; // 대사 시간

    // [추가] 스킵할 키 설정
    [SerializeField] private KeyCode skipKey = KeyCode.Z;

    [Header("UI 연결")]
    [SerializeField] private DialogueBubble dialogueBubble;

    [Header("대사 순서 (ID)")]
    [SerializeField] private string[] dialogueSequence; //예)S1_01, S1

    [Header("캐릭터 연결 (CSV: 오브젝트)")]
    [SerializeField] private List<ActorMapping> actors; 

    // 인스펙터에서 보일 클래스
    [System.Serializable]
    public struct ActorMapping
    {
        public string csvName;  // CSV에 적힌 이름 
        public Transform targetObj; // Player
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartCoroutine(PlaySequence());
        }
    }

    // 외부에서 트리거로 호출 가능
    public void StartCutscene()
    {
        StartCoroutine(PlaySequence());
    }
    
       private IEnumerator PlaySequence()
    {
        if (GameManager.Instance != null) GameManager.Instance.StartCutscene();

        foreach (string id in dialogueSequence)
        {
            DialogueEntry data = DialogueManager.Instance.GetDialogue(id);

         
            if (data != null)
            {
                Transform speakerTransform = GetActorTransform(data.Speaker);
                if (speakerTransform != null && dialogueBubble != null)
                {
                    dialogueBubble.Show(data.Text, speakerTransform, defaultDuration);
                }
            }

            float timer = 0f;
            float waitTime = defaultDuration + 0.5f; 

            while (timer < waitTime)
            {
                timer += Time.deltaTime;

                //GetKey -> GetKeyDown 으로 변경 (중복 입력 방지)
                if (Input.GetKeyDown(skipKey)) 
                {
                    break; 
                }

                yield return null;
            }
            
            //스킵 직후, 잠깐 대기해서 다음 키 입력이 먹히는 걸 방지
            yield return null; 
        }

        // 컷신이 끝나면 마지막 말풍선을 끄고 조작 해제
        if (dialogueBubble != null) dialogueBubble.Hide(); // 이전 함수 호출
        
        if (GameManager.Instance != null) GameManager.Instance.EndCutscene();
        Debug.Log("컷신 종료");
    }

    // CSV 이름으로 실제 Transform 찾기
    private Transform GetActorTransform(string name)
    {
        foreach (var actor in actors)
        {
            if (actor.csvName == name) return actor.targetObj;
        }
        return null; // 못 찾으면 null 반환
    }
}