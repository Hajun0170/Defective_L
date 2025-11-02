using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    // 이벤트로 해금되는 능력들
    public bool bSprintUnlocked = false;
    public void UnlockSprint(){ bSprintUnlocked = true; }

    // 추후 확장 예시
    public bool bDoubleJumpUnlocked = false;
    public void UnlockDoubleJump(){ bDoubleJumpUnlocked = true; }
}
