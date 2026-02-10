using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro

public class UpgradeManager : MonoBehaviour //무기 강화 코드
{
    [Header("Data")]
    // 모든 무기 데이터 순서대로 삽입
    // 0~5번까지 총 6개의 무기를 강화. 
    public Weapon[] allWeapons; 

    [Header("UI Components")]
    public Image weaponIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI statusText; // 공격력 10 -> 12 같은 스탯치
    public TextMeshProUGUI costText;
    public Button upgradeButton;

    private int currentIdx = 0; // 현재 보고 있는 무기

    // 패널이 켜질 때마다 실행됨
    private void OnEnable()
    {
       // 데이터가 없으면 종료. 오류로 안정 장치 필요.
        if (DataManager.Instance == null || allWeapons.Length == 0) return;

        // 0번부터 시작
        currentIdx = 0;
        
        // 만약 0번 무기가 없다면, 있을 때까지 다음으로 넘김
        if (!DataManager.Instance.currentData.hasWeapons[currentIdx])
        {
            ClickChangeWeapon(1); 
        }
        else
        {
            UpdateUI();
        }
    }

    private void Update()
    {
        // 패널이 켜져 있을 때만 작동
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClickClose(); // 기존에 만들어둔 닫기 함수 호출
        }

        // 키보드 좌우 방향키는 플레이어의 이동을 멈춰야하는데 멈추는 과정에서 다른 곳과 꼬여서 오류 발생. 보류
       // if (Input.GetKeyDown(KeyCode.LeftArrow)) ClickChangeWeapon(-1);
       // if (Input.GetKeyDown(KeyCode.RightArrow)) ClickChangeWeapon(1);
    }

    // 화면 갱신 함수
    void UpdateUI()
    {
        if (allWeapons.Length == 0) return;

        // 데이터 가져오기
        Weapon weapon = allWeapons[currentIdx];
        int currentLvl = DataManager.Instance.currentData.weaponLevels[currentIdx];
        int myGold = DataManager.Instance.currentData.gold;
        int cost = 1; // 강화 비용. 2로 조정할 지 고민. 필드 입수 난이도가 낮은 편이라 남아도는 편
        int maxLevel = 3; // 최대 강화 수치

        // 텍스트/이미지 갱신
        weaponIcon.sprite = weapon.icon;
        nameText.text = weapon.weaponName;
        
        // 능력치 미리보기 계산
        // 현재 공격력과 다음 레벨 공격력
        int currentDmg = weapon.baseDamage + (currentLvl * weapon.damagePerLevel);
        int nextDmg = weapon.baseDamage + ((currentLvl + 1) * weapon.damagePerLevel);

        if (currentLvl >= maxLevel)
        {
            levelText.text = $"MAX";
            statusText.text = $"{currentDmg}:MAX";
            costText.text = "-";
            upgradeButton.interactable = false; // 최대 레벨이면 버튼 비활성
        }
        else
        {
            levelText.text = $"{currentLvl +1} > <color=green>{currentLvl + 2}</color>";
            statusText.text = $"{currentDmg} > <color=green>{nextDmg}</color>";
            
            // 돈 부족하면 빨간색, 충분하면 흰색
            string color = (myGold >= cost) ? "white" : "red";
            costText.text = $"<color={color}>{cost}</color> / {myGold}";

            // 돈 있고 레벨 낮으면 버튼 활성
            upgradeButton.interactable = (myGold >= cost);
        }
    }

    // 강화하기 버튼
    public void ClickUpgrade()
    {
        int myGold = DataManager.Instance.currentData.gold;
        int currentLvl = DataManager.Instance.currentData.weaponLevels[currentIdx];
        int cost = 1;

        if (myGold >= cost && currentLvl < 3)
        {
            // 재화 소모와 레벨 업
            DataManager.Instance.currentData.gold -= cost;
            DataManager.Instance.currentData.weaponLevels[currentIdx]++;

            // 저장. 강화 직후 저장
            DataManager.Instance.SaveDataToDisk();

            // UI 갱신. 효과음
            Debug.Log($"{allWeapons[currentIdx].weaponName} 강화 성공");
            UpdateUI();
        }
    }

    // [버튼 연결] 다음/이전 무기 보기
    public void ClickChangeWeapon(int direction) // +1 또는 -1
    {
        int loopCount = 0; // 무한 루프 방지용
        int totalWeapons = allWeapons.Length;

        // 가진 무기가 나올 때까지 반복해서 넘김
        while (loopCount < totalWeapons)
        {
            currentIdx += direction;

            // 순환
            if (currentIdx >= totalWeapons) currentIdx = 0;
            if (currentIdx < 0) currentIdx = totalWeapons - 1;

            //데이터 매니저로 내가 이 무기를 가지고 있는지 확인
            if (DataManager.Instance.currentData.hasWeapons[currentIdx])
            {
                // 가지고 있으면 UI 갱신하고 종료
                UpdateUI();
                return;
            }

            // 안 가지고 있으면 while문이 다시 돌면서 다음 인덱스로 넘어감
            loopCount++;
        }

        //가진 무기가 하나도 없음 = 에러 발생. 현재 설계상 불가능한 경우지만 예외인 경우를 위해 삽입
        Debug.Log("소지한 무기가 없습니다.");
    }

    // 닫기 버튼
    public void ClickClose()
    {
        gameObject.SetActive(false);
    }
}