using UnityEngine;

public class EndingElevator : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 2.0f; // 승강기 올라가는 속도
    public Transform stopPosition; // 승강기가 멈출 위치: 포탈 위치와 묶어버림

    private bool isActivated = false;
    private Transform playerTransform;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActivated && collision.CompareTag("Player"))
        {
            StartElevator(collision.gameObject);
        }
    }

    void StartElevator(GameObject player) // 플레이어가 승강기에 탑승했을 때 이동을 묶고 자연스럽게 올라가게 만듦
    {
        isActivated = true;
        playerTransform = player.transform;

        // 애니메이터 초기화 
        Animator anim = player.GetComponent<Animator>();
        if (anim != null)
        {
            // 달리기 관련 파라미터를 모두 끔
            anim.SetBool("IsRun", false); 
            
            // Idle 상태 재생
            anim.Play("Idle"); 
        }

        //물리력 초기화 (관성 제거)
        if (player.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // 물리 영향 안 받음
        }

        // 플레이어 이동 스크립트 끄기
        if (player.TryGetComponent(out PlayerMovement movement))
        {
            movement.enabled = false; 
        }
        
        // 공격 스크립트 끄기 
        if (player.TryGetComponent(out PlayerAttack attack))
        {
            attack.enabled = false;
        }

        //플레이어를 승강기의 자식으로 포함하여 같이 이동
        player.transform.SetParent(this.transform);
    }

    void Update()
    {
        if (isActivated)
        {
            // 위로 이동
            transform.Translate(Vector2.up * speed * Time.deltaTime);

            // 목표 지점 도달하면 멈춤 
            if (stopPosition != null && transform.position.y >= stopPosition.position.y)
            {
                isActivated = false; // 이동 끝
            }
        }
    }
}