using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScenarioPlayer : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI HeaderTitleText; // ヘッダーのタイトル文字
    public Image BgImage;           // 背景
    public Image SlideImage;        // 左側のメイン画像
    public Image CharacterImage;    // キャラクター
    public TextMeshProUGUI MessageText; // セリフ文字
    
    [Header("Audio")]
    public AudioSource VoiceSource; // ボイス用スピーカー
    public AudioSource SeSource;    // 効果音用スピーカー

    [Header("Navigation Buttons")]
    public Button NextButton;       // 「次へ」ボタン
    public Button PrevButton;       // 「戻る」ボタン

    [Header("Data (Debug)")]
    public ScenarioData currentScenario; // 現在再生中のデータ
    public int currentStepIndex = 0;     // 今何枚目か

    // AppManagerから呼ばれる
    public void StartScenario(ScenarioData data)
    {
        if (data == null)
        {
            Debug.LogError("エラー：受け取ったシナリオデータが空(null)です！");
            return;
        }

        currentScenario = data;
        currentStepIndex = 0;

        // ヘッダーのタイトルを更新
        if (HeaderTitleText != null)
        {
            if (!string.IsNullOrEmpty(data.ScenarioTitle))
            {
                HeaderTitleText.text = data.ScenarioTitle;
            }
            else
            {
                HeaderTitleText.text = data.name; 
            }
        }

        // ボタンの監視を開始
        if (NextButton)
        {
            NextButton.onClick.RemoveAllListeners();
            NextButton.onClick.AddListener(OnClickNext);
        }

        if (PrevButton)
        {
            PrevButton.onClick.RemoveAllListeners();
            PrevButton.onClick.AddListener(OnClickPrev);
        }
        
        // 最初の1枚目を表示
        ShowStep();
    }

    // 画面更新処理
    void ShowStep()
    {
        if (currentScenario == null || currentScenario.Steps.Count == 0) return;

        ScenarioStep step = currentScenario.Steps[currentStepIndex];

        // 1. テキスト更新
        if(MessageText) MessageText.text = step.MainText;

        // 2. 画像更新
        if (step.BgImage != null && BgImage) BgImage.sprite = step.BgImage;
        if (step.CenterImage != null && SlideImage) SlideImage.sprite = step.CenterImage;

        // 3. ボイス再生処理
        if (VoiceSource)
        {
            if (step.VoiceClip != null)
            {
                if (VoiceSource.clip != step.VoiceClip)
                {
                    VoiceSource.clip = step.VoiceClip; 
                    VoiceSource.Play();              
                }
            }
            else
            {
                VoiceSource.Stop(); 
            }
        }

        // 4. 効果音(SE)再生処理
        if (SeSource && step.SeClip != null)
        {
            SeSource.PlayOneShot(step.SeClip); 
        }

        // 5. ボタン表示制御 ★修正箇所
        // 最初のページなら「戻る」を隠す
        if (PrevButton) PrevButton.gameObject.SetActive(currentStepIndex > 0);

        // 最後のページなら「次へ」を隠す
        if (NextButton)
        {
            bool isLastPage = (currentStepIndex >= currentScenario.Steps.Count - 1);
            NextButton.gameObject.SetActive(!isLastPage);
        }
    }

    public void OnClickNext()
    {
        // 最後のページなら何もしない（ボタンが消えているはずだが念のため）
        if (currentStepIndex >= currentScenario.Steps.Count - 1)
        {
            return;
        }
        currentStepIndex++;
        ShowStep();
    }

    public void OnClickPrev()
    {
        if (currentStepIndex > 0)
        {
            currentStepIndex--;
            ShowStep();
        }
    }
    
    public void StopAllAudio()
    {
        if (VoiceSource != null) VoiceSource.Stop();
        if (SeSource != null) SeSource.Stop();
    }
}