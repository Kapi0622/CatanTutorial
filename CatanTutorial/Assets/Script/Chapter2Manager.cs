using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Chapter2Manager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject PracticePanel;
    public TextMeshProUGUI GuideText;
    public Image BoardImage;
    public TextMeshProUGUI HeaderTitleText; // ヘッダータイトル

    [Header("Practice Containers")]
    public GameObject QuizContainer;           // 実践1の親
    public GameObject ResourceGainContainer;   // 実践2の親 (カード置き場含む)
    public GameObject ConstructionContainer;   // 実践3の親
    public GameObject TradeContainer;          // 実践4の親

    // =================================================================
    // ▼ 実践1：クイズ用設定
    // =================================================================
    [Header("--- Section 1: Quiz Settings ---")]
    public Image QuestionImage;
    // ボタン順序: 0:木, 1:土, 2:羊, 3:麦, 4:鉄
    public Button[] AnswerButtons;
    // 画像順序: 0:森, 1:丘, 2:牧場, 3:畑, 4:山
    public Sprite[] TerrainSprites;

    // =================================================================
    // ▼ 実践2：資源獲得用設定
    // =================================================================
    [Header("--- Section 2: Resource Gain Settings ---")]
    public PracticeBoardManager managerSection2; // ★実践2専用の監督
    public Sprite SlideResourceNormal;
    public Sprite SlideResourceActive;
    
    public GameObject DicePanel;
    public Image DiceImage1, DiceImage2;
    public Button RollButton;
    public Sprite[] DiceSprites;
    
    public GameObject CardPrefab;
    public Transform CardSpawnPoint;
    public Transform CardTargetPoint;
    public Transform CardSlotContainer; // カードが並ぶ箱 (実践4でも使用)
    public TextMeshProUGUI ResourceCountText;

    // =================================================================
    // ▼ 実践3：建設用設定
    // =================================================================
    [Header("--- Section 3: Construction Settings ---")]
    public PracticeBoardManager managerSection3; // ★実践3専用の監督
    
    public TextMeshProUGUI TxtWood, TxtBrick, TxtWool, TxtWheat, TxtOre;
    public Button BtnRoad, BtnSettlement, BtnCity, BtnDevCard;
    public GameObject DevCardVisual;

    // =================================================================
    // ▼ 実践4：トレード用設定
    // =================================================================
    [Header("--- Section 4: Trade Settings ---")]
    public Button BtnTradeAction; // アクション進行用ボタン (旧BtnTradeBank)
    // (Element 0:木, 1:土, 2:羊, 3:麦, 4:鉄)
    public GameObject[] ResourceCardPrefabs;
    
    public Sprite SlideTradeBank;   // シーン1：銀行(4:1)用
    public Sprite SlideTradePort;   // シーン2：港(3:1)用
    public Sprite SlideTradePlayer; // シーン3：プレイヤー交換用
    
    // ★追加: 汎用アクションボタン（終了ボタン用）
    [Header("UI Controls")]
    public Button ActionButton;           // 汎用ボタン (Btn_Action等を割り当て)
    public TextMeshProUGUI ActionButtonText; 

    // 内部変数
    private bool isRolling = false;
    private int currentAnswer = -1;
    private bool isAnswered = false;
    private bool isButtonClicked = false;

    // -----------------------------------------------------------------
    // 起動処理：ここですべてをリセットし、必要なコンテナだけを開く
    // -----------------------------------------------------------------
    public void StartPractice(int sectionIndex)
    {
        StopAllCoroutines();

        // 1. 全コンテナを非表示（初期化）
        if(QuizContainer) QuizContainer.SetActive(false);
        if(ResourceGainContainer) ResourceGainContainer.SetActive(false);
        if(ConstructionContainer) ConstructionContainer.SetActive(false);
        if(TradeContainer) TradeContainer.SetActive(false);
        
        // ボタン類を隠す
        if (ActionButton) ActionButton.gameObject.SetActive(false);
        if (BtnTradeAction) BtnTradeAction.gameObject.SetActive(false);

        // 2. 変数リセット
        isAnswered = false;
        isButtonClicked = false;
        isRolling = false;
        currentAnswer = -1;

        // 3. セクションごとの分岐
        if (sectionIndex == 0)
        {
            // --- 実践1：クイズ ---
            SetHeaderTitle("タイルの種類ともらえる資源");
            StartCoroutine(Flow_QuizPractice());
        }
        else if (sectionIndex == 1)
        {
            // --- 実践2：資源獲得 ---
            SetHeaderTitle("資源を獲得する流れ");
            
            if (managerSection2 != null)
            {
                managerSection2.Initialize();
                managerSection2.ClearBoard(); // 盤面掃除
            }
            ClearHandCards(); // カード掃除
            StartCoroutine(Flow_GainResourcePractice());
        }
        else if (sectionIndex == 2)
        {
            // --- 実践3：建設 ---
            SetHeaderTitle("建設、都市化、発展カード");

            if (managerSection3 != null)
            {
                managerSection3.Initialize();
                managerSection3.ClearBoard(); // 盤面掃除
            }
            StartCoroutine(Flow_ConstructionPractice());
        }
        else if (sectionIndex == 3)
        {
            // --- 実践4：トレード ---
            SetHeaderTitle("資源カードの交換");
            ClearHandCards();
            StartCoroutine(Flow_TradePractice());
        }
    }

    // =================================================================
    // 実践1：資源クイズのフロー (5問ループ)
    // =================================================================
    IEnumerator Flow_QuizPractice()
    {
        QuizContainer.SetActive(true);

        // 0:森(木), 1:丘(土), 2:牧場(羊), 3:畑(麦), 4:山(鉄)
        yield return StartCoroutine(RunQuestion(0, "森林", "木材", 0));
        yield return StartCoroutine(RunQuestion(1, "丘陵", "レンガ", 1));
        yield return StartCoroutine(RunQuestion(2, "牧場", "羊毛", 2));
        yield return StartCoroutine(RunQuestion(3, "畑", "小麦", 3));
        yield return StartCoroutine(RunQuestion(4, "山地", "鉱石", 4));

        GuideText.text = "全問正解！素晴らしいです。\nそれぞれの地形から取れる資源を\nしっかり覚えられましたね。";
        
        // ★修正: 終了ボタン待機 -> クリア画面
        SetButton("終了");
        yield return StartCoroutine(WaitForButtonPress());
        ShowClearScreen();
    }

    IEnumerator RunQuestion(int spriteIndex, string terrainName, string resourceName, int correctBtnIndex)
    {
        GuideText.text = $"この「{terrainName}」タイルからは、\nどの資源が取れるでしょうか？";
        if (spriteIndex < TerrainSprites.Length) QuestionImage.sprite = TerrainSprites[spriteIndex];

        foreach(var btn in AnswerButtons) btn.interactable = true;
        bool isCorrect = false;

        while (!isCorrect)
        {
            isAnswered = false;
            currentAnswer = -1;
            yield return new WaitUntil(() => isAnswered);

            if (currentAnswer == correctBtnIndex)
            {
                GuideText.text = $"正解！\n\n「{terrainName}」からは「{resourceName}」が取れます。";
                isCorrect = true;
                foreach(var btn in AnswerButtons) btn.interactable = false;
                yield return new WaitForSeconds(2.0f);
            }
            else
            {
                GuideText.text = "違います... もう一度選んでみてください。";
                if (currentAnswer >= 0 && currentAnswer < AnswerButtons.Length)
                    AnswerButtons[currentAnswer].interactable = false;
            }
        }
    }

    // =================================================================
    // 実践2：資源獲得のフロー (自動配置・自動道設置)
    // =================================================================
    IEnumerator Flow_GainResourcePractice()
    {
        ResourceGainContainer.SetActive(true);
        if(DicePanel) DicePanel.SetActive(true);
        if(ResourceCountText) ResourceCountText.gameObject.SetActive(true);
        
        if (SlideResourceNormal != null) BoardImage.sprite = SlideResourceNormal;
        
        ResourceCountText.text = "木材: 0";
        int currentWood = 0;
        
        // 初期配置
        managerSection2.SpawnPiece(1, Color.red, 0);   
        managerSection2.SpawnPiece(101, Color.red, 1); 

        GuideText.text = "【実践2】資源獲得\n\n画面中央の「森林（数字6）」に、\nあなたの開拓地と街道があります。\n\nサイコロを振ってください。";

        // --- 1回目 ---
        yield return StartCoroutine(WaitForDiceRoll(6));

        if (SlideResourceActive != null) BoardImage.sprite = SlideResourceActive;
        GuideText.text = "「6」が出ました！\n数字が一致したので、\n資源（木材）を1枚獲得します。";
        yield return StartCoroutine(AnimateResourceGain(1));
        currentWood += 1;
        ResourceCountText.text = $"木材: {currentWood}";
        
        yield return new WaitForSeconds(2.0f);
        if (SlideResourceNormal != null) BoardImage.sprite = SlideResourceNormal;

        // --- 増築フェーズ ---
        GuideText.text = "ここで、あなたの開拓地が\nもう一箇所に増えました。";
        yield return new WaitForSeconds(0.5f);
        managerSection2.SpawnPiece(2, Color.red, 0);   
        managerSection2.SpawnPiece(102, Color.red, 1); 
        yield return new WaitForSeconds(1.5f);

        // --- 2回目 ---
        GuideText.text = "開拓地が2つある状態で\nもう一度サイコロを振ってみましょう。";
        yield return StartCoroutine(WaitForDiceRoll(6));

        if (SlideResourceActive != null) BoardImage.sprite = SlideResourceActive;
        GuideText.text = "また「6」が出ました！\n\n開拓地が2つあるので、\n獲得できる資源も2枚になります！";
        yield return StartCoroutine(AnimateResourceGain(2));
        currentWood += 2;
        ResourceCountText.text = $"木材: {currentWood}";

        yield return new WaitForSeconds(1.0f);
        GuideText.text = "実践2 クリア！\n\nちなみに、都市に発展させると\n1箇所から2枚（合計4枚）貰えるようになります。";
        
        // ★修正: 終了ボタン待機 -> クリア画面
        SetButton("終了");
        yield return StartCoroutine(WaitForButtonPress());
        ShowClearScreen();
    }

    // =================================================================
    // 実践3：建設・都市化フロー (即時建設・完全分離)
    // =================================================================
    IEnumerator Flow_ConstructionPractice()
    {
        ConstructionContainer.SetActive(true);
        if (DevCardVisual) DevCardVisual.SetActive(false);
        UpdateResources(0,0,0,0,0);

        if (managerSection3 == null)
        {
            Debug.LogError("Manager Section 3 が設定されていません！");
            yield break;
        }

        // --- ① 街道 ---
        GuideText.text = "【実践3】建設\n\nまずは「街道」を作ってみましょう\n必要な資源：木材1・レンガ1";
        yield return new WaitForSeconds(1.0f);
        UpdateResources(1, 1, 0, 0, 0);

        BtnRoad.interactable = true;
        isButtonClicked = false;
        yield return new WaitUntil(() => isButtonClicked);
        BtnRoad.interactable = false;

        managerSection3.SpawnPiece(101, Color.red, 1);
        GuideText.text = "街道が建設されました！";
        UpdateResources(0, 0, 0, 0, 0);
        yield return new WaitForSeconds(1.5f);

        // --- ② 開拓地 ---
        GuideText.text = "次は「開拓地」を建設します\n必要な資源：木・土・羊・麦";
        UpdateResources(1, 1, 1, 1, 0);

        BtnSettlement.interactable = true;
        isButtonClicked = false;
        yield return new WaitUntil(() => isButtonClicked);
        BtnSettlement.interactable = false;

        managerSection3.SpawnPiece(1, Color.red, 0);
        GuideText.text = "開拓地が建設されました！";
        UpdateResources(0, 0, 0, 0, 0);
        yield return new WaitForSeconds(1.5f);

        // --- ③ 都市 ---
        GuideText.text = "開拓地を「都市」に発展させます\n必要な資源：麦2・鉄3";
        UpdateResources(0, 0, 0, 2, 3);

        BtnCity.interactable = true;
        isButtonClicked = false;
        yield return new WaitUntil(() => isButtonClicked);
        BtnCity.interactable = false;

        managerSection3.UpgradeToCity(1, Color.red);
        GuideText.text = "都市になりました！";
        UpdateResources(0, 0, 0, 0, 0);
        yield return new WaitForSeconds(1.5f);

        // --- ④ 発展カード ---
        GuideText.text = "最後に「発展カード」を引きます";
        UpdateResources(0, 0, 1, 1, 1);

        BtnDevCard.interactable = true;
        isButtonClicked = false;
        yield return new WaitUntil(() => isButtonClicked);
        BtnDevCard.interactable = false;

        if (DevCardVisual) DevCardVisual.SetActive(true);
        UpdateResources(0, 0, 0, 0, 0);

        GuideText.text = "「騎士カード」を引きました！\n\nお疲れ様でした。\nこれで建設の基本はバッチリです！";
        
        // ★修正: 終了ボタン待機 -> クリア画面
        SetButton("終了");
        yield return StartCoroutine(WaitForButtonPress());
        ShowClearScreen();
    }

    // =================================================================
    // 実践4：トレード実演フロー (カード増減デモ)
    // =================================================================
    IEnumerator Flow_TradePractice()
    {
        // UI初期化
        TradeContainer.SetActive(true);
        if(ResourceGainContainer) ResourceGainContainer.SetActive(true);
        if(DicePanel) DicePanel.SetActive(false); 
        if(ResourceCountText) ResourceCountText.gameObject.SetActive(false);
        ClearHandCards();

        BtnTradeAction.gameObject.SetActive(true);
        TextMeshProUGUI btnText = BtnTradeAction.GetComponentInChildren<TextMeshProUGUI>();

        // --- Scene 1: 銀行交換 (4:1) ---
        if (SlideTradeBank != null) BoardImage.sprite = SlideTradeBank;
        
        GuideText.text = "【実践4】資源の交換\n\n欲しい資源（レンガ）を手に入れるため、\nまずは「銀行」と交換してみましょう。\n\nレートは【4:1】です。";
        yield return StartCoroutine(SpawnCardsToHand(0, 4)); // 木4枚
        
        if(btnText) btnText.text = "4枚で交換する";
        // ★BtnTradeActionはSection4専用ボタンなのでそのまま使用
        BtnTradeAction.interactable = true;
        isButtonClicked = false;
        yield return new WaitUntil(() => isButtonClicked);
        BtnTradeAction.interactable = false;

        ClearHandCards();
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(SpawnCardsToHand(1, 1)); // 土1枚
        
        GuideText.text = "木材4枚が、レンガ1枚になりました。\n\n自分のターンにいつでも交換できますが、\nコストが非常に高いのが欠点です。";
        
        if(btnText) btnText.text = "次へ";
        yield return StartCoroutine(WaitForTradeButtonPress()); 

        // --- Scene 2: 港での交換 (3:1) ---
        if (SlideTradePort != null) BoardImage.sprite = SlideTradePort;
        ClearHandCards();
        GuideText.text = "次は「港」を使ってみましょう。\n特定の場所に開拓地を建てると使えます。\n\n一般港のレートは【3:1】です。";
        yield return StartCoroutine(SpawnCardsToHand(0, 3)); // 木3枚

        if(btnText) btnText.text = "港で交換する (3枚)";
        yield return StartCoroutine(WaitForTradeButtonPress());

        ClearHandCards();
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(SpawnCardsToHand(1, 1)); // 土1枚

        GuideText.text = "3枚で交換できました。\n銀行より少しお得ですね。\n「専門港」なら2枚で済む場合もあります。";
        
        if(btnText) btnText.text = "次へ";
        yield return StartCoroutine(WaitForTradeButtonPress());

        // --- Scene 3: 他プレイヤーとの交換 (1:1) ---
        if (SlideTradePlayer != null) BoardImage.sprite = SlideTradePlayer;
        ClearHandCards();
        GuideText.text = "最後に「他プレイヤー」との交換です。\n\nレートは【交渉】で自由に決められます。\n相手が了承すれば成立です。";
        yield return StartCoroutine(SpawnCardsToHand(2, 1)); // 羊1枚

        if(btnText) btnText.text = "交渉して交換";
        yield return StartCoroutine(WaitForTradeButtonPress());

        ClearHandCards();
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(SpawnCardsToHand(1, 1)); // 土1枚

        GuideText.text = "交渉成立！\n今回はお互いの利害が一致したので\n【1:1】で交換できました。\n\nこれが最も効率が良い方法です。";
        
        // ★修正: 最後は汎用終了ボタンで統一
        if(BtnTradeAction) BtnTradeAction.gameObject.SetActive(false); // トレード用ボタンは隠す
        
        SetButton("終了");
        yield return StartCoroutine(WaitForButtonPress());
        ShowClearScreen();
    }

    // -----------------------------------------------------------------
    // ヘルパー関数群
    // -----------------------------------------------------------------
    
    // ★追加: 汎用ボタン制御
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

    // 実践4(Trade)専用のボタン待ち
    IEnumerator WaitForTradeButtonPress()
    {
        BtnTradeAction.interactable = true;
        isButtonClicked = false;
        yield return new WaitUntil(() => isButtonClicked);
        BtnTradeAction.interactable = false;
    }

    IEnumerator WaitForDiceRoll(int targetSum)
    {
        RollButton.gameObject.SetActive(true);
        RollButton.interactable = true;
        isRolling = false;
        yield return new WaitUntil(() => isRolling);
        RollButton.gameObject.SetActive(false);
        
        float duration = 1.0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            DiceImage1.sprite = DiceSprites[Random.Range(0, 6)];
            DiceImage2.sprite = DiceSprites[Random.Range(0, 6)];
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        int val1 = targetSum / 2;
        int val2 = targetSum - val1;
        DiceImage1.sprite = DiceSprites[val1 - 1];
        DiceImage2.sprite = DiceSprites[val2 - 1];
    }

    IEnumerator AnimateResourceGain(int count)
    {
        for(int i=0; i<count; i++)
        {
            GameObject card = Instantiate(CardPrefab, PracticePanel.transform);
            card.transform.position = CardSpawnPoint.position;
            
            float t = 0;
            Vector3 startPos = CardSpawnPoint.position;
            Vector3 endPos = CardTargetPoint.position;
            while(t < 1.0f)
            {
                t += Time.deltaTime * 2.0f;
                card.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            if(CardSlotContainer != null) card.transform.SetParent(CardSlotContainer, false);
            else Destroy(card);
            
            card.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator SpawnCardsToHand(int cardTypeIndex, int count)
    {
        if (ResourceCardPrefabs == null || cardTypeIndex >= ResourceCardPrefabs.Length) yield break;
        GameObject prefabToUse = ResourceCardPrefabs[cardTypeIndex];
        for(int i=0; i<count; i++)
        {
            GameObject card = Instantiate(prefabToUse, CardSlotContainer);
            card.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void ClearHandCards()
    {
        if (CardSlotContainer != null) foreach (Transform child in CardSlotContainer) Destroy(child.gameObject);
    }

    void UpdateResources(int wood, int brick, int wool, int wheat, int ore)
    {
        if(TxtWood) TxtWood.text = wood.ToString();
        if(TxtBrick) TxtBrick.text = brick.ToString();
        if(TxtWool) TxtWool.text = wool.ToString();
        if(TxtWheat) TxtWheat.text = wheat.ToString();
        if(TxtOre) TxtOre.text = ore.ToString();
    }

    void SetHeaderTitle(string title) { if (HeaderTitleText != null) HeaderTitleText.text = title; }
    
    // ★追加: クリア画面呼び出し
    void ShowClearScreen()
    {
        var app = GetComponent<AppManager>();
        if (app != null) app.ShowClearPanel();
    }

    // UIイベント受信
    public void OnClickRollButton() { isRolling = true; }
    public void OnClickAnswer(int idx) { currentAnswer = idx; isAnswered = true; }
    
    // 汎用アクション・建設・トレード全て共通でフラグを立てる
    public void OnClickAction() { isButtonClicked = true; }
    public void OnClickBuildButton() { isButtonClicked = true; } 
}