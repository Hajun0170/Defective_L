using UnityEngine;

public class GroundSensor2D : MonoBehaviour
{
    [Header("Ground Check")]
    public LayerMask lmGround;
    public Vector2 vOffset = new Vector2(0f, -0.6f);
    public float fRadius = 0.18f;

    [Header("Coyote/Buffer")]
    public float fCoyoteTime = 0.1f;
    public float fJumpBuffer = 0.1f;

    float fCoy, fBuf;
    public bool bIsGrounded { get; private set; }
    public bool CanCoyoteJump => fCoy > 0f;
    public bool HasBufferedJump => fBuf > 0f;

    void Update(){
        Vector2 p = (Vector2)transform.position + vOffset;
        bIsGrounded = Physics2D.OverlapCircle(p, fRadius, lmGround) != null;
        if (bIsGrounded) fCoy = fCoyoteTime; else fCoy -= Time.deltaTime;
        if (fBuf > 0f) fBuf -= Time.deltaTime;
    }

    public void NotifyJumpPressed(){ fBuf = fJumpBuffer; }
    public void ConsumeBuffer(){ fBuf = 0f; }

#if UNITY_EDITOR
    void OnDrawGizmosSelected(){
        Gizmos.color = Color.green;
        Vector2 p = (Vector2)transform.position + vOffset;
        Gizmos.DrawWireSphere(p, fRadius);
    }
#endif
}
