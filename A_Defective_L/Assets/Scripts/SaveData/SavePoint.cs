using UnityEngine;
using UnityEngine.SceneManagement; // ★ 이 줄이 없어서 에러가 났던 것입니다!

public class SavePoint : MonoBehaviour
{
    [Header("쉼터 설정")]
    public int shelterID = 0; // ★ 이 쉼터의 고유 번호 (인스펙터에서 0, 1, 2... 설정)

    private bool isPlayerInRange = false;
    private Transform playerTransform; // ★ 플레이어 위치 정보를 저장할 변수

    

    // 플레이어가 쉼터 범위에 들어왔을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerTransform = collision.transform; // 플레이어 정보 기억해둠
            // 여기에 "저장하려면 화살표 위 키를 누르세요" UI 띄우기 코드 추가 가능

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerTransform = null; // 나갔으니 비움
        }
    }

    private void Update()
    {
        // 쉼터에서 상호작용 키(예: Z키, 윗방향키)를 누르면 저장
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.UpArrow))
        {
           // ★ DataManager에게 필요한 3가지를 다 넘겨줍니다.
            // 1. 플레이어 (위치 저장용)
            // 2. 현재 씬 이름 (나중에 여기로 로드하려고)
            // 3. 쉼터 ID
            DataManager.Instance.SaveGame(playerTransform, SceneManager.GetActiveScene().name, shelterID);
            
            Debug.Log($"쉼터({shelterID})에서 저장 완료!");
        }
    }
}