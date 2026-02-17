using UnityEngine;

public class StageAbilityUnlocker : MonoBehaviour //테스트용으로 능력 확인
{
    [Header("테스트용 능력 해금 설정")]
    public bool unlockSprint = true;
    public bool unlockWallCling = true;

    void Start()
    {
        // 에디터에서 실행할 때만 작동
#if UNITY_EDITOR
        if (unlockSprint) 
        {
            DataManager.Instance.currentData.hasSprint = true;
            Debug.Log("질주 능력 자동 해금됨");
        }

        if (unlockWallCling)
        {
            DataManager.Instance.currentData.hasWallCling = true;
        }
#endif
    }
}