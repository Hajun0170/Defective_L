using UnityEngine;

public class EndingElevator : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 2.0f; // 승강기 올라가는 속도
    public Transform stopPosition; // 승강기가 멈출 위치 (Y축) - 포탈 위치 쯤

    private bool isActivated = false;
    private Transform playerTransform;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActivated && collision.CompareTag("Player"))
        {
            StartElevator(collision.gameObject);
        }
    }

    void StartElevator(GameObject player)
    {
        isActivated = true;
        playerTransform = player.transform;

        // ★ [핵심 추가] 애니메이터 파라미터 초기화 (스크립트 끄기 전에 해야 함!)
        Animator anim = player.GetComponent<Animator>();
        if (anim != null)
        {
            // 1. 달리기 관련 파라미터를 모두 끕니다.
            // (님 애니메이터 설정에 맞는 변수명을 쓰세요. 예: "Speed", "IsRun", "MoveX" 등)
            //anim.SetFloat("Speed", 0f); 
            anim.SetBool("IsRun", false); // 혹시 Bool 쓰시면 이것도
            
            // 2. 강제로 Idle 상태 재생
            anim.Play("Idle"); 
        }

        // 3. 물리력 초기화 (관성 제거)
        if (player.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // 물리 영향 안 받게
        }

        // 4. 플레이어 이동 스크립트 끄기
        if (player.TryGetComponent(out PlayerMovement movement))
        {
            movement.enabled = false; 
        }
        
        // 공격 스크립트도 끄기 (선택)
        if (player.TryGetComponent(out PlayerAttack attack))
        {
            attack.enabled = false;
        }

        // 5. 플레이어를 승강기의 자식으로 (같이 이동)
        player.transform.SetParent(this.transform);
    }

    void Update()
    {
        if (isActivated)
        {
            // 위로 이동
            transform.Translate(Vector2.up * speed * Time.deltaTime);

            // 목표 지점 도달하면 멈춤 (혹은 그냥 쭉 가다가 포탈에 닿게 둬도 됨)
            if (stopPosition != null && transform.position.y >= stopPosition.position.y)
            {
                isActivated = false; // 이동 끝
            }
        }
    }
}