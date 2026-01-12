using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic;

// 章ごとのデータセット
[System.Serializable]
public class ChapterData
{
    public string ChapterName;          // 章の名前
    public List<ScenarioData> Scenarios; // シナリオ一覧
    
    [Header("UI素材")]
    public Sprite SectionButtonSprite; // ボタン背景
    public Sprite BackButtonSprite;    // 戻るボタン
    public Sprite DecorationSprite;    // 装飾画像
}

[System.Serializable]
public class ChapterButtonUI
{
    public GameObject ButtonObj;         // 章選択ボタンそのもの
    public TextMeshProUGUI ProgressText; // "1/3" のテキスト
    public Image GaugeFillImage;         // ゲージの中身（Filled）
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
    public GameObject ClearPanel;      // ★追加: クリア画面

    [Header("Video System")]
    public GameObject VideoPanel; 
    public VideoPlayer OpeningVideoPlayer;
    public RawImage VideoScreen;

    [Header("Dynamic UI")]
    public TextMeshProUGUI SectionTitleText;
    public Image BackButtonImage;
    public Image DecorationImage;
    
    public List<ChapterData> Chapters; 
    public Button[] SectionButtons; 
    
    [Header("Chapter Select UI")]
    // Inspectorで章ごとのボタンとUIを登録するリスト
    public List<ChapterButtonUI> ChapterButtonsUI;
    
    [Header("Shared UI")]
    // ★追加: 全章で共有している背景Imageをここにも登録する
    public Image SharedBoardImage; 
    // ★追加: リセット時に表示する画像（「何もない海」や「空の机」など）。無ければNoneでOK
    public Sprite DefaultBoardSprite;

    [Header("Game System")]
    public ScenarioPlayer scenarioPlayer;

    [Header("Clear Screen Controls")]
    public Button BtnClearTitle;       // タイトルへ
    public Button BtnClearRetry;       // リトライ
    public Button BtnClearNext;        // 次のお題

    private int currentChapterId = 0; 
    private int currentSectionIndex = 0;
    
    [Header("Audio Settings")]
    public AudioSource GlobalSeSource; // SEを鳴らすスピーカー
    public AudioClip ButtonClickClip;  // 鳴らしたい音データ(mp3など)

    void Start()
    {
        ShowTitle();
    }

    // =================================================================
    // 画面遷移・メニュー関連
    // =================================================================

    public void ShowTitle()
    {
        HideAllPanels();
        TitlePanel.SetActive(true);
    }
    
    // セクションクリア時に呼び出す保存処理
    public void MarkSectionCleared(int chapterId, int sectionIndex)
    {
        // キーを作成 (例: "Clear_Ch1_Sec0")
        string key = $"Clear_Ch{chapterId}_Sec{sectionIndex}";
        
        // まだクリアしていない場合のみ保存
        if (PlayerPrefs.GetInt(key, 0) == 0)
        {
            PlayerPrefs.SetInt(key, 1); // 1 = クリア済み
            PlayerPrefs.Save(); // データを確定
            Debug.Log($"進捗保存: Chapter {chapterId}, Section {sectionIndex} をクリアしました。");
        }
    }

    // 章選択画面を開くときにUIを更新する処理
    public void UpdateChapterProgressUI()
    {
        // 登録されている章ボタンの数だけループ
        for (int i = 0; i < ChapterButtonsUI.Count; i++)
        {
            // データリスト(Chapters)とUIリスト(ChapterButtonsUI)のインデックスを合わせる前提
            // ChapterID は 1 から始まるが、リストは 0 からなので調整が必要
            // ここでは「ChapterButtonsUI[0]」が「第1章(ID=1)」に対応すると仮定します。
            
            // はじめに(ID=0) は進捗がないのでスキップ、または別途対応
            // ここではシンプルに i=0 -> ChapterID=1, i=1 -> ChapterID=2... とします。
            int targetChapterId = i + 1; 

            // データが存在するか確認
            if (targetChapterId < Chapters.Count) 
            {
                ChapterData data = Chapters[targetChapterId];
                int totalScenarios = data.Scenarios.Count;
                int clearedCount = 0;

                // クリア数をカウント
                for (int s = 0; s < totalScenarios; s++)
                {
                    string key = $"Clear_Ch{targetChapterId}_Sec{s}";
                    if (PlayerPrefs.GetInt(key, 0) == 1)
                    {
                        clearedCount++;
                    }
                }

                // UI反映
                var ui = ChapterButtonsUI[i];
                if (ui.ProgressText)
                {
                    ui.ProgressText.text = $"{clearedCount}/{totalScenarios}";
                }
                if (ui.GaugeFillImage)
                {
                    // 0除算防止
                    float fill = (totalScenarios > 0) ? (float)clearedCount / totalScenarios : 0f;
                    ui.GaugeFillImage.fillAmount = fill;
                }
            }
        }
    }

    public void GoToChapterSelect()
    {
        if (scenarioPlayer != null) scenarioPlayer.StopAllAudio();
        
        HideAllPanels();
        ChapterSelectPanel.SetActive(true);
        
        UpdateChapterProgressUI();
    }

    public void OnClickChapter(int chapterId)
    {
        currentChapterId = chapterId;

        // ID 0 (はじめに) は動画再生へ
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

        if (chapterId >= Chapters.Count) return;

        ChapterData currentChapter = Chapters[chapterId];

        // UIの動的書き換え
        if (SectionTitleText) SectionTitleText.text = currentChapter.ChapterName;
        if (BackButtonImage && currentChapter.BackButtonSprite) BackButtonImage.sprite = currentChapter.BackButtonSprite;
        if (DecorationImage && currentChapter.DecorationSprite) DecorationImage.sprite = currentChapter.DecorationSprite;

        // セクションボタンの設定
        int scenarioCount = currentChapter.Scenarios.Count;
        for (int i = 0; i < SectionButtons.Length; i++)
        {
            if (i < scenarioCount)
            {
                SectionButtons[i].gameObject.SetActive(true);

                // 画像差し替え
                Image btnImage = SectionButtons[i].GetComponent<Image>();
                if (btnImage && currentChapter.SectionButtonSprite)
                {
                    btnImage.sprite = currentChapter.SectionButtonSprite;
                }
                
                // テキスト設定
                TextMeshProUGUI btnText = SectionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText) btnText.text = currentChapter.Scenarios[i].ScenarioTitle;
                
                // クリックイベント設定
                int targetSectionIndex = i;
                SectionButtons[i].onClick.RemoveAllListeners();
                SectionButtons[i].onClick.AddListener(() => StartGame(chapterId, targetSectionIndex));

                // ★追加: クリア済みなら「合格スタンプ」を表示する
                // ボタンの中にある "ClearMark" という名前の画像を探す
                Transform markTrans = SectionButtons[i].transform.Find("ClearMark");
                if (markTrans != null)
                {
                    // 保存されたデータをチェック (キー: Clear_ChX_SecY)
                    string key = $"Clear_Ch{chapterId}_Sec{i}";
                    bool isCleared = (PlayerPrefs.GetInt(key, 0) == 1);

                    // クリア済みなら表示、そうでなければ非表示
                    markTrans.gameObject.SetActive(isCleared);
                }
            }
            else
            {
                SectionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // =================================================================
    // ゲーム（シナリオ）パート
    // =================================================================

    public void StartGame(int chapterId, int sectionIndex)
    {
        currentChapterId = chapterId;
        currentSectionIndex = sectionIndex;
        
        HideAllPanels();
        GamePanel.SetActive(true);

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
    
    // =================================================================
    // 実践パート (Practice)
    // =================================================================

    // シナリオ画面の「実践へ」ボタンなどから呼ばれる
    public void GoToPractice()
    {
        if (scenarioPlayer != null) scenarioPlayer.StopAllAudio();
        StartPractice(currentChapterId, currentSectionIndex);
    }

    // 指定された章・セクションの実践を開始するメイン処理
    public void StartPractice(int chapterId, int sectionIndex)
    {
        currentChapterId = chapterId;
        currentSectionIndex = sectionIndex;

        // パネル切り替え
        HideAllPanels(); // 一旦全部消して
        PracticePanel.SetActive(true); // 実践パネルだけ出す
        
        // クリア画面が残っていたら消す
        if (ClearPanel) ClearPanel.SetActive(false);
        
        if (SharedBoardImage != null)
        {
            // デフォルト画像があればそれをセット、なければ null（透明/白）にする
            SharedBoardImage.sprite = DefaultBoardSprite;
            
            // もし画像がnullなら、白い四角が表示されないように色を透明にする処理を入れても良いですが、
            // 基本的には「DefaultBoardSprite」に「空の盤面」などを設定することを推奨します。
            if (SharedBoardImage.sprite == null) 
            {
                // Spriteがnullだと白い四角が出るため、一時的に透明にするなどの工夫が必要
                // ここではシンプルにSpriteをnullにするだけに留めます
            }
        }
        
        // ★追加: 他の章の残留物を消す（リセット処理）
        ResetAllPractices();

        // 各章マネージャーへの分岐
        if (chapterId == 1)
        {
            var manager = GetComponent<Chapter1Manager>();
            if (manager) manager.StartPractice(sectionIndex);
        }
        else if (chapterId == 2)
        {
            var manager = GetComponent<Chapter2Manager>();
            if (manager) manager.StartPractice(sectionIndex);
        }
        else if (chapterId == 3)
        {
            var manager = GetComponent<Chapter3Manager>();
            if (manager) manager.StartPractice(sectionIndex);
        }
        else if (chapterId == 4)
        {
            var manager = GetComponent<Chapter4Manager>();
            if (manager) manager.StartPractice(sectionIndex);
        }
        else if (chapterId == 5) // Assemble (組み立て)
        {
            var manager = GetComponent<AssembleManager>();
            if (manager) manager.StartPractice();
        }
    }
    
    // ★追加: 全ての章の実践状態をリセット（非表示）にする関数
    void ResetAllPractices()
    {
        // 第1~4章は StartPractice(-1) を呼ぶことで、コンテナ非表示＆コルーチン停止を行う
        // （各ManagerのStartPracticeは冒頭でSetActive(false)しているため、不正なindexを渡せばリセットとして機能します）
        
        var ch1 = GetComponent<Chapter1Manager>();
        if (ch1) ch1.StartPractice(-1);

        var ch2 = GetComponent<Chapter2Manager>();
        if (ch2) ch2.StartPractice(-1);

        var ch3 = GetComponent<Chapter3Manager>();
        if (ch3) ch3.StartPractice(-1);

        var ch4 = GetComponent<Chapter4Manager>();
        if (ch4) ch4.StartPractice(-1);

        // 第5章（Assemble）は引数なしのStartPracticeしかないため、手動で切る
        var ch5 = GetComponent<AssembleManager>();
        if (ch5) 
        {
            ch5.StopAllCoroutines();
            if(ch5.AssembleContainer) ch5.AssembleContainer.SetActive(false);
            if(ch5.ActionButton) ch5.ActionButton.gameObject.SetActive(false);
        }
    }

    public void BackToLearning()
    {
        PracticePanel.SetActive(false);
        GamePanel.SetActive(true);
    }

    // =================================================================
    // クリア画面 (Clear Screen)
    // =================================================================

    // 各Managerの終了時にこれを呼ぶ
    public void ShowClearPanel()
    {
        MarkSectionCleared(currentChapterId, currentSectionIndex);
        
        if (ClearPanel) ClearPanel.SetActive(true);
        
        // ボタンイベント登録
        if(BtnClearTitle) 
        {
            BtnClearTitle.onClick.RemoveAllListeners();
            BtnClearTitle.onClick.AddListener(OnClickClearTitle);
        }
        if(BtnClearRetry) 
        {
            BtnClearRetry.onClick.RemoveAllListeners();
            BtnClearRetry.onClick.AddListener(OnClickClearRetry);
        }
        if(BtnClearNext) 
        {
            BtnClearNext.onClick.RemoveAllListeners();
            BtnClearNext.onClick.AddListener(OnClickClearNext);
            BtnClearNext.gameObject.SetActive(true); 
        }
    }

    // タイトルへ（セクション選択に戻る）
    void OnClickClearTitle()
    {
        if(ClearPanel) ClearPanel.SetActive(false);
        GoToSectionSelect(currentChapterId); 
    }

    // リトライ
    void OnClickClearRetry()
    {
        if(ClearPanel) ClearPanel.SetActive(false);
        StartPractice(currentChapterId, currentSectionIndex);
    }

    // 次のお題
    void OnClickClearNext()
    {
        if(ClearPanel) ClearPanel.SetActive(false);

        // 次のセクションのインデックスを計算
        int nextSectionIndex = currentSectionIndex + 1;
        
        // 次があるか確認
        if (currentChapterId < Chapters.Count && nextSectionIndex < Chapters[currentChapterId].Scenarios.Count)
        {
            // ★修正: 実践(StartPractice)ではなく、学習パート(StartGame)を開始する
            StartGame(currentChapterId, nextSectionIndex);
        }
        else
        {
            // 次がない場合は選択画面へ
            Debug.Log("次のセクションはありません。");
            GoToSectionSelect(currentChapterId);
        }
    }

    // =================================================================
    // 動画関連
    // =================================================================

    private void PlayOpeningVideo()
    {
        HideAllPanels();
        VideoPanel.SetActive(true);

        OpeningVideoPlayer.prepareCompleted += (source) =>
        {
            VideoScreen.texture = source.texture;
            source.Play();
        };

        OpeningVideoPlayer.loopPointReached += OnVideoEnd;
        OpeningVideoPlayer.Prepare();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoEnd;
        VideoPanel.SetActive(false);
        GoToChapterSelect();
    }
    
    public void OnClickSkipVideo()
    {
        if (OpeningVideoPlayer.isPlaying) OpeningVideoPlayer.Stop();
        OpeningVideoPlayer.loopPointReached -= OnVideoEnd;
        VideoPanel.SetActive(false);
        GoToChapterSelect();
    }

    // =================================================================
    // メニュー・その他
    // =================================================================

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
        if(ClearPanel) ClearPanel.SetActive(false); // ★追加
    }
    
    public void PlayClickSE()
    {
        if (GlobalSeSource != null && ButtonClickClip != null)
        {
            GlobalSeSource.PlayOneShot(ButtonClickClip);
        }
    }
    
    public void PlayCustomSE(AudioClip clip)
    {
        if (GlobalSeSource != null && clip != null)
        {
            GlobalSeSource.PlayOneShot(clip);
        }
    }
}