using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AssembleManager : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI GuideText;
    public TextMeshProUGUI HeaderTitleText;
    public Image BoardImage;

    [Header("Containers")]
    public GameObject AssembleContainer; 

    [Header("UI Controls")]
    public Button ActionButton;          
    public TextMeshProUGUI ActionButtonText; 

    [Header("Slides (Setup Steps)")]
    // ★追加: Step 0 何もない状態（ただの海、または机など）
    public Sprite SlideStep0_Empty;    
    
    public Sprite SlideStep1_Sea;      // 海フレームだけ
    public Sprite SlideStep2_Tiles;    // タイル配置済み
    
    // ★追加: Step 3 チップ裏面（アルファベット）
    public Sprite SlideStep3_ChipsBack; 
    
    public Sprite SlideStep4_Complete; // 完成（数字面）※旧 SlideStep3_Complete

    // 内部変数
    private bool isButtonClicked = false;

    // -----------------------------------------------------------------
    // 起動処理
    // -----------------------------------------------------------------
    public void StartPractice()
    {
        StopAllCoroutines();
        
        if (AssembleContainer) AssembleContainer.SetActive(true);
        if (ActionButton) ActionButton.gameObject.SetActive(false);

        StartCoroutine(Flow_SetupBoard());
    }

    // =================================================================
    // 組み立て実践フロー
    // =================================================================
    IEnumerator Flow_SetupBoard()
    {
        SetHeaderTitle("カタンの組み立て");
        
        // ---------------------------------------------------------
        // Step 0: 準備（何もない状態）
        // ---------------------------------------------------------
        // ★修正: 最初は「フレーム画像」を出さず、「何もない画像」を出す
        if (SlideStep0_Empty != null) 
        {
            BoardImage.sprite = SlideStep0_Empty;
        }
        else
        {
            // 何もない画像が設定されていない場合、とりあえず既存の画像を非表示にするなどの対応が必要ですが、
            // ここでは画像そのままで進行します（前の画面が残る）
        }

        GuideText.text = "【実践】ボードの作成\n\nカタンのゲームを始める前に、\n島を組み立てる手順を覚えましょう。\n\nまずは「海フレーム」です。";
        yield return new WaitForSeconds(1.0f);


        // ---------------------------------------------------------
        // Step 1: 海フレーム
        // ---------------------------------------------------------
        GuideText.text = "海フレームのつなぎ目には\nそれぞれ数字が書かれています。\n\n同じ数字同士を合わせて組み立てます。";
        
        SetButton("フレームを組む");
        yield return StartCoroutine(WaitForButtonPress());

        // ★修正: ボタンを押した後にフレーム画像を表示！
        if (SlideStep1_Sea != null) BoardImage.sprite = SlideStep1_Sea;
        // SE: ガチャン！

        GuideText.text = "海フレームが組み上がりました。\n綺麗な六角形になります。";
        
        SetButton("次へ");
        yield return StartCoroutine(WaitForButtonPress());


        // ---------------------------------------------------------
        // Step 2: タイル配置
        // ---------------------------------------------------------
        GuideText.text = "次に「地形タイル」を並べます。\n\nまず中央に縦に5枚。\nその両隣に4枚ずつ。\nさらに外側に3枚ずつ並べます。";
        
        SetButton("タイルを並べる");
        yield return StartCoroutine(WaitForButtonPress());

        // 画像切り替え：タイルあり
        if (SlideStep2_Tiles != null) BoardImage.sprite = SlideStep2_Tiles;

        GuideText.text = "タイルが並びました。\n（港がない場所から並べ始めます）";
        
        SetButton("次へ");
        yield return StartCoroutine(WaitForButtonPress());


        // ---------------------------------------------------------
        // Step 3: 数字チップ (アルファベット面)
        // ---------------------------------------------------------
        GuideText.text = "最後に「数字チップ」です。\n\n最初は【アルファベット面】を表にして、\nAから順に並べていきます。";
        yield return new WaitForSeconds(2.0f);

        GuideText.text = "置く順番は決まっています。\n\n「外側から内側に向かって」\n反時計回りにぐるぐると置いていきます。";

        SetButton("チップを置く (A～R)");
        yield return StartCoroutine(WaitForButtonPress());

        // ★追加: ここで「チップ裏面（アルファベット）」の画像を表示
        if (SlideStep3_ChipsBack != null) BoardImage.sprite = SlideStep3_ChipsBack;

        GuideText.text = "チップを置き終わりました。\n\nこれで、確率のバランスが取れた\n配置になります。";
        yield return new WaitForSeconds(1.5f);


        // ---------------------------------------------------------
        // Step 4: 完成 (数字面)
        // ---------------------------------------------------------
        GuideText.text = "最後に、全てのチップを裏返して\n数字が見えるようにします。";

        SetButton("チップを裏返す");
        yield return StartCoroutine(WaitForButtonPress());

        // 画像切り替え：完成形（数字面）
        if (SlideStep4_Complete != null) BoardImage.sprite = SlideStep4_Complete;

        GuideText.text = "これでカタン島の完成です！\n\nここから開拓競争が始まります。";

        SetButton("終了");
        yield return StartCoroutine(WaitForButtonPress());
        
        // ★追加: クリア画面を表示！
        var app = GetComponent<AppManager>();
        if (app != null) app.ShowClearPanel();
        
        GuideText.text = "組み立て編 クリア！";
    }

    // -----------------------------------------------------------------
    // ヘルパー関数群
    // -----------------------------------------------------------------
    void SetButton(string label)
    {
        if (ActionButton)
        {
            ActionButton.gameObject.SetActive(true);
            ActionButton.interactable = true;
            ActionButton.onClick.RemoveAllListeners();
            ActionButton.onClick.AddListener(OnClickAction);
        }
        if (ActionButtonText) ActionButtonText.text = label;
    }

    IEnumerator WaitForButtonPress()
    {
        if(ActionButton) ActionButton.interactable = true;
        isButtonClicked = false;
        yield return new WaitUntil(() => isButtonClicked);
        if(ActionButton) ActionButton.interactable = false;
        if(ActionButton) ActionButton.gameObject.SetActive(false); 
    }

    void SetHeaderTitle(string title) { if (HeaderTitleText != null) HeaderTitleText.text = title; }
    public void OnClickAction() { isButtonClicked = true; }
}