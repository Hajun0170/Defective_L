using UnityEngine;

public class TitleUIConnector : MonoBehaviour //타이틀과 인게임의 옵션창 연동. 대부분의 기능은 SettingsU으로 옮김
{
    [Header("Window Control")]
    public GameObject optionWindowPanel; 

    private void Start()
    {
        // 옵션창 끄기
        if (optionWindowPanel != null) optionWindowPanel.SetActive(false);
    }

    public void OpenOptions() => optionWindowPanel.SetActive(true);
    public void CloseOptions() => optionWindowPanel.SetActive(false);
}