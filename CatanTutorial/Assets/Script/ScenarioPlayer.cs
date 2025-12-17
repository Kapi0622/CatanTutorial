using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScenarioPlayer : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI HeaderTitleText; // ★追加：ヘッダーのタイトル文字
    public Image BgImage;           // 背景
    public Image SlideImage;        // 左側のメイン画像
    public Image CharacterImage;    // キャラクター
    public TextMeshProUGUI MessageText; // セリフ文字

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

        // ★追加：ヘッダーのタイトルを更新
        if (HeaderTitleText != null)
        {
            // データにタイトルが設定されていればそれを表示、なければファイル名を表示
            if (!string.IsNullOrEmpty(data.ScenarioTitle))
            {
                HeaderTitleText.text = data.ScenarioTitle;
            }
            else
            {
                HeaderTitleText.text = data.name; // 未設定時の保険
            }
        }

        // ボタンの監視を開始
        NextButton.onClick.RemoveAllListeners();
        NextButton.onClick.AddListener(OnClickNext);

        PrevButton.onClick.RemoveAllListeners();
        PrevButton.onClick.AddListener(OnClickPrev);
        
        // 最初の1枚目を表示
        ShowStep();
    }

    // 画面更新処理
    void ShowStep()
    {
        if (currentScenario.Steps.Count == 0) return;

        ScenarioStep step = currentScenario.Steps[currentStepIndex];

        // 1. テキスト更新
        MessageText.text = step.MainText;

        // 2. 画像更新
        if (step.BgImage != null) BgImage.sprite = step.BgImage;
        if (step.CenterImage != null) SlideImage.sprite = step.CenterImage;

        // 3. ボタン表示制御
        PrevButton.gameObject.SetActive(currentStepIndex > 0);
    }

    public void OnClickNext()
    {
        // 最後のページなら終了
        if (currentStepIndex >= currentScenario.Steps.Count - 1)
        {
            Debug.Log("シナリオ終了！");
            // 将来的にはここでクリア画面を出します
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
}