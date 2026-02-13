using UnityEngine;

public class TitleUIConnector : MonoBehaviour
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