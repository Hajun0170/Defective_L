using System.Collections;
using UnityEngine;

public class PlayerOneWayPlatform : MonoBehaviour //나무 발판 기준 하단 키를 누르면 스무스하게 내려감
{
    private GameObject currentOneWayPlatform; // 현재 밟고 있는 발판
    [SerializeField] private Collider2D playerCollider; // 플레이어의 콜라이더

    private void Update()
    {
        // 아래 키가 눌렸고 + 발판 위에 서 있는 경우
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
        // 발판 'OneWayPlatform' 태그인지 확인
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

    private IEnumerator DisableCollision() //일시적으로 충돌을 끄고 다시 켜서 자연스럽게 통과 가능
    {
        // 발판의 콜라이더를 가져옴 BoxCollider2D
        Collider2D platformCollider = currentOneWayPlatform.GetComponent<Collider2D>();

        // 플레이어와 발판 충돌을 잠시 끔 (물리적으로 통과됨)
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);

        // 0.5초 정도 기다림
        yield return new WaitForSeconds(0.5f);

        //다시 충돌을 킴
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }
}