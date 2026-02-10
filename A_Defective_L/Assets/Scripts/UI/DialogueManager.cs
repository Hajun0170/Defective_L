using System.Collections.Generic;
using UnityEngine;

// 대사 데이터를 담을 구조체
public class DialogueEntry //대사 출력 코드인데 현재 보류함. 이해도가 부족해서 ㅇㅇ
{
    public string Speaker; //  L
    public string Text;    // 내용
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    // string 대신 DialogueEntry를 저장하도록 변경
    private Dictionary<string, DialogueEntry> dialogueMap = new Dictionary<string, DialogueEntry>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
        LoadCSV();
    }

    private void LoadCSV()
    {
        TextAsset data = Resources.Load<TextAsset>("DialogueData");
        if (data == null) return;

        string[] lines = data.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(',');
            if (parts.Length >= 3)
            {
                string key = parts[0].Trim();
                string speaker = parts[1].Trim(); // 두 번째 칸이 캐릭터
                string text = string.Join(",", parts, 2, parts.Length - 2).Trim();

                // 딕셔너리에 구조체로 저장... 
                if (!dialogueMap.ContainsKey(key))
                {
                    dialogueMap.Add(key, new DialogueEntry { Speaker = speaker, Text = text });
                }
            }
        }
    }

    // ID를 주면 구조체 Speaker + Text를 리턴
    public DialogueEntry GetDialogue(string id)
    {
        return dialogueMap.ContainsKey(id) ? dialogueMap[id] : null;
    }
}