using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System; 

// 章ごとのデータセット
[System.Serializable]
public class ChapterData
{
    public string ChapterName;          
    public List<ScenarioData> Scenarios; 
    
    [Header("UI素材")]
    public Sprite SectionButtonSprite; 
    public Sprite BackButtonSprite;    
    public Sprite DecorationSprite;    
}

// 章ボタンUI（進捗表示用）
[System.Serializable]
public class ChapterButtonUI
{
    public int TargetChapterID;          
    public GameObject ButtonObj;         
    public TextMeshProUGUI ProgressText; 
    public Image GaugeFillImage;         
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
    public GameObject ClearPanel;      

    [Header("Video System")]
    public GameObject VideoPanel; 
    public VideoPlayer OpeningVideoPlayer;
    public RawImage VideoScreen;

    [Header("Dynamic UI")]
    public TextMeshProUGUI SectionTitleText;
    public Image BackButtonImage;
    public Image DecorationImage;
    
    public List<ChapterData> Chapters; 

    // ★変更: 配列をやめて、プレハブと生成場所（親）に変更
    [Header("Section Button System")]
    public GameObject SectionButtonPrefab; // ボタンの元（プレハブ）
    public Transform SectionButtonContainer; // ボタンを並べる親（Contentなど）

    [Header("Chapter Select UI")]
    public List<ChapterButtonUI> ChapterButtonsUI; 

    [Header("Game System")]
    public ScenarioPlayer scenarioPlayer;

    [Header("Clear Screen Controls")]
    public Button BtnClearTitle;       
    public Button BtnClearRetry;       
    public Button BtnClearNext;        

    [Header("Audio Settings")]
    public AudioSource GlobalSeSource; 
    public AudioClip ButtonClickClip;  
    
    [Header("BGM Settings")]
    public AudioSource GlobalBgmSource; 
    public AudioClip TitleBgm;          
    public AudioClip LearnBgm;          
    public AudioClip PracticeBgm;       
    public AudioClip ClearBgm;          

    [Header("Shared UI")]
    public Image SharedBoardImage; 
    public Sprite DefaultBoardSprite; 

    [Header("Fade System")]
    public CanvasGroup FadeCanvasGroup; 
    public float FadeDuration = 0.5f;   

    private int currentChapterId = 0; 
    private int currentSectionIndex = 0;

    void Start()
    {
        ShowTitle();
    }

    // =================================================================
    // オーディオ
    // =================================================================
    public void PlayClickSE()
    {
        if (GlobalSeSource != null && ButtonClickClip != null) GlobalSeSource.PlayOneShot(ButtonClickClip);
    }

    public void PlayCustomSE(AudioClip clip)
    {
        if (GlobalSeSource != null && clip != null) GlobalSeSource.PlayOneShot(clip);
    }
    
    public void PlayBGM(AudioClip clip)
    {
        if (GlobalBgmSource == null) return;
        if (clip == null) { GlobalBgmSource.Stop(); return; }
        if (GlobalBgmSource.clip == clip && GlobalBgmSource.isPlaying) return;

        GlobalBgmSource.Stop();
        GlobalBgmSource.clip = clip;
        GlobalBgmSource.Play();
    }

    // =================================================================
    // 進捗管理
    // =================================================================
    public void MarkSectionCleared(int chapterId, int sectionIndex)
    {
        string key = $"Clear_Ch{chapterId}_Sec{sectionIndex}";
        if (PlayerPrefs.GetInt(key, 0) == 0)
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }
    }

    public void UpdateChapterProgressUI()
    {
        for (int i = 0; i < ChapterButtonsUI.Count; i++)
        {
            var ui = ChapterButtonsUI[i];
            int targetChapterId = ui.TargetChapterID;

            if (targetChapterId < Chapters.Count) 
            {
                ChapterData data = Chapters[targetChapterId];
                int totalScenarios = data.Scenarios.Count;
                int clearedCount = 0;

                for (int s = 0; s < totalScenarios; s++)
                {
                    string key = $"Clear_Ch{targetChapterId}_Sec{s}";
                    if (PlayerPrefs.GetInt(key, 0) == 1) clearedCount++;
                }

                if (ui.ProgressText) ui.ProgressText.text = $"{clearedCount}/{totalScenarios}";
                if (ui.GaugeFillImage) ui.GaugeFillImage.fillAmount = (totalScenarios > 0) ? (float)clearedCount / totalScenarios : 0f;
            }
        }
    }

    // =================================================================
    // 画面遷移
    // =================================================================

    public void ShowTitle()
    {
        HideAllPanels();
        TitlePanel.SetActive(true);
        PlayBGM(TitleBgm);
    }

    public void GoToChapterSelect()
    {
        if (scenarioPlayer != null) scenarioPlayer.StopAllAudio();
        PlayBGM(TitleBgm);
        HideAllPanels();
        ChapterSelectPanel.SetActive(true);
        UpdateChapterProgressUI(); 
    }

    public void OnClickChapter(int chapterId)
    {
        currentChapterId = chapterId;
        if (chapterId == 0) PlayOpeningVideo();
        else FadeAndSwitch(() => GoToSectionSelect(chapterId));
    }

    // ★重要: セクション選択画面（生成方式に変更）
    public void GoToSectionSelect(int chapterId)
    {
        HideAllPanels();
        SectionSelectPanel.SetActive(true);

        if (chapterId >= Chapters.Count) return;

        ChapterData currentChapter = Chapters[chapterId];

        // UI情報の更新
        if (SectionTitleText) SectionTitleText.text = currentChapter.ChapterName;
        if (BackButtonImage && currentChapter.BackButtonSprite) BackButtonImage.sprite = currentChapter.BackButtonSprite;
        if (DecorationImage && currentChapter.DecorationSprite) DecorationImage.sprite = currentChapter.DecorationSprite;

        PlayBGM(TitleBgm); 

        // 1. 古いボタンを全て削除する（お掃除）
        // Containerの中に残っている子供（ボタン）を全消去します
        foreach (Transform child in SectionButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. 新しいボタンを必要な数だけ生成する
        int scenarioCount = currentChapter.Scenarios.Count;
        for (int i = 0; i < scenarioCount; i++)
        {
            // プレハブから新品のボタンを作る
            GameObject newBtnObj = Instantiate(SectionButtonPrefab, SectionButtonContainer);
            
            // ボタンの設定
            Button btn = newBtnObj.GetComponent<Button>();
            Image btnImage = newBtnObj.GetComponent<Image>();
            
            if (btnImage && currentChapter.SectionButtonSprite) btnImage.sprite = currentChapter.SectionButtonSprite;
            
            TextMeshProUGUI btnText = newBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText) btnText.text = currentChapter.Scenarios[i].ScenarioTitle;

            // クリックイベント設定
            int targetSectionIndex = i;
            if(btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => FadeAndSwitch(() => StartGame(chapterId, targetSectionIndex)));
            }

            // スタンプ設定
            Transform markTrans = newBtnObj.transform.Find("ClearMark");
            if (markTrans != null)
            {
                string key = $"Clear_Ch{chapterId}_Sec{i}";
                markTrans.gameObject.SetActive(PlayerPrefs.GetInt(key, 0) == 1);
            }
        }
    }

    // =================================================================
    // ゲーム開始・実践・クリアなど
    // =================================================================

    public void StartGame(int chapterId, int sectionIndex)
    {
        currentChapterId = chapterId;
        currentSectionIndex = sectionIndex;
        HideAllPanels();
        GamePanel.SetActive(true);
        PlayBGM(LearnBgm);

        if (chapterId < Chapters.Count && sectionIndex < Chapters[chapterId].Scenarios.Count)
        {
            scenarioPlayer.StartScenario(Chapters[chapterId].Scenarios[sectionIndex]);
        }
    }
    
    public void GoToPractice()
    {
        FadeAndSwitch(() => 
        {
            if (scenarioPlayer != null) scenarioPlayer.StopAllAudio();
            StartPractice(currentChapterId, currentSectionIndex);
        });
    }

    public void StartPractice(int chapterId, int sectionIndex)
    {
        currentChapterId = chapterId;
        currentSectionIndex = sectionIndex;

        HideAllPanels(); 
        PracticePanel.SetActive(true); 
        PlayBGM(PracticeBgm);
        if (ClearPanel) ClearPanel.SetActive(false);

        if (SharedBoardImage != null) SharedBoardImage.sprite = DefaultBoardSprite;
        ResetAllPractices();

        if (chapterId == 1) { var m = GetComponent<Chapter1Manager>(); if(m) m.StartPractice(sectionIndex); }
        else if (chapterId == 2) { var m = GetComponent<Chapter2Manager>(); if(m) m.StartPractice(sectionIndex); }
        else if (chapterId == 3) { var m = GetComponent<Chapter3Manager>(); if(m) m.StartPractice(sectionIndex); }
        else if (chapterId == 4) { var m = GetComponent<Chapter4Manager>(); if(m) m.StartPractice(sectionIndex); }
        else if (chapterId == 5) { var m = GetComponent<AssembleManager>(); if(m) m.StartPractice(); }
    }

    void ResetAllPractices()
    {
        var ch1 = GetComponent<Chapter1Manager>(); if (ch1) ch1.StartPractice(-1);
        var ch2 = GetComponent<Chapter2Manager>(); if (ch2) ch2.StartPractice(-1);
        var ch3 = GetComponent<Chapter3Manager>(); if (ch3) ch3.StartPractice(-1);
        var ch4 = GetComponent<Chapter4Manager>(); if (ch4) ch4.StartPractice(-1);
        var ch5 = GetComponent<AssembleManager>(); 
        if (ch5) {
            ch5.StopAllCoroutines();
            if(ch5.AssembleContainer) ch5.AssembleContainer.SetActive(false);
            if(ch5.ActionButton) ch5.ActionButton.gameObject.SetActive(false);
        }
    }

    public void BackToLearning()
    {
        FadeAndSwitch(() => { PracticePanel.SetActive(false); GamePanel.SetActive(true); });
    }

    public void ShowClearPanel()
    {
        MarkSectionCleared(currentChapterId, currentSectionIndex);
        if (ClearPanel) ClearPanel.SetActive(true);
        PlayBGM(ClearBgm);
        
        if(BtnClearTitle) { BtnClearTitle.onClick.RemoveAllListeners(); BtnClearTitle.onClick.AddListener(OnClickClearTitle); }
        if(BtnClearRetry) { BtnClearRetry.onClick.RemoveAllListeners(); BtnClearRetry.onClick.AddListener(OnClickClearRetry); }
        if(BtnClearNext)  { BtnClearNext.onClick.RemoveAllListeners(); BtnClearNext.onClick.AddListener(OnClickClearNext); BtnClearNext.gameObject.SetActive(true); }
    }

    void OnClickClearTitle() { FadeAndSwitch(() => { if(ClearPanel) ClearPanel.SetActive(false); GoToSectionSelect(currentChapterId); }); }
    void OnClickClearRetry() { FadeAndSwitch(() => { if(ClearPanel) ClearPanel.SetActive(false); StartPractice(currentChapterId, currentSectionIndex); }); }
    void OnClickClearNext()
    {
        if(ClearPanel) ClearPanel.SetActive(false);
        int nextSectionIndex = currentSectionIndex + 1;
        if (currentChapterId < Chapters.Count && nextSectionIndex < Chapters[currentChapterId].Scenarios.Count)
            FadeAndSwitch(() => StartGame(currentChapterId, nextSectionIndex));
        else
            FadeAndSwitch(() => GoToSectionSelect(currentChapterId));
    }

    // =================================================================
    // 動画・その他
    // =================================================================

    private void PlayOpeningVideo()
    {
        HideAllPanels();
        VideoPanel.SetActive(true);
        PlayBGM(null);

        OpeningVideoPlayer.prepareCompleted += (source) => { VideoScreen.texture = source.texture; source.Play(); };
        OpeningVideoPlayer.loopPointReached += OnVideoEnd;
        OpeningVideoPlayer.Prepare();
    }

    private void OnVideoEnd(VideoPlayer vp) { vp.loopPointReached -= OnVideoEnd; VideoPanel.SetActive(false); GoToChapterSelect(); }
    public void OnClickSkipVideo() { if (OpeningVideoPlayer.isPlaying) OpeningVideoPlayer.Stop(); OpeningVideoPlayer.loopPointReached -= OnVideoEnd; VideoPanel.SetActive(false); GoToChapterSelect(); }

    public void OpenGameMenu() { InGameMenuPanel.SetActive(true); }
    public void CloseGameMenu() { InGameMenuPanel.SetActive(false); }
    public void OnClickBackToSection() { CloseGameMenu(); FadeAndSwitch(() => GoToSectionSelect(currentChapterId)); }
    public void OnClickBackToChapter() { CloseGameMenu(); FadeAndSwitch(() => GoToChapterSelect()); }
    
    private void HideAllPanels()
    {
        if(TitlePanel) TitlePanel.SetActive(false);
        if(ChapterSelectPanel) ChapterSelectPanel.SetActive(false);
        if(SectionSelectPanel) SectionSelectPanel.SetActive(false);
        if(GamePanel) GamePanel.SetActive(false);
        if(InGameMenuPanel) InGameMenuPanel.SetActive(false);
        if(VideoPanel) VideoPanel.SetActive(false);
        if(PracticePanel) PracticePanel.SetActive(false);
        if(ClearPanel) ClearPanel.SetActive(false);
    }

    public void FadeAndSwitch(System.Action action) { StartCoroutine(CoFadeAndSwitch(action)); }
    IEnumerator CoFadeAndSwitch(System.Action action)
    {
        if (FadeCanvasGroup) FadeCanvasGroup.blocksRaycasts = true;
        float time = 0f;
        while (time < FadeDuration) { time += Time.deltaTime; if (FadeCanvasGroup) FadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / FadeDuration); yield return null; }
        if (FadeCanvasGroup) FadeCanvasGroup.alpha = 1f;
        action?.Invoke();
        yield return new WaitForSeconds(0.2f);
        time = 0f;
        while (time < FadeDuration) { time += Time.deltaTime; if (FadeCanvasGroup) FadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, time / FadeDuration); yield return null; }
        if (FadeCanvasGroup) FadeCanvasGroup.alpha = 0f;
        if (FadeCanvasGroup) FadeCanvasGroup.blocksRaycasts = false;
    }
    
    public void OnClickResetData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("【システム】セーブデータを全消去しました。");
        UpdateChapterProgressUI();
        PlayClickSE();
    }
}