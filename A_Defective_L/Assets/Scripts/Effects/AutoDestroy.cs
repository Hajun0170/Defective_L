using UnityEngine;

public class AutoDestroy : MonoBehaviour //이펙트 1번만 출력하고 끔
{
    void Start()
    {
        // 애니메이터 컴포넌트 가져오기
        Animator anim = GetComponent<Animator>();
        
        if (anim != null)
        {
            // 현재 재생 중인 애니메이션의 길이를 가져옴
            float length = anim.GetCurrentAnimatorStateInfo(0).length;
            // 그 시간 뒤에 삭제
            Destroy(gameObject, length);
        }
        else
        {
            // 애니메이션 없으면 0.5초 뒤 삭제 
            Destroy(gameObject, 0.5f);
        }
    }
}