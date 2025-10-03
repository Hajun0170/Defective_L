using UnityEngine;
using UnityEngine.InputSystem; // InputSystem을 사용하기 위해 네임스페이스 추가

[RequireComponent(typeof(PlayerMovement), typeof(PlayerJump), typeof(PlayerDash))]
public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerJump jump;
    private PlayerDash dash;
    
    private Vector2 moveInput; // 이동 값을 저장할 변수

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        jump = GetComponent<PlayerJump>();
        dash = GetComponent<PlayerDash>();
    }

    // PlayerInput 컴포넌트가 'Move' 액션을 감지했을 때 이 함수를 호출합니다.
    // (Input Actions 에셋에서 설정한 이름과 동일해야 함)
    public void OnMove(InputAction.CallbackContext context)
    {
        // context.ReadValue<Vector2>()는 (x, y) 값을 읽어옵니다. 
        // 좌우 이동이므로 x값만 필요합니다.
        moveInput = context.ReadValue<Vector2>();
    }
    
    // 'Jump' 액션이 감지되었을 때 호출
    public void OnJump(InputAction.CallbackContext context)
    {
        // context.performed는 키가 '눌렸을 때'를 의미합니다.
        if (context.performed)
        {
            jump.PerformJump();
        }
    }
    
    // 'Dash' 액션이 감지되었을 때 호출
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            dash.PerformDash();
        }
    }

    // 물리 업데이트는 FixedUpdate에서 처리하는 것이 더 안정적입니다.
    void FixedUpdate()
    {
        // OnMove에서 받아온 x값을 PlayerMovement에 전달합니다.
        movement.Move(moveInput.x);
    }
}