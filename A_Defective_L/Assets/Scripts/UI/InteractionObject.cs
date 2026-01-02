using UnityEngine;

public class InteractionObject : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private string dialogueID;
    
    // [추가] 말풍선이 뜰 위치 (이걸 안 넣으면 자기 자신의 위치 사용)
    [SerializeField] private Transform dialoguePoint; 

    private bool isPlayerNearby = false;

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
           // string text = DialogueManager.Instance.GetDialogue(dialogueID);

            // [수정] dialoguePoint가 설정되어 있으면 그걸 쓰고, 없으면 내 위치(transform) 사용
            Transform targetParams = (dialoguePoint != null) ? dialoguePoint : transform;

            // 유니티 6 권장 함수 사용
           // FindAnyObjectByType<DialogueBubble>().Show(text, targetParams, 2.0f);
        }
    }
    // ... OnTriggerEnter/Exit 코드는 그대로 ...
}