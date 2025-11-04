using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform tTarget;             // 따라갈 대상 (플레이어)
    public Vector3 vOffset = new Vector3(0, 0, -10); // 기본 시야 오프셋
    public float fSmoothSpeed = 5f;       // 따라가는 부드러움 정도

    void LateUpdate()
    {
        if (tTarget == null) return;

        Vector3 vTargetPos = tTarget.position + vOffset;
        Vector3 vSmoothedPos = Vector3.Lerp(transform.position, vTargetPos, fSmoothSpeed * Time.deltaTime);
        transform.position = vSmoothedPos;
    }
}
