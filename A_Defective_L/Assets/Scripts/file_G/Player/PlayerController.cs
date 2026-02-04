using UnityEngine; // ★ 필수: 유니티 기능 사용 선언
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("벽 관련 설정")]
    public float wallSlideSpeed = 2f; // 슬라이딩 속도
    public Transform wallCheck;       // 벽 감지 위치 (빈 오브젝트)
    public LayerMask wallLayer;       // 벽 레이어 (Wall)

    [Header("상태 확인용 (보기만 하세요)")]
    public bool isTouchingWall;
    public bool isWallClinging;
    public bool isGrounded; // 땅 감지 여부

    // 내부 컴포넌트
    private Rigidbody2D rb;
    private Animator anim;
    
    // 땅 감지용 변수 (기존에 쓰시던 게 있다면 그걸 쓰세요)
    public Transform groundCheck;
    public LayerMask groundLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. 감지 로직 (땅 & 벽)
        CheckSurroundings();

        // 2. 벽 상호작용 로직 (아이템 검사 없이 바로 실행!)
        CheckWallInteraction();

        // 3. 애니메이션 업데이트
        UpdateAnimation();

        // ... 여기에 기존 이동(Move), 점프(Jump) 코드가 있어야 합니다 ...
    }

    void CheckSurroundings()
    {
        // 벽 감지
        if (wallCheck != null)
            isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);

        // 땅 감지 (기존 코드가 있다면 그걸 쓰시고, 없다면 이거라도 있어야 에러가 안 납니다)
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    void CheckWallInteraction()
    {
        // 땅에 있을 때는 벽 타기를 하지 않음
        if (isGrounded)
        {
            isWallClinging = false;
            // rb.gravityScale = 1; // (점프 로직 등에서 중력을 초기화해줘야 함)
            return;
        }

        // 공중이고 + 벽에 닿아있다면
        if (isTouchingWall)
        {
            // 플레이어가 보고 있는 방향이 오른쪽(1)인지 왼쪽(-1)인지
            float direction = transform.localScale.x; 

            // 벽 쪽으로 키를 꾹 누르고 있는지 확인
            // (오른쪽을 보고 있으면 오른쪽키, 왼쪽을 보고 있으면 왼쪽키)
            bool isPushingWall = (direction > 0 && Input.GetAxisRaw("Horizontal") > 0) ||
                                 (direction < 0 && Input.GetAxisRaw("Horizontal") < 0);

            if (isPushingWall)
            {
                // ★ [매달리기] 벽 쪽으로 키를 밀고 있으면 -> 딱 멈춤
                isWallClinging = true;
                rb.gravityScale = 0; // 중력 끄기
                rb.linearVelocity = Vector2.zero; // 속도 0으로 고정 (Unity 6.0버전은 linearVelocity, 구버전은 velocity)
            }
            else
            {
                // ★ [슬라이딩] 벽에 닿았지만 키를 안 누르면 -> 천천히 미끄러짐
                isWallClinging = false;
                rb.gravityScale = wallSlideSpeed; 
            }
        }
        else
        {
            // 벽에서 떨어지면 원래대로
            isWallClinging = false;
            rb.gravityScale = 1; // 기본 중력으로 복귀
        }
    }

    void UpdateAnimation()
    {
        if (anim != null)
        {
            // 매달리기 애니메이션
            anim.SetBool("IsWallClinging", isWallClinging);
            
            // 슬라이딩 애니메이션 (벽엔 붙어있는데, 매달린 건 아닐 때)
            anim.SetBool("IsWallSliding", isTouchingWall && !isGrounded && !isWallClinging);
        }
    }

    // 에디터에서 범위 그리기
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (wallCheck != null) Gizmos.DrawWireSphere(wallCheck.position, 0.2f);
        if (groundCheck != null) Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
    }

    void SwapWeapon()
    {
        // ... (무기 교체 로직: 1번 -> 2번) ...

        // ★ 교체권(스태미나 등)이 있어서 교체에 성공했다면?
        // 버프 발동!
        GetComponent<PlayerStats>().ActivateSwapBuff();
    }
}