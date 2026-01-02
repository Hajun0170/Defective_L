using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Settings")]
    public string sceneName; // 이동할 씬 이름 (예: Stage2)
    public Vector2 spawnPoint; // 이동 후 캐릭터가 서있을 좌표

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 플레이어에게 "다음 씬에서의 위치"를 미리 저장해둠
            GameManager.Instance.NextSpawnPoint = spawnPoint;
            
            // 씬 로드
            SceneManager.LoadScene(sceneName);
        }
    }
}