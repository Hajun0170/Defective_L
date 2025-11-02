using UnityEngine;
using UnityEngine.InputSystem;

// 역할: Input System 이벤트만 받고 '의도'를 Motor에 전달
public class PlayerInputProxy : MonoBehaviour
{
    public PlayerMotor cMotor;
    public GroundSensor2D cGround; // 점프 버퍼용

    public void OnMove(InputAction.CallbackContext ctx){
        Vector2 v = ctx.ReadValue<Vector2>();
        cMotor.SetMoveX(v.x);
    }

    public void OnJump(InputAction.CallbackContext ctx){
        if (ctx.started){
            cGround.NotifyJumpPressed();
            cMotor.SetJumpHeld(true);
            cMotor.RequestJump();
        }
        if (ctx.canceled){
            cMotor.SetJumpHeld(false);
        }
    }

    public void OnDash(InputAction.CallbackContext ctx){
        if (ctx.started) cMotor.RequestDash();
        // (유지 여부는 Motor가 자체 판단)
    }

    // 공격/상호작용 추가 예정: OnMelee / OnRanged ...
}
