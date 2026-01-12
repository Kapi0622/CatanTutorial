using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Chapter1Manager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject PracticePanel;      // 実践画面パネル全体
    public TextMeshProUGUI GuideText;     // 右側のガイドメッセージ
    public Image BoardImage;              // 背景のスライド画像

    [Header("Practice Containers")]
    public GameObject DiceContainer;      // 実践1用の親オブジェクト
    public GameObject BoardContainer;     // 実践2用の親オブジェクト

    [Header("Manager References")]
    public PracticeBoardManager boardManager; // 実践2の現場監督スクリプト

    [Header("--- Section 1: Dice Settings ---")]
    public Image DiceImage1;              // 左のサイコロ
    public Image DiceImage2;              // 右のサイコロ
    public Button RollButton;             // 「振る」ボタン (※サイコロ専用)
    public Sprite[] DiceSprites;          // サイコロの目画像 (0~5)
    public Sprite SlideBeforeRoll;        // 実践1：振る前のスライド
    public Sprite SlideAfterRoll;         // 実践1：振った後のスライド

    [Header("--- Section 2: Settlement Settings ---")]
    public Sprite SlideSettlementInitial; // 実践2：初期盤面
    public Sprite SlideResourceGet;       // 実践2：資源獲得時の盤面（ハイライトなど）

    // ★追加: 汎用アクションボタン（終了ボタン用）
    [Header("UI Controls")]
    public Button ActionButton;           // 汎用ボタン (Btn_Action等を割り当て)
    public TextMeshProUGUI ActionButtonText; 

    // 内部フラグ
    private bool isRolling = false;
    private bool isStepFinished = false;
    private bool isButtonClicked = false; // ★追加
    
    public Color[] PlayerColors;

    // --- 起動処理 (AppManagerから呼ばれる) ---
    public void StartPractice(int sectionIndex)
    {
        // ★重要：前のプレイで行っていた処理（コルーチン）を全て強制停止
        StopAllCoroutines();

        // ★重要：フラグのリセット
        isRolling = false;
        isStepFinished = false;
        isButtonClicked = false;

        // コンテナを一旦すべて隠す
        if (DiceContainer) DiceContainer.SetActive(false);
        if (BoardContainer) BoardContainer.SetActive(false);
        
        // ボタンも隠す
        if (RollButton) RollButton.gameObject.SetActive(false);
        if (ActionButton) ActionButton.gameObject.SetActive(false);

        // セクション番号に応じて開始する処理を分岐
        if (sectionIndex == 0)
        {
            // 実践1：順番決め（サイコロ）
            StartCoroutine(Flow_DicePractice());
        }
        else if (sectionIndex == 1)
        {
            // 実践2：開拓地の設置（カタン返し）
            if (boardManager != null) boardManager.Initialize();
            StartCoroutine(Flow_SettlementPractice());
        }
    }

    // ========================================================================
    // 実践1：順番決め（サイコロ）のフロー
    // ========================================================================
    IEnumerator Flow_DicePractice()
    {
        if (DiceContainer) DiceContainer.SetActive(true);

        // 1. 初期状態のセット
        if (SlideBeforeRoll != null) BoardImage.sprite = SlideBeforeRoll;

        GuideText.text = "【実践1】順番決め\nカタンでは、最初にサイコロを振って\n数字が一番大きい人がスタートプレイヤーになります。\n\n「振る」ボタンを押してください。";

        // サイコロの目を「1」に戻しておく
        if (DiceSprites != null && DiceSprites.Length > 0)
        {
            if (DiceImage1) DiceImage1.sprite = DiceSprites[0];
            if (DiceImage2) DiceImage2.sprite = DiceSprites[0];
        }

        // ボタンを表示して待機
        if (RollButton)
        {
            RollButton.gameObject.SetActive(true);
            RollButton.interactable = true;
        }

        // 2. ユーザーがボタンを押すまで待機
        isRolling = false;
        yield return new WaitUntil(() => isRolling);

        // 3. サイコロ演出開始
        if (RollButton) RollButton.gameObject.SetActive(false);
        GuideText.text = "サイコロを振っています...";

        // パラパラ漫画アニメーション（1.5秒間）
        float duration = 1.5f;
        float elapsed = 0f;
        if (DiceSprites != null && DiceSprites.Length >= 6)
        {
            while (elapsed < duration)
            {
                if (DiceImage1) DiceImage1.sprite = DiceSprites[Random.Range(0, 6)];
                if (DiceImage2) DiceImage2.sprite = DiceSprites[Random.Range(0, 6)];
                elapsed += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }

            // 4. 結果確定（イカサマ：合計11にする）
            if (DiceImage1) DiceImage1.sprite = DiceSprites[4]; // 5の目
            if (DiceImage2) DiceImage2.sprite = DiceSprites[5]; // 6の目
        }

        // スライド切り替え（1番手になった画像へ）
        if (SlideAfterRoll != null) BoardImage.sprite = SlideAfterRoll;

        GuideText.text = "「11」が出ました！\n最も大きい数字です。\nあなたが1番手のプレイヤーになりました。";

        // ★追加: 終了ボタンを表示してクリア画面へ
        SetButton("終了");
        yield return StartCoroutine(WaitForButtonPress());
        
        // クリア画面表示
        ShowClearScreen();
    }

    // ========================================================================
    // 実践2：開拓地設置（カタン返し）のフロー
    // ========================================================================
    IEnumerator Flow_SettlementPractice()
    {
        if (BoardContainer) BoardContainer.SetActive(true);
        if (SlideSettlementInitial != null) BoardImage.sprite = SlideSettlementInitial;

        // 色の設定
        Color myColor = (PlayerColors.Length > 0) ? PlayerColors[0] : Color.whiteSmoke;
        Color cpu1Color = (PlayerColors.Length > 1) ? PlayerColors[1] : Color.darkRed;
        Color cpu2Color = (PlayerColors.Length > 2) ? PlayerColors[2] : new Color(1f, 0.5f, 0f);
        Color cpu3Color = (PlayerColors.Length > 3) ? PlayerColors[3] : Color.darkBlue;

        // --- 往路（1番手 → 4番手） ---

        // 【あなた：1番手】開拓地
        GuideText.text = "【実践2】初期配置\n\nあなたが1番手です。\nまずは「開拓地」を置きます。\n光っている場所をクリックしてください。";
        isStepFinished = false;
        boardManager.HighlightNode(1, (clickedId) => 
        {
            boardManager.SpawnPiece(clickedId, myColor, 0); 
            boardManager.DisableAllNodes();
            isStepFinished = true;
        });
        yield return new WaitUntil(() => isStepFinished);

        // 【あなた：1番手】街道
        GuideText.text = "続いて、その開拓地から伸びる\n「街道」を1本設置します。";
        isStepFinished = false;
        boardManager.HighlightNode(101, (clickedId) => 
        {
            boardManager.SpawnPiece(clickedId, myColor, 1); 
            boardManager.DisableAllNodes();
            isStepFinished = true;
        });
        yield return new WaitUntil(() => isStepFinished);


        // 【CPU】
        GuideText.text = "2番手 (CPU:白) の手番です...";
        yield return new WaitForSeconds(1.0f);
        boardManager.SpawnPiece(10, cpu1Color, 0); 
        yield return new WaitForSeconds(0.5f);
        boardManager.SpawnPiece(110, cpu1Color, 1); 

        GuideText.text = "3番手 (CPU:青) の手番です...";
        yield return new WaitForSeconds(1.0f);
        boardManager.SpawnPiece(20, cpu2Color, 0); 
        yield return new WaitForSeconds(0.5f);
        boardManager.SpawnPiece(120, cpu2Color, 1); 

        GuideText.text = "4番手 (CPU:橙) の手番です...";
        yield return new WaitForSeconds(1.0f);
        boardManager.SpawnPiece(30, cpu3Color, 0); 
        yield return new WaitForSeconds(0.5f);
        boardManager.SpawnPiece(130, cpu3Color, 1); 


        // --- 復路（4番手 → 1番手） ---
        
        GuideText.text = "4番手 (CPU:橙) が折り返します...";
        yield return new WaitForSeconds(1.0f);
        boardManager.SpawnPiece(31, cpu3Color, 0); 
        yield return new WaitForSeconds(0.5f);
        boardManager.SpawnPiece(131, cpu3Color, 1); 

        GuideText.text = "3番手 (CPU:青) の手番です...";
        yield return new WaitForSeconds(1.0f);
        boardManager.SpawnPiece(21, cpu2Color, 0); 
        yield return new WaitForSeconds(0.5f);
        boardManager.SpawnPiece(121, cpu2Color, 1); 

        GuideText.text = "2番手 (CPU:白) の手番です...";
        yield return new WaitForSeconds(1.0f);
        boardManager.SpawnPiece(11, cpu1Color, 0); 
        yield return new WaitForSeconds(0.5f);
        boardManager.SpawnPiece(111, cpu1Color, 1); 


        // --- あなたのターン（最後） ---

        // 【あなた：1番手 (2個目)】開拓地
        GuideText.text = "あなたの手番です。\n2つ目の「開拓地」を\n指定の場所に置いてください。";
        isStepFinished = false;
        boardManager.HighlightNode(2, (clickedId) => 
        {
            boardManager.SpawnPiece(clickedId, myColor, 0);
            boardManager.DisableAllNodes();
            isStepFinished = true;
        });
        yield return new WaitUntil(() => isStepFinished);

        // 【あなた：1番手 (2個目)】街道
        GuideText.text = "最後に「街道」を設置します。";
        isStepFinished = false;
        boardManager.HighlightNode(102, (clickedId) => 
        {
            boardManager.SpawnPiece(clickedId, myColor, 1);
            boardManager.DisableAllNodes();
            isStepFinished = true;
        });
        yield return new WaitUntil(() => isStepFinished);


        // --- 資源獲得 ---
        if (SlideResourceGet != null) BoardImage.sprite = SlideResourceGet;
        GuideText.text = "全員の配置が完了しました！\n\n最後に置いた開拓地（2軒目）の\n周囲から資源を獲得します。";
        
        yield return new WaitForSeconds(2.0f);

        // ★追加: 終了ボタンを表示してクリア画面へ
        SetButton("終了");
        yield return StartCoroutine(WaitForButtonPress());

        // クリア画面表示
        ShowClearScreen();
    }

    // =================================================================
    // ヘルパー関数
    // =================================================================

    // サイコロボタン用
    public void OnClickRollButton()
    {
        isRolling = true;
    }

    // 汎用アクションボタン用
    public void OnClickAction()
    {
        isButtonClicked = true;
    }

    // ボタンを設定して表示する
    void SetButton(string label)
    {
        if (ActionButton)
        {
            ActionButton.gameObject.SetActive(true);
            ActionButton.interactable = true;
            // リスナーをリセットして自分を登録
            ActionButton.onClick.RemoveAllListeners();
            ActionButton.onClick.AddListener(OnClickAction);
        }
        if (ActionButtonText) ActionButtonText.text = label;
    }

    // ボタンが押されるのを待つ
    IEnumerator WaitForButtonPress()
    {
        if(ActionButton) ActionButton.interactable = true;
        isButtonClicked = false;
        yield return new WaitUntil(() => isButtonClicked);
        if(ActionButton) ActionButton.interactable = false;
        if(ActionButton) ActionButton.gameObject.SetActive(false); 
    }

    // AppManagerのクリア画面を呼ぶ
    void ShowClearScreen()
    {
        var app = GetComponent<AppManager>();
        if (app != null) app.ShowClearPanel();
    }
}