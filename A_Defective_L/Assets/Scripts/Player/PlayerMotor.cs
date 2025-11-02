using UnityEngine;
using System.Collections;

// 이동/점프/대시/질주 물리 담당(입력은 Proxy에서 전달)
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Refs")]
    public Transform tSpriteRoot;
    public GroundSensor2D cGround;
    public AbilityManager cAbility;
    public PlayerStats cStats; // 대시 무적 등에 사용(선택)

    [Header("Move/Run")]
    public float fMoveSpeed = 6f;
    public float fRunSpeed = 9f;
    public float fRunAccelMult = 1.1f;
    public float fAirControl = 0.85f;
    public float fAcceleration = 60f;
    public float fDeceleration = 80f;
    public float fRunStartGrace = 0.08f; // 대시→질주 유예

    [Header("Jump")]
    public float fJumpPower = 12f;
    public float fGravity = 3.5f;
    public float fFallGravity = 4.5f;

    [Header("Dash")]
    public float fDashSpeed = 16f;
    public float fDashTime = 0.18f;
    public float fDashCooldown = 0.35f;

    Rigidbody2D rb;
    float fMoveX;
    bool bFacingRight = true;

    bool bDashing, bRunning, bCanDash = true;
    float fRunGraceLeft;
    bool bJumpHeld;   // 짧은 점프 컷용
    bool _wantJump;   // 버퍼된 점프 의도

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = fGravity;
    }

    // ===== 입력 의도 메서드 =====
    public void SetMoveX(float x){
        fMoveX = Mathf.Clamp(x, -1f, 1f);
        if (Mathf.Abs(fMoveX) > 0.01f) SetFacing(fMoveX > 0f);
    }
    public void SetJumpHeld(bool held){ bJumpHeld = held; }
    public void RequestJump(){ _wantJump = true; }
    public void RequestDash(){ if (!bDashing && bCanDash) StartCoroutine(CoDash()); }

    void Update(){
        // 중력 보정
        rb.gravityScale = (rb.linearVelocity.y < -0.01f ? fFallGravity : fGravity);

        // 점프(버퍼+코요테)
        if (_wantJump && (cGround.bIsGrounded || cGround.CanCoyoteJump)){
            _wantJump = false;
            cGround.ConsumeBuffer();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fJumpPower);
        }
        // 짧은 점프 컷
        if (!bJumpHeld && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);

        // 대시→질주
        if (!bDashing && !bRunning && cAbility && cAbility.bSprintUnlocked){
            if (fRunGraceLeft > 0f && IsDashKeyHeld() && Mathf.Abs(fMoveX) > 0.01f)
                StartRun();
        }
        if (bRunning){
            if (!IsDashKeyHeld() || Mathf.Abs(fMoveX) < 0.01f) StopRun();
        }
        if (fRunGraceLeft > 0f) fRunGraceLeft -= Time.deltaTime;
    }

    void FixedUpdate(){
        if (bDashing) return;

        float baseSpd = bRunning ? fRunSpeed : fMoveSpeed;
        float target = fMoveX * baseSpd;

        float accel = (Mathf.Abs(target) > 0.01f ? fAcceleration : fDeceleration);
        if (bRunning && Mathf.Abs(target) > 0.01f) accel *= fRunAccelMult;

        float air = cGround.bIsGrounded ? 1f : fAirControl;
        float newVX = Mathf.MoveTowards(rb.linearVelocity.x, target, accel * air * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newVX, rb.linearVelocity.y);
    }

    IEnumerator CoDash(){
        bDashing = true; bCanDash = false;
        if (cStats) cStats.bInvincible = true;

        float dir = bFacingRight ? 1f : -1f;
        float t = fDashTime;
        float prevGrav = rb.gravityScale; rb.gravityScale = 0f;
        while (t > 0f){
            rb.linearVelocity = new Vector2(dir * fDashSpeed, 0f);
            t -= Time.deltaTime; yield return null;
        }
        rb.gravityScale = prevGrav;
        if (cStats) cStats.bInvincible = false;
        bDashing = false;

        // 질주 유예
        fRunGraceLeft = fRunStartGrace;

        yield return new WaitForSeconds(fDashCooldown);
        bCanDash = true;
    }

    void StartRun(){ bRunning = true; /* Animator: IsRunning=true */ }
    void StopRun(){  bRunning = false;/* Animator: IsRunning=false */ }

    void SetFacing(bool right){
        if (bFacingRight == right) return;
        bFacingRight = right;
        var s = tSpriteRoot.localScale; s.x = Mathf.Abs(s.x) * (right?1f:-1f); tSpriteRoot.localScale = s;
    }

    // Proxy에서 키를 직접 주는 게 이상적이지만, 임시로 헬퍼 둠
    bool IsDashKeyHeld(){
#if ENABLE_INPUT_SYSTEM
        return UnityEngine.InputSystem.Keyboard.current != null &&
               UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed;
#else
        return Input.GetKey(KeyCode.LeftShift);
#endif
    }
}
