using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 쓴다면 필수 (일반 Text면 Text로 변경)

public class UpgradeManager : MonoBehaviour
{
    [Header("Data")]
    // ★ 게임에 존재하는 모든 무기 데이터(ScriptableObject)를 순서대로 넣으세요!
    // 인덱스 0번엔 weaponID가 0인 무기, 1번엔 1인 무기... 순서 중요!
    public Weapon[] allWeapons; 

    [Header("UI Components")]
    public Image weaponIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI statusText; // "공격력 10 -> 12" 같은 정보
    public TextMeshProUGUI costText;
    public Button upgradeButton;

    private int currentIdx = 0; // 현재 보고 있는 무기 번호

    // 패널이 켜질 때마다 실행됨
    private void OnEnable()
    {
        currentIdx = 0; // 0번 무기부터 보여주기
        UpdateUI();
    }

    private void Update()
    {
        // 패널이 켜져 있을 때만 작동
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClickClose(); // 기존에 만들어둔 닫기 함수 호출
        }

        // (팁) 키보드 좌우 방향키로 무기 넘기기도 넣고 싶다면?
       // if (Input.GetKeyDown(KeyCode.LeftArrow)) ClickChangeWeapon(-1);
       // if (Input.GetKeyDown(KeyCode.RightArrow)) ClickChangeWeapon(1);
    }

    // 화면 갱신 함수
    void UpdateUI()
    {
        if (allWeapons.Length == 0) return;

        // 1. 데이터 가져오기
        Weapon weapon = allWeapons[currentIdx];
        int currentLvl = DataManager.Instance.currentData.weaponLevels[currentIdx];
        int myGold = DataManager.Instance.currentData.gold;
        int cost = 1; // 강화 비용 (개당 1개라고 하셨으니 고정)
        int maxLevel = 3; // 최대 강화 수치

        // 2. 텍스트/이미지 갱신
        weaponIcon.sprite = weapon.icon;
        nameText.text = weapon.weaponName;
        
        // 능력치 미리보기 계산
        // 현재 공격력 vs 다음 레벨 공격력
        int currentDmg = weapon.baseDamage + (currentLvl * weapon.damagePerLevel);
        int nextDmg = weapon.baseDamage + ((currentLvl + 1) * weapon.damagePerLevel);

        if (currentLvl >= maxLevel)
        {
            levelText.text = $"Lv.MAX";
            statusText.text = $"{currentDmg} (최대)";
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

    // [버튼 연결] 강화하기
    public void ClickUpgrade()
    {
        int myGold = DataManager.Instance.currentData.gold;
        int currentLvl = DataManager.Instance.currentData.weaponLevels[currentIdx];
        int cost = 1;

        if (myGold >= cost && currentLvl < 3)
        {
            // 1. 재화 소모 & 레벨 업
            DataManager.Instance.currentData.gold -= cost;
            DataManager.Instance.currentData.weaponLevels[currentIdx]++;

            // 2. 저장 (중요: 강화 직후 저장해야 안전함)
            DataManager.Instance.SaveDataToDisk();

            // 3. UI 갱신 & 효과음
            Debug.Log($"🔨 {allWeapons[currentIdx].weaponName} 강화 성공!");
            UpdateUI();
        }
    }

    // [버튼 연결] 다음/이전 무기 보기
    public void ClickChangeWeapon(int direction) // +1 또는 -1
    {
        currentIdx += direction;

        // 범위 넘어가면 순환시키기
        if (currentIdx >= allWeapons.Length) currentIdx = 0;
        if (currentIdx < 0) currentIdx = allWeapons.Length - 1;

        UpdateUI();
    }

    // [버튼 연결] 닫기
    public void ClickClose()
    {
        gameObject.SetActive(false);
    }
}