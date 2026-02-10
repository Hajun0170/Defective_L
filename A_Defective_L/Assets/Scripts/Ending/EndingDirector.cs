using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.SceneManagement;

public class EndingDirector : MonoBehaviour //엔딩 씬 관리자
{
    [Header("1. Actors")]
    public Transform playerSpawnPoint; 
    public Transform walkTargetPoint;  
    public Camera mainCamera;

    [Header("2. Directing Settings")]
    public float startFadeDuration = 3.0f; 
    public float walkSpeed = 2.5f;     
    
    //카메라 위치 보정 (X, Y)
    //(0, 2)로 설정하면 플레이어보다 Y축으로 2만큼 위를 비춤. 카메라 위치 조정
    public Vector2 cameraOffset = new Vector2(0f, 2.0f); 

    public float zoomSize = 7.0f;      
    public float zoomDuration = 4.0f;  

    [Header("3. UI Elements")]
    public CanvasGroup endingTextGroup; 

    private GameObject playerObj;
    private Animator playerAnim;

    //Start() 대신 Awake()를 사용해서 초기화 작업을 먼저 수행.. UI 틀어지는 문제는 일단 보류
    private void Awake()
    {
        //UIManager가 있다면 화면을 검게 덮고 UI를 꺼버림
        if (UIManager.Instance != null)
        {
            // UI 끄기
            UIManager.Instance.SetHUDActive(false);
            UIManager.Instance.SetUpgradePanelActive(false);
            UIManager.Instance.TogglePauseUI(false);

            // 강제로 페이드 이미지를 최상단으로 올리고 불투명하게 만듦
            if (UIManager.Instance.fadeImage != null)
            {
                UIManager.Instance.fadeImage.gameObject.SetActive(true);
                
                //페이드 이미지가 다른 UI보다 뒤에 있으면 안 보여서 
                // 맨 마지막 순서(SetAsLastSibling)로 보내서 가장 위에 그리게 함
                UIManager.Instance.fadeImage.transform.SetAsLastSibling(); 
                
                Color c = UIManager.Instance.fadeImage.color;
                c.a = 1f; //검정
                UIManager.Instance.fadeImage.color = c;
            }
        }
    }
    private void Start()
    {
        StartCoroutine(EndingSequence());
    }

    IEnumerator EndingSequence()
    {
        if (GameManager.Instance != null) GameManager.Instance.IsCutscene = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHUDActive(false); 
            UIManager.Instance.SetUpgradePanelActive(false); 
            UIManager.Instance.TogglePauseUI(false);
            
            if (UIManager.Instance.keyHintObjects != null)
            {
                foreach (var obj in UIManager.Instance.keyHintObjects)
                    if (obj != null) obj.SetActive(false);
            }

            float originalFadeDuration = UIManager.Instance.fadeDuration;
            UIManager.Instance.fadeDuration = startFadeDuration;
            
            StartCoroutine(UIManager.Instance.FadeIn()); 
        }

        playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerObj.transform.position = playerSpawnPoint.position;
            
            //카메라 초기 위치도 오프셋 적용
            Vector3 initialCamPos = playerObj.transform.position;
            initialCamPos.x += cameraOffset.x;
            initialCamPos.y += cameraOffset.y;
            initialCamPos.z = -10f; //2D 카메라는 Z값 고정 필수
            mainCamera.transform.position = initialCamPos;

            if (playerObj.TryGetComponent(out PlayerMovement movement)) movement.enabled = false;
            if (playerObj.TryGetComponent(out PlayerAttack attack)) attack.enabled = false;
            
            playerAnim = playerObj.GetComponent<Animator>();
        }

        yield return new WaitForSeconds(startFadeDuration); 


        // 플레이어 걸어가기 + 카메라 따라가기 연출. 물리 문제로 캐릭터 Y축을 프리징 했음
 
        if (playerObj != null && walkTargetPoint != null)
        {
            if (playerAnim != null)
            {
              playerAnim.SetBool("IsRun", true);
            }

            Vector3 scale = playerObj.transform.localScale;
            scale.x = Mathf.Abs(scale.x); 
            playerObj.transform.localScale = scale;

            while (Vector2.Distance(playerObj.transform.position, walkTargetPoint.position) > 0.1f)
            {
                playerObj.transform.position = Vector2.MoveTowards(
                    playerObj.transform.position, 
                    walkTargetPoint.position, 
                    walkSpeed * Time.deltaTime
                );

                //카메라가 플레이어 + 오프셋 위치를 따라감
                Vector3 camPos = playerObj.transform.position;
                camPos.x += cameraOffset.x; // 우측으로 더 이동하고 싶으면 양수
                camPos.y += cameraOffset.y; // 위쪽을 비추고 싶으면 양수
                camPos.z = -10f;
                mainCamera.transform.position = camPos;

                yield return null;
            }

            if (playerAnim != null)
            {
               playerAnim.SetBool("IsRun", false); //걷기 애니메이션 종료
                playerAnim.Play("Idle_E"); //도착해서 정면 보기
            }
        }

        yield return new WaitForSeconds(0.5f);

        // 카메라 줌 아웃 (위치 고정)
  
        float startSize = mainCamera.orthographicSize;
        float time = 0f;
        
        // 마지막으로 맞춰진 카메라 위치(플레이어 도착지점 + 오프셋)를 기억
        Vector3 fixedCamPos = mainCamera.transform.position;

        while (time < zoomDuration)
        {
            time += Time.deltaTime;
            float t = time / zoomDuration;
            
            mainCamera.orthographicSize = Mathf.Lerp(startSize, zoomSize, t);
            mainCamera.transform.position = fixedCamPos; // 위치 고정
            
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);

        //암전과 동시에 텍스트 보여주기

        if (UIManager.Instance != null)
        {
            UIManager.Instance.fadeDuration = 1.0f; 
            StartCoroutine(UIManager.Instance.FadeOut());
        }

        yield return new WaitForSeconds(1.0f); 

        if (endingTextGroup != null)
        {
            endingTextGroup.gameObject.SetActive(true);
            endingTextGroup.alpha = 0f;
            
            float fadeTime = 0f;
            while (fadeTime < 2.0f)
            {
                fadeTime += Time.deltaTime;
                endingTextGroup.alpha = fadeTime / 2.0f;
                yield return null;
            }
        }

        yield return new WaitForSeconds(2.0f); 

        //타이틀로 되돌림

        if (GameManager.Instance != null) GameManager.Instance.IsCutscene = false;
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Title"); 
    }
}