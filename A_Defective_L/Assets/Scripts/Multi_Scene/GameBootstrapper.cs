using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // 코루틴 필수

public class GameBootstrapper : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private string GaugeSceneName = "Scene_Gauge";
    [SerializeField] private string firstStageName = "Prologue"; 

    private IEnumerator Start()
    {
        // 1. 게이지 씬을 '겹쳐서' 로드
        //SceneManager.LoadScene(GaugeSceneName, LoadSceneMode.Additive);

        // 2. 첫 번째 스테이지 로드 (해당 씬은 스테이지 진행에 따라 자유로이 교체)
       // SceneManager.LoadScene(firstStageName, LoadSceneMode.Additive);
       yield return SceneManager.LoadSceneAsync(GaugeSceneName, LoadSceneMode.Additive);

        // 2. 그 다음 스테이지 로드
        yield return SceneManager.LoadSceneAsync(firstStageName, LoadSceneMode.Additive);

        
        // ★ [중요] GameManager에게 "지금 Prologue 맵 켰어"라고 알려줌
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentStageName = firstStageName;
        }
    }
}