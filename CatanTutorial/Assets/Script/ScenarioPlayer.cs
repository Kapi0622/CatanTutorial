using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // コルーチン用

public class ScenarioPlayer : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI HeaderTitleText; 
    public Image BgImage;           
    public Image SlideImage;        
    public Image CharacterImage;    
    public TextMeshProUGUI MessageText; 
    
    [Header("Audio")]
    public AudioSource VoiceSource; 
    public AudioSource SeSource;    

    [Header("Navigation Buttons")]
    public Button NextButton;       
    public Button PrevButton;       

    [Header("Settings")]
    public float TypeSpeed = 0.05f; // 1文字表示するのにかかる時間（秒）

    [Header("Data (Debug)")]
    public ScenarioData currentScenario; 
    public int currentStepIndex = 0;     

    private Coroutine typingCoroutine; // 現在動いている文字送り処理

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

        // 1. テキスト更新（タイプライター演出）
        if(MessageText)
        {
            // 前の文字送りが動いていたら止める
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            
            // 新しい文字送りを開始
            typingCoroutine = StartCoroutine(TypeWriterEffect(step.MainText));
        }

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

        // 5. ボタン表示制御
        if (PrevButton) PrevButton.gameObject.SetActive(currentStepIndex > 0);

        if (NextButton)
        {
            bool isLastPage = (currentStepIndex >= currentScenario.Steps.Count - 1);
            NextButton.gameObject.SetActive(!isLastPage);
        }
    }

    // ★追加: 1文字ずつ表示するコルーチン
    IEnumerator TypeWriterEffect(string fullText)
    {
        MessageText.text = ""; // いったん空にする

        foreach (char c in fullText)
        {
            MessageText.text += c; // 1文字足す
            yield return new WaitForSeconds(TypeSpeed); // 少し待つ
        }

        typingCoroutine = null; // 完了したら空にする
    }

    public void OnClickNext()
    {
        // もし文字送り中だったら、一瞬で全文表示して止める（スキップ機能）
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            
            // 全文を表示
            if (currentScenario != null && currentScenario.Steps.Count > 0)
            {
                MessageText.text = currentScenario.Steps[currentStepIndex].MainText;
            }
            return; // ここで処理を終わる（次のページには行かない）
        }

        // 最後のページなら何もしない
        if (currentScenario != null && currentStepIndex >= currentScenario.Steps.Count - 1)
        {
            return;
        }

        // 次のページへ
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