using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // 코루틴 필수

public class GameBootstrapper : MonoBehaviour //멀티씬 테스트용: 사용 안 함
{
    [Header("설정")]
    [SerializeField] private string GaugeSceneName = "Scene_Gauge";
    [SerializeField] private string firstStageName = "Prologue"; 

    private IEnumerator Start()
    {
        //게이지 씬을 겹쳐서 로드
        //SceneManager.LoadScene(GaugeSceneName, LoadSceneMode.Additive);

        // 1번째 스테이지 로드: 스테이지 진행에 따라 교체
       yield return SceneManager.LoadSceneAsync(GaugeSceneName, LoadSceneMode.Additive);

        // 2. 그 다음 스테이지 로드
        yield return SceneManager.LoadSceneAsync(firstStageName, LoadSceneMode.Additive);

        
        // GameManager에게 현재 Prologue 맵을 연 것을 알림 
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentStageName = firstStageName;
        }
    }
}