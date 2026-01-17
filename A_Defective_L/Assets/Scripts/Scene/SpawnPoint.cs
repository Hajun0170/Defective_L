using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("이 위치의 번호")]
    public int spawnID; 

    private void Awake()
    {
        // 씬이 켜지자마자 (화면이 밝아지기 전에) 검사
        if (DataManager.Instance.nextSpawnPointID == spawnID)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // 플레이어를 내 위치로 순간이동
                player.transform.position = transform.position;
            }
        }
    }

    // 에디터에서 위치 보기 편하게
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}