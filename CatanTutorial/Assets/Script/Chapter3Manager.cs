using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Chapter3Manager : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI GuideText;
    public TextMeshProUGUI HeaderTitleText;
    public Image BoardImage;

    [Header("Containers")]
    public GameObject Chapter3Container;
    public GameObject Section1Container; // 実践1用
    public GameObject Section2Container; // 実践2用 ★追加

    [Header("Prefabs & Objects")]
    public GameObject RobberPrefab;   // 盗賊のコマ
    public GameObject KnightCardPrefab; // 騎士カードのプレハブ ★追加
    public Transform TileAnchor;      // 盗賊を置く場所
    public Transform CardSlotContainer; 
    public GameObject[] ResourceCardPrefabs; 

    [Header("UI Controls")]
    public Button ActionButton;       
    public TextMeshProUGUI ActionButtonText; 

    [Header("Dice Settings")]
    public GameObject DicePanel;
    public Image DiceImage1, DiceImage2;
    public Sprite[] DiceSprites;

    [Header("Slides (Section 1)")]
    public Sprite SlideRobberBefore;
    public Sprite SlideRobberAfter;

    // ★追加：セクション2用のスライド
    [Header("Slides (Section 2)")]
    public Sprite SlideKnightBefore; // 騎士カードを使う前
    public Sprite SlideKnightAfter;  // 使った後（盗賊移動後）

    // 内部変数
    private bool isButtonClicked = false;
    private GameObject currentRobber; 
    private GameObject currentKnightCard; // 表示中の騎士カード

    public void StartPractice(int sectionIndex)
    {
        StopAllCoroutines();
        
        // 全体初期化
        if (Chapter3Container) Chapter3Container.SetActive(true);
        if (Section1Container) Section1Container.SetActive(false);
        if (Section2Container) Section2Container.SetActive(false);
        if (DicePanel) DicePanel.SetActive(false);
        
        // オブジェクトお掃除
        if (currentRobber != null) Destroy(currentRobber);
        if (currentKnightCard != null) Destroy(currentKnightCard);
        ClearHandCards();

        isButtonClicked = false;
        if (ActionButton) ActionButton.gameObject.SetActive(false);

        // 分岐
        if (sectionIndex == 0)
        {
            StartCoroutine(Flow_BurstPractice());
        }
        else if (sectionIndex == 1) // ★追加
        {
            StartCoroutine(Flow_KnightPractice());
        }
    }

    // =================================================================
    // 実践1：バーストと盗賊
    // =================================================================
    IEnumerator Flow_BurstPractice()
    {
        Section1Container.SetActive(true);
        SetHeaderTitle("「7」と盗賊について");
        
        if (SlideRobberBefore != null) BoardImage.sprite = SlideRobberBefore;

        GuideText.text = "【実践1】バースト\n\n手札が8枚以上ある時に「7」が出ると、\n手札を半分捨てなければなりません。";
        yield return StartCoroutine(SpawnCards(0, 8)); // 木8枚
        
        SetButton("サイコロを振る");
        yield return StartCoroutine(WaitForDiceRoll(7)); 

        GuideText.text = "「7」が出ました！\n\n手札が8枚あるので、\n半分の4枚を選んで捨ててください。";

        SetButton("半分捨てる (4枚)");
        yield return StartCoroutine(WaitForButtonPress());

        ClearHandCards();
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(SpawnCards(0, 4)); 
        
        GuideText.text = "手札が半分になりました。\n\n次に、7を出した人は「盗賊」を動かします。";
        yield return new WaitForSeconds(1.0f);

        // --- 盗賊移動 & 略奪 (共通処理っぽいが、あえてベタ書きで分かりやすく) ---
        SetButton("盗賊を動かす");
        yield return StartCoroutine(WaitForButtonPress());

        SpawnRobber();
        if (SlideRobberAfter != null) BoardImage.sprite = SlideRobberAfter;

        GuideText.text = "盗賊を移動させました！\n\n盗賊がいるタイルの周りの人から\n資源を1枚奪います。";

        SetButton("資源を奪う");
        yield return StartCoroutine(WaitForButtonPress());

        yield return StartCoroutine(SpawnCards(2, 1)); // 羊ゲット
        GuideText.text = "相手から資源を奪いました！";
        
        SetButton("終了");
        yield return StartCoroutine(WaitForButtonPress());
        
        // ★追加: クリア画面を表示！
        var app = GetComponent<AppManager>();
        if (app != null) app.ShowClearPanel();
        
        GuideText.text = "実践 クリア！";
    }

    // =================================================================
    // 実践2：騎士カードと盗賊 (修正版)
    // =================================================================
    IEnumerator Flow_KnightPractice()
    {
        Section2Container.SetActive(true);
        SetHeaderTitle("騎士カードについて");

        // スライド：使用前
        if (SlideKnightBefore != null) BoardImage.sprite = SlideKnightBefore;

        // 騎士カード生成
        if (KnightCardPrefab != null && Section2Container != null)
        {
            currentKnightCard = Instantiate(KnightCardPrefab, Section2Container.transform);
            currentKnightCard.transform.localPosition = Vector3.zero; 
            currentKnightCard.transform.localScale = Vector3.one;

            // ボタン機能追加
            Button cardBtn = currentKnightCard.GetComponent<Button>();
            if (cardBtn != null)
            {
                cardBtn.onClick.RemoveAllListeners(); // 念のためクリア
                cardBtn.onClick.AddListener(OnClickAction);
            }
        }

        GuideText.text = "【実践2】騎士カード\n\n「騎士カード」を使うと、\nサイコロの「7」と同じ効果を発動できます。\n\n画面の騎士カードをタップして使ってみましょう。";
        
        // 汎用ボタンは一旦隠す
        if (ActionButton) ActionButton.gameObject.SetActive(false);

        // クリック待ち
        isButtonClicked = false;
        yield return new WaitUntil(() => isButtonClicked);

        // --- 使用後の処理 ---
        if (currentKnightCard != null) Destroy(currentKnightCard);
        
        if (SlideKnightAfter != null) BoardImage.sprite = SlideKnightAfter;

        GuideText.text = "騎士カードを使いました！\n（盗賊が移動しました）";
        
        // ★修正点：ここで少しだけ待つが、すぐに次へ進む
        yield return new WaitForSeconds(2f);

        SpawnRobber();
        GuideText.text = "盗賊を移動させました！\n\nここからは「7」の時と同じです。\n相手から資源を奪いましょう。";

        // ★修正点：ボタンを確実に表示
        if (ActionButton) ActionButton.gameObject.SetActive(true);
        SetButton("資源を奪う");
        
        yield return StartCoroutine(WaitForButtonPress());

        yield return StartCoroutine(SpawnCards(4, 1)); // 鉄ゲット
        GuideText.text = "相手から「鉱石」を奪いました！\n\n騎士カードは強力な攻撃手段です。\nうまく活用しましょう。";

        SetButton("終了");
        yield return StartCoroutine(WaitForButtonPress());
        
        // ★追加: クリア画面を表示！
        var app = GetComponent<AppManager>();
        if (app != null) app.ShowClearPanel();
        
        GuideText.text = "実践2 クリア！";
    }

    // -----------------------------------------------------------------
    // ヘルパー関数
    // -----------------------------------------------------------------
    void SpawnRobber()
    {
        if (RobberPrefab != null && TileAnchor != null)
        {
            if(currentRobber != null) Destroy(currentRobber);
            currentRobber = Instantiate(RobberPrefab, TileAnchor.position, Quaternion.identity);
            currentRobber.transform.localScale = new Vector3(20, 40, 20); // 必要に応じて調整
            currentRobber.transform.SetParent(TileAnchor);
        }
    }

    // ... (以下、WaitForDiceRoll, WaitForButtonPress等は変更なし) ...
    IEnumerator WaitForDiceRoll(int targetSum) { /* 前回と同じコード */ 
        if(ActionButton) ActionButton.interactable = true;
        isButtonClicked = false;
        yield return new WaitUntil(() => isButtonClicked);
        if(ActionButton) ActionButton.gameObject.SetActive(false);
        if(DicePanel) DicePanel.SetActive(true);
        float duration = 1.0f;
        float elapsed = 0f;
        if (DiceSprites != null && DiceSprites.Length >= 6) {
            while (elapsed < duration) {
                DiceImage1.sprite = DiceSprites[Random.Range(0, 6)];
                DiceImage2.sprite = DiceSprites[Random.Range(0, 6)];
                elapsed += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }
            int val1 = targetSum / 2; int val2 = targetSum - val1;
            if(targetSum == 7) { val1 = 1; val2 = 6; }
            DiceImage1.sprite = DiceSprites[val1 - 1];
            DiceImage2.sprite = DiceSprites[val2 - 1];
        }
        yield return new WaitForSeconds(1.5f);
        if(DicePanel) DicePanel.SetActive(false);
    }
    IEnumerator WaitForButtonPress() {
        if(ActionButton) ActionButton.interactable = true;
        isButtonClicked = false;
        yield return new WaitUntil(() => isButtonClicked);
        if(ActionButton) ActionButton.interactable = false;
        if(ActionButton) ActionButton.gameObject.SetActive(false); 
    }
    void SetButton(string label) {
        if (ActionButton) { ActionButton.gameObject.SetActive(true); ActionButton.interactable = true; }
        if (ActionButtonText) ActionButtonText.text = label;
    }
    void SetHeaderTitle(string title) { if (HeaderTitleText != null) HeaderTitleText.text = title; }
    IEnumerator SpawnCards(int typeIndex, int count) {
        if (ResourceCardPrefabs == null || typeIndex >= ResourceCardPrefabs.Length) yield break;
        GameObject prefab = ResourceCardPrefabs[typeIndex];
        for (int i = 0; i < count; i++) {
            if (CardSlotContainer) {
                GameObject card = Instantiate(prefab, CardSlotContainer);
                card.transform.localScale = Vector3.one;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    void ClearHandCards() {
        if (CardSlotContainer) foreach (Transform child in CardSlotContainer) Destroy(child.gameObject);
    }
    public void OnClickAction() { isButtonClicked = true; }
}