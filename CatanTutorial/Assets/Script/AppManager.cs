using UnityEngine;
using TMPro; // テキスト書き換え用

public class AppManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject TitlePanel;
    public GameObject ChapterSelectPanel;
    public GameObject SectionSelectPanel;
    public GameObject GamePanel;

    [Header("Dynamic UI")]
    public TextMeshProUGUI SectionTitleText; // セクション画面のタイトル文字
    
    [Header("Text Data")]
    // ↓これを追加：インスペクターで自由に文字を登録できるリスト
    public string[] ChapterTitles;

    // どのチャプターを選んだかを識別するID（0:はじめに, 1:1章, 2:2章...）
    private int currentChapterId;

    void Start()
    {
        ShowTitle();
    }

    // --- 画面遷移系 ---

    public void ShowTitle()
    {
        HideAllPanels();
        TitlePanel.SetActive(true);
    }

    public void GoToChapterSelect()
    {
        HideAllPanels();
        ChapterSelectPanel.SetActive(true);
    }

    // ★重要：チャプターボタンから呼ばれる関数
    // UnityのInspectorで数字(0, 1, 2...)を指定できるようにします
    public void OnClickChapter(int chapterId)
    {
        currentChapterId = chapterId;

        if (chapterId == 0)
        {
            // ID 0 (はじめに) の場合は、セクション選択をスキップして即再生
            // ※ここで「紙芝居システムの動画モード」を開始する処理を呼びます
            Debug.Log("はじめに（動画）を再生します");
            StartGame();
        }
        else
        {
            // それ以外 (1, 2, 3...) の場合は、セクション選択画面へ
            GoToSectionSelect(chapterId);
        }
    }

    public void GoToSectionSelect(int chapterId)
    {
        HideAllPanels();
        SectionSelectPanel.SetActive(true);

        // ★修正ポイント：if文をやめて、リストから文字を取り出す
        // 配列の長さチェック（エラー防止）
        if (chapterId < ChapterTitles.Length)
        {
            SectionTitleText.text = ChapterTitles[chapterId];
        }
        else
        {
            Debug.LogWarning("タイトルの設定が足りません！ ID: " + chapterId);
        }
        
    }

    public void StartGame()
    {
        HideAllPanels();
        GamePanel.SetActive(true);
        // ここで currentChapterId に基づいたデータを読み込みます
    }

    // 全パネルを一旦隠す便利関数
    private void HideAllPanels()
    {
        TitlePanel.SetActive(false);
        ChapterSelectPanel.SetActive(false);
        SectionSelectPanel.SetActive(false);
        GamePanel.SetActive(false);
    }
}