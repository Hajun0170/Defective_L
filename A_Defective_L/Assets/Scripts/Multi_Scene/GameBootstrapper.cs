using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrapper : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private string GaugeSceneName = "Scene_Gauge";
    [SerializeField] private string firstStageName = "Prologue"; 

    private void Start()
    {
        // 1. 게이지 씬을 '겹쳐서' 로드
        SceneManager.LoadScene(GaugeSceneName, LoadSceneMode.Additive);

        // 2. 첫 번째 스테이지 로드 (해당 씬은 스테이지 진행에 따라 자유로이 교체)
        SceneManager.LoadScene(firstStageName, LoadSceneMode.Additive);

    }
}