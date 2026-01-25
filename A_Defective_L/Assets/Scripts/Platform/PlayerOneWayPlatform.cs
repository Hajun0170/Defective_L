using System.Collections;
using UnityEngine;

public class PlayerOneWayPlatform : MonoBehaviour
{
    private GameObject currentOneWayPlatform; // 현재 밟고 있는 발판
    [SerializeField] private Collider2D playerCollider; // 플레이어의 콜라이더

    private void Update()
    {
        // 아래 키가 눌렸고 + 현재 단방향 발판 위에 서 있다면
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentOneWayPlatform != null)
            {
                StartCoroutine(DisableCollision());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 발판에 닿았을 때 그 발판이 'OneWayPlatform' 태그나 레이어인지 확인
        if (collision.gameObject.CompareTag("OneWayPlatform")) 
        {
            currentOneWayPlatform = collision.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // 발판에서 벗어나면 초기화
        if (collision.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = null;
        }
    }

    private IEnumerator DisableCollision()
    {
        // 1. 발판의 콜라이더를 가져옴 (타일맵이면 CompositeCollider2D, 일반이면 BoxCollider2D 등)
        Collider2D platformCollider = currentOneWayPlatform.GetComponent<Collider2D>();

        // 2. 플레이어와 발판 간의 충돌을 잠시 끈다 (물리적으로 통과됨)
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);

        // 3. 0.5초 정도 기다린다 (충분히 빠져나갈 시간)
        yield return new WaitForSeconds(0.5f);

        // 4. 다시 충돌을 켠다
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }
}