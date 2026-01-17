using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("설정")]
    public string transferSceneName; // 이동할 씬 이름 (예: Stage_2)
    public int targetSpawnID;        // 그 씬의 몇 번 위치로 갈 건지

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. 목적지 번호표 저장
            DataManager.Instance.nextSpawnPointID = targetSpawnID;

            // 2. 우아하게 퇴장 (페이드 아웃 -> 씬 이동)
            SceneTransitionManager.Instance.LoadScene(transferSceneName);
        }
    }
}