using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform tTarget;             // 따라갈 대상 
    public Vector3 vOffset = new Vector3(0, 0, -10); // 기본 시야 오프셋
    public float fSmoothSpeed = 5f;       // 따라가는 부드러움

    //특정 상황에서 카메라 시점을 뺏기 위한 변수
    private bool isCutsceneMode = false;

    void LateUpdate()
    {
        if (tTarget == null || isCutsceneMode) return;

        Vector3 vTargetPos = tTarget.position + vOffset;
        Vector3 vSmoothedPos = Vector3.Lerp(transform.position, vTargetPos, fSmoothSpeed * Time.deltaTime);
        transform.position = vSmoothedPos;
    }

    //GameManager에서 타겟을 설정해주는 함수
    public void SetTarget(Transform newTarget)
    {
        tTarget = newTarget;
        // 씬 이동 직후에는 텔레포트하듯이 즉시 이동 
        transform.position = tTarget.position + vOffset;
    }

    // 외부에서 카메라를 조종하는 신호 주는 함수
    public void SetCutsceneMode(bool isActive)
    {
        isCutsceneMode = isActive;
    }
}
  
