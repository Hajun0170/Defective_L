using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    private void Start()
    {
        // 씬이 켜지자마자 코루틴 시작
        StartCoroutine(WaitAndMove());
    }

    IEnumerator WaitAndMove()
    {
        // 대기 
        yield return new WaitForSeconds(10.0f);

  
       // SceneManager.LoadScene("Prologue"); 

        SceneTransitionManager.Instance.LoadScene("Main_Scene"); //페이드 아웃 적용
    }
}