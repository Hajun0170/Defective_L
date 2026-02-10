using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueBubble : MonoBehaviour //대사 출력 코드이고 안 써서 보류
{
    [SerializeField] private GameObject bubbleObj;
    [SerializeField] private TextMeshProUGUI textComp;

    private Transform target;

    private void Update()
    {
        if (target != null && bubbleObj.activeSelf)
        {
            //target.position 사용
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