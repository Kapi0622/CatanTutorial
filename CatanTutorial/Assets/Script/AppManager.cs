using UnityEngine;
using UnityEngine.UI; // ボタン操作用
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic; // リスト用

// 章ごとのデータセットを定義するクラス
[System.Serializable]
public class ChapterData
{
    public string ChapterName;          // 章の名前（メモ用）
    public List<ScenarioData> Scenarios; // その章に含まれるシナリオのリスト
    
    [Header("UI素材")]
    public Sprite SectionButtonSprite; // セクションボタンの背景画像
    public Sprite BackButtonSprite;    // 「戻る」ボタンの背景画像
    public Sprite DecorationSprite;    // ボタンの上の装飾画像（キャラなど）
}

public class AppManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject TitlePanel;
    public GameObject ChapterSelectPanel;
    public GameObject SectionSelectPanel;
    public GameObject GamePanel;
    public GameObject InGameMenuPanel;
    public GameObject PracticePanel;
    
    [Header("Video System")]
    public GameObject VideoPanel; 
    public VideoPlayer OpeningVideoPlayer;
    public RawImage VideoScreen; // 動画を映すUI

    [Header("Dynamic UI")]
    public TextMeshProUGUI SectionTitleText;
    public Image BackButtonImage;    // 戻るボタンのImage本体
    public Image DecorationImage;    // 装飾画像のImage本体
    
    public List<ChapterData> Chapters; 
    
    public Button[] SectionButtons; 

    [Header("Game System")]
    public ScenarioPlayer scenarioPlayer;

    private int currentChapterId = 0; 
    
    private int currentSectionIndex = 0;

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
        if (scenarioPlayer != null) scenarioPlayer.StopAllAudio();
        
        HideAllPanels();
        ChapterSelectPanel.SetActive(true);
    }

    public void OnClickChapter(int chapterId)
    {
        currentChapterId = chapterId;

        // ID 0 (はじめに) は特別扱い：その章の0番目のシナリオを即再生
        if (chapterId == 0)
        {
            PlayOpeningVideo();
        }
        else
        {
            GoToSectionSelect(chapterId);
        }
    }

    public void GoToSectionSelect(int chapterId)
    {
        HideAllPanels();
        SectionSelectPanel.SetActive(true);

        // データが存在しない場合は何もしない（エラー防止）
        if (chapterId >= Chapters.Count) return;

        // 現在の章データを取得
        ChapterData currentChapter = Chapters[chapterId];

        // 1. タイトル書き換え
        SectionTitleText.text = currentChapter.ChapterName;

        // 2. ★追加: 画像素材の動的差し替え
        // 戻るボタンと装飾画像を、その章のものに変更
        if (BackButtonImage != null) BackButtonImage.sprite = currentChapter.BackButtonSprite;
        if (DecorationImage != null) DecorationImage.sprite = currentChapter.DecorationSprite;

        // 3. ボタンの中身を動的に書き換える
        int scenarioCount = currentChapter.Scenarios.Count;
        for (int i = 0; i < SectionButtons.Length; i++)
        {
            if (i < scenarioCount)
            {
                SectionButtons[i].gameObject.SetActive(true);

                // ★追加: セクションボタンの背景画像を差し替える
                Image btnImage = SectionButtons[i].GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.sprite = currentChapter.SectionButtonSprite;
                }
                
                // (テキスト設定とクリックイベント登録はそのまま)
                TextMeshProUGUI btnText = SectionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = currentChapter.Scenarios[i].ScenarioTitle;
                int targetSectionIndex = i;
                SectionButtons[i].onClick.RemoveAllListeners();
                SectionButtons[i].onClick.AddListener(() => StartGame(chapterId, targetSectionIndex));
            }
            else
            {
                SectionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void StartGame(int chapterId, int sectionIndex)
    {
        currentChapterId = chapterId;
        currentSectionIndex = sectionIndex;
        
        HideAllPanels();
        GamePanel.SetActive(true);

        // 指定された章とセクションのデータを取り出す
        if (chapterId < Chapters.Count)
        {
            var scenarios = Chapters[chapterId].Scenarios;
            if (sectionIndex < scenarios.Count)
            {
                scenarioPlayer.StartScenario(scenarios[sectionIndex]);
                return;
            }
        }
        
        Debug.LogError($"データが見つかりません: Chapter {chapterId}, Section {sectionIndex}");
    }
    
    private void PlayOpeningVideo()
    {
        HideAllPanels();
        VideoPanel.SetActive(true);

        // RawImageに動画のテクスチャを流し込む準備
        OpeningVideoPlayer.prepareCompleted += (source) =>
        {
            VideoScreen.texture = source.texture;
            source.Play();
        };

        // 動画が終わった時の処理を登録
        OpeningVideoPlayer.loopPointReached += OnVideoEnd;

        // 再生準備＆開始
        OpeningVideoPlayer.Prepare();
    }

    // ★追加: 動画終了時の処理
    private void OnVideoEnd(VideoPlayer vp)
    {
        // イベント解除（重要）
        vp.loopPointReached -= OnVideoEnd;
        
        Debug.Log("動画終了。章選択に戻ります。");
        
        // 動画パネルを閉じて章選択へ戻る（またはゲーム画面へ行くなどお好みで）
        VideoPanel.SetActive(false);
        GoToChapterSelect();
    }
    
    public void OnClickSkipVideo()
    {
        // 1. もし再生中なら止める
        if (OpeningVideoPlayer.isPlaying)
        {
            OpeningVideoPlayer.Stop();
        }

        // 2. 「動画が終わった時」の監視イベントを解除する（重要！）
        // これを忘れると、次回再生時にバグる可能性があります
        OpeningVideoPlayer.loopPointReached -= OnVideoEnd;

        Debug.Log("動画をスキップしました");

        // 3. 動画画面を閉じて章選択へ
        VideoPanel.SetActive(false);
        GoToChapterSelect();
    }
    
    public void GoToPractice()
    {
        GamePanel.SetActive(false);
        PracticePanel.SetActive(true);
        
        if (currentChapterId == 1) // 第1章なら
        {
            // Chapter1Managerを取得して、セクション番号を渡して起動！
            var manager = GetComponent<Chapter1Manager>();
            if (manager != null)
            {
                manager.StartPractice(currentSectionIndex);
            }
        }
    }
    
    public void BackToLearning()
    {
        PracticePanel.SetActive(false);
        GamePanel.SetActive(true);
    }

    // --- メニュー系 (既存のまま) ---
    public void OpenGameMenu() { InGameMenuPanel.SetActive(true); }
    public void CloseGameMenu() { InGameMenuPanel.SetActive(false); }
    public void OnClickBackToSection() { CloseGameMenu(); GoToSectionSelect(currentChapterId); }
    public void OnClickBackToChapter() { CloseGameMenu(); GoToChapterSelect(); }
    
    private void HideAllPanels()
    {
        if(TitlePanel) TitlePanel.SetActive(false);
        if(ChapterSelectPanel) ChapterSelectPanel.SetActive(false);
        if(SectionSelectPanel) SectionSelectPanel.SetActive(false);
        if(GamePanel) GamePanel.SetActive(false);
        if(InGameMenuPanel) InGameMenuPanel.SetActive(false);
        if(VideoPanel) VideoPanel.SetActive(false);
        if(PracticePanel) PracticePanel.SetActive(false);
    }
}