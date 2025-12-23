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
}

public class AppManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject TitlePanel;
    public GameObject ChapterSelectPanel;
    public GameObject SectionSelectPanel;
    public GameObject GamePanel;
    public GameObject InGameMenuPanel;
    
    [Header("Video System")]
    public GameObject VideoPanel; 
    public VideoPlayer OpeningVideoPlayer;
    public RawImage VideoScreen; // 動画を映すUI

    [Header("Dynamic UI")]
    public TextMeshProUGUI SectionTitleText;
    
    public List<ChapterData> Chapters; 
    
    public Button[] SectionButtons; 

    [Header("Game System")]
    public ScenarioPlayer scenarioPlayer;

    private int currentChapterId = 0; 

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

        // 1. タイトル書き換え
        if (chapterId < Chapters.Count)
        {
            SectionTitleText.text = Chapters[chapterId].ChapterName;
        }

        // 2. ★重要: ボタンの中身を動的に書き換える
        // その章にシナリオがいくつあるか確認
        int scenarioCount = 0;
        if (chapterId < Chapters.Count)
        {
            scenarioCount = Chapters[chapterId].Scenarios.Count;
        }

        // ボタンの数だけループして設定
        for (int i = 0; i < SectionButtons.Length; i++)
        {
            // データが存在するならボタンを表示して機能を割り当て
            if (i < scenarioCount)
            {
                SectionButtons[i].gameObject.SetActive(true);
                
                // ボタンのテキストをシナリオタイトルに変える（もしボタン内にTextがあれば）
                TextMeshProUGUI btnText = SectionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = Chapters[chapterId].Scenarios[i].ScenarioTitle;
                }

                // クリックイベントの再登録（ここが味噌！）
                int targetSectionIndex = i; // 一時変数が必要
                SectionButtons[i].onClick.RemoveAllListeners();
                SectionButtons[i].onClick.AddListener(() => StartGame(chapterId, targetSectionIndex));
            }
            else
            {
                // データがないボタンは隠す
                SectionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void StartGame(int chapterId, int sectionIndex)
    {
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
    }
}