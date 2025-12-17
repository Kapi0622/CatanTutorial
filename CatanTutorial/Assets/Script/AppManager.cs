using UnityEngine;
using TMPro;

public class AppManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject TitlePanel;        // タイトル画面
    public GameObject ChapterSelectPanel;// チャプター選択画面
    public GameObject SectionSelectPanel;// セクション選択画面
    public GameObject GamePanel;         // ゲーム画面（紙芝居）
    public GameObject InGameMenuPanel;

    [Header("Dynamic UI")]
    public TextMeshProUGUI SectionTitleText; // セクション画面のタイトル
    public string[] ChapterTitles;           // 章タイトルのリスト（Inspectorで入力）

    [Header("Game System")]
    public ScenarioPlayer scenarioPlayer;    // 紙芝居再生機
    public ScenarioData[] scenarioList;      // シナリオデータのリスト
    
    [Header("State")]
    // ★追加：現在選択中の章IDを記憶しておく変数
    private int currentChapterId = 0;

    // 起動時
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
    
    // ■ 追加：メニューの開閉
    public void OpenGameMenu()
    {
        InGameMenuPanel.SetActive(true);
        // ゲーム進行を一時停止したい場合はここでTime.timeScale = 0;など
    }

    public void CloseGameMenu()
    {
        InGameMenuPanel.SetActive(false);
    }

    // ■ 追加：セクション選択に戻る（記憶したIDを使う）
    public void OnClickBackToSection()
    {
        CloseGameMenu(); // メニューを閉じてから
        GoToSectionSelect(currentChapterId); // 記憶していた章の画面に戻る
    }
    

    // ★チャプターボタンから呼ばれる関数
    public void OnClickChapter(int chapterId)
    {
        if (chapterId == 0)
        {
            // ID 0 (はじめに) の場合は、セクション選択をスキップして即再生
            Debug.Log("はじめに（導入シナリオ）を再生します");
            
            // ★修正ポイント：ここにも引数(0)を入れてエラーを解消！
            // シナリオリストの0番目（はじめに用のデータ）を再生すると仮定します
            StartGame(0); 
        }
        else
        {
            // それ以外 (1, 2, 3...) の場合は、セクション選択画面へ
            GoToSectionSelect(chapterId);
        }
    }

    public void GoToSectionSelect(int chapterId)
    {
        currentChapterId = chapterId;
        
        HideAllPanels();
        SectionSelectPanel.SetActive(true);

        // タイトル書き換え（エラー防止付き）
        if (chapterId < ChapterTitles.Length)
        {
            SectionTitleText.text = ChapterTitles[chapterId];
        }
        else
        {
            SectionTitleText.text = "章タイトル未設定";
        }
    }

    // ■ 追加：章選択に戻る
    public void OnClickBackToChapter()
    {
        CloseGameMenu();
        GoToChapterSelect();
    }

    // --- ゲーム開始系 ---

    // ★引数(scenarioIndex)を受け取るように変更済み
    public void StartGame(int scenarioIndex)
    {
        HideAllPanels();
        GamePanel.SetActive(true);

        // 指定された番号のデータがあるかチェックして再生
        if (scenarioIndex < scenarioList.Length && scenarioList[scenarioIndex] != null)
        {
            scenarioPlayer.StartScenario(scenarioList[scenarioIndex]);
        }
        else
        {
            Debug.LogError("シナリオデータが見つかりません！ Index: " + scenarioIndex);
        }
    }

    // 全パネルを一旦隠す便利関数
    private void HideAllPanels()
    {
        if(TitlePanel) TitlePanel.SetActive(false);
        if(ChapterSelectPanel) ChapterSelectPanel.SetActive(false);
        if(SectionSelectPanel) SectionSelectPanel.SetActive(false);
        if(GamePanel) GamePanel.SetActive(false);
    }
}