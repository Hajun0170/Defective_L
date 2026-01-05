using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueBubble : MonoBehaviour
{
    [SerializeField] private GameObject bubbleObj;
    [SerializeField] private TextMeshProUGUI textComp;
    
    // [수정] offset 변수 삭제! (이제 타겟 위치 그대로 따라감)
    // [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0); 

    private Transform target;

    private void Update()
    {
        if (target != null && bubbleObj.activeSelf)
        {
            // [수정] offset 더하기 로직 제거 -> target.position 그대로 사용
            transform.position = Camera.main.WorldToScreenPoint(target.position);
        }
    }
    public void Hide()
    {
        StopAllCoroutines(); // 진행 중인 타이머 정지
        bubbleObj.SetActive(false); // 화면에서 숨김
    }

    public void Show(string text, Transform newTarget, float duration)
    {
        target = newTarget;
        textComp.text = text;
        bubbleObj.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(HideRoutine(duration));
    }

    private IEnumerator HideRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        bubbleObj.SetActive(false);
    }
}