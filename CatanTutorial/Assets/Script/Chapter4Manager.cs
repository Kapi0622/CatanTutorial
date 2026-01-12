using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Chapter4Manager : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI GuideText;
    public TextMeshProUGUI HeaderTitleText;
    public Image BoardImage;

    [Header("Containers")]
    public GameObject Chapter4Container; 
    public GameObject Section1Container; 
    public GameObject Section2Container; 
    public GameObject Section3Container; 

    [Header("Prefabs & Objects")]
    public GameObject KnightCardPrefab;  
    public Transform CardSlotContainer;  
    public Transform DevCardSpawnPoint;  
    public GameObject[] ResourceCardPrefabs; 

    [Header("--- Section 2: Progress Cards ---")]
    public GameObject RoadPrefab;        
    public Transform RoadAnchor1;        
    public Transform RoadAnchor2;        
    
    // 実践2用のImage
    public Image CenterCardImage;        
    public Sprite SpriteRoadBuilding;    
    public Sprite SpriteYearOfPlenty;    
    public Sprite SpriteMonopoly;        

    [Header("--- Section 3: Bonus Cards ---")]
    public Transform RoadAnchor_Longest; 
    public Transform KnightAnchor_Army;  
    public Sprite SpriteLongestRoad;     
    public Sprite SpriteLargestArmy;     

    // ★追加：実践3専用のImage
    public Image BonusCardImage;         

    [Header("UI Controls")]
    public Button ActionButton;          
    public TextMeshProUGUI ActionButtonText; 

    [Header("Slides")]
    public Sprite SlideDevCardShop;      
    public Sprite SlideRoadBuilding;     
    public Sprite SlideMonopoly;         
    public Sprite SlideLongestRoad;      
    public Sprite SlideLargestArmy;      

    // 内部変数
    private bool isButtonClicked = false;
    private GameObject currentDevCard;
    private GameObject spawnedRoad1, spawnedRoad2, spawnedRoad3;
    private GameObject spawnedKnight;

    // -----------------------------------------------------------------
    // 起動処理
    // -----------------------------------------------------------------
    public void StartPractice(int sectionIndex)
    {
        StopAllCoroutines();
        
        // 1. 全コンテナ初期化
        if (Chapter4Container) Chapter4Container.SetActive(true);
        if (Section1Container) Section1Container.SetActive(false);
        if (Section2Container) Section2Container.SetActive(false);
        if (Section3Container) Section3Container.SetActive(false);
        
        // 2. オブジェクトお掃除
        ClearHandCards();
        if (currentDevCard != null) Destroy(currentDevCard);
        if (spawnedRoad1) Destroy(spawnedRoad1);
        if (spawnedRoad2) Destroy(spawnedRoad2);
        if (spawnedRoad3) Destroy(spawnedRoad3);
        if (spawnedKnight) Destroy(spawnedKnight);
        
        // 画像を隠す
        if (CenterCardImage) CenterCardImage.gameObject.SetActive(false);
        if (BonusCardImage) BonusCardImage.gameObject.SetActive(false); // ★追加

        // 3. ボタン初期化
        if (ActionButton) ActionButton.gameObject.SetActive(false);

        // 4. セクション分岐
        if (sectionIndex == 0) StartCoroutine(Flow_BuyDevCard());
        else if (sectionIndex == 1) StartCoroutine(Flow_ProgressCards());
        else if (sectionIndex == 2) StartCoroutine(Flow_BonusCards());
    }

    // =================================================================
    // セクション1：発展カードを引く (変更なし)
    // =================================================================
    IEnumerator Flow_BuyDevCard()
    {
        // ... (前回と同じなので省略。そのまま残してください) ...
        Section1Container.SetActive(true);
        SetHeaderTitle("発展カードについて");
        if (SlideDevCardShop != null) BoardImage.sprite = SlideDevCardShop;
        GuideText.text = "【実践1】発展カード\n\n特定の資源を消費することで、\n「発展カード」を1枚引くことができます。\n\n必要な資源は【小麦・羊毛・鉱石】です。";
        yield return StartCoroutine(SpawnCard(2)); 
        yield return StartCoroutine(SpawnCard(3)); 
        yield return StartCoroutine(SpawnCard(4)); 
        yield return new WaitForSeconds(1.0f);
        SetButton("発展カードを引く");
        yield return StartCoroutine(WaitForButtonPress());
        ClearHandCards();
        GuideText.text = "資源を支払いました。\n山札からカードを1枚引きます。";
        yield return new WaitForSeconds(0.5f);
        if (KnightCardPrefab != null && DevCardSpawnPoint != null) {
            currentDevCard = Instantiate(KnightCardPrefab, DevCardSpawnPoint);
            currentDevCard.transform.localPosition = Vector3.zero;
            currentDevCard.transform.localScale = Vector3.one; 
            Button btn = currentDevCard.GetComponent<Button>();
            if(btn) btn.interactable = false; 
        }
        GuideText.text = "「騎士カード」を引きました！\n\n注意点として、引いた発展カードは\n「そのターンには使えません」。";
        SetButton("終了");
        yield return StartCoroutine(WaitForButtonPress());
        
        // ★追加: クリア画面を表示！
        var app = GetComponent<AppManager>();
        if (app != null) app.ShowClearPanel();
        
        GuideText.text = "セクション1 クリア！";
    }

    // =================================================================
    // セクション2：進捗カード (CenterCardImageを使用)
    // =================================================================
    IEnumerator Flow_ProgressCards()
    {
        Section2Container.SetActive(true);
        SetHeaderTitle("進捗カードについて");

        // Scene A
        if (SlideRoadBuilding != null) BoardImage.sprite = SlideRoadBuilding;
        ShowCard(CenterCardImage, SpriteRoadBuilding); // ★変更：ターゲット指定

        GuideText.text = "【実践2】進捗カード\n\nまずは「街道建設」です。\n資源を使わずに、街道を2本建設できます。";
        yield return new WaitForSeconds(1.5f);
        SetButton("街道建設を使う");
        yield return StartCoroutine(WaitForButtonPress());

        if (RoadPrefab) {
            if (RoadAnchor1) spawnedRoad1 = Instantiate(RoadPrefab, RoadAnchor1);
            if (RoadAnchor2) spawnedRoad2 = Instantiate(RoadPrefab, RoadAnchor2);
        }
        GuideText.text = "街道が2本建設されました！\n\n道をつなげたい時に非常に便利です。";
        SetButton("次へ");
        yield return StartCoroutine(WaitForButtonPress());
        
        if (spawnedRoad1) Destroy(spawnedRoad1);
        if (spawnedRoad2) Destroy(spawnedRoad2);

        // Scene B
        ShowCard(CenterCardImage, SpriteYearOfPlenty);
        ClearHandCards();
        GuideText.text = "次は「発見」です。\n\n山札から好きな資源カードを\n【2枚】受け取ることができます。";
        yield return new WaitForSeconds(1.5f);
        SetButton("発見を使う");
        yield return StartCoroutine(WaitForButtonPress());
        yield return StartCoroutine(SpawnCard(3)); 
        yield return StartCoroutine(SpawnCard(4)); 
        GuideText.text = "資源を2枚獲得しました！";
        SetButton("次へ");
        yield return StartCoroutine(WaitForButtonPress());

        // Scene C
        if (SlideMonopoly != null) BoardImage.sprite = SlideMonopoly;
        ShowCard(CenterCardImage, SpriteMonopoly);
        ClearHandCards();
        GuideText.text = "最後は「独占」です。\n\n資源を1種類指定し、\n他の全員からその資源をすべて奪います。";
        yield return new WaitForSeconds(1.5f);
        SetButton("独占を使う (羊を指定)");
        yield return StartCoroutine(WaitForButtonPress());
        for(int i=0; i<5; i++) {
            yield return StartCoroutine(SpawnCard(2)); 
            yield return new WaitForSeconds(0.05f); 
        }
        GuideText.text = "大量の羊毛を奪いました！";
        SetButton("終了");
        yield return StartCoroutine(WaitForButtonPress());
        
        // ★追加: クリア画面を表示！
        var app = GetComponent<AppManager>();
        if (app != null) app.ShowClearPanel();
        
        GuideText.text = "実践2 クリア！";
    }

    // =================================================================
    // セクション3：ボーナスカード (BonusCardImageを使用)
    // =================================================================
    IEnumerator Flow_BonusCards()
    {
        Section3Container.SetActive(true);
        SetHeaderTitle("ボーナスカードについて");

        // Scene A
        if (SlideLongestRoad != null) BoardImage.sprite = SlideLongestRoad;
        
        // ★修正: BonusCardImageは最初は隠す
        if(BonusCardImage) BonusCardImage.gameObject.SetActive(false);

        GuideText.text = "【実践3】ボーナスカード\n\nカタンには条件を満たすと貰える\n特別な2点カードがあります。\n\nまずは「最長交易路」です。";
        yield return new WaitForSeconds(5.0f);
        GuideText.text = "自分の街道を【5本以上】長くつなげると\nこの権利を獲得できます。\n\nあと1本で5本になります。";
        
        SetButton("街道を伸ばす");
        yield return StartCoroutine(WaitForButtonPress());

        if (RoadPrefab && RoadAnchor_Longest) {
            spawnedRoad3 = Instantiate(RoadPrefab, RoadAnchor_Longest);
        }

        // ★修正: ここでBonusCardImageを表示！
        ShowCard(BonusCardImage, SpriteLongestRoad);
        
        GuideText.text = "「最長交易路」を獲得しました！\n\nただし、他の誰かが自分より長くした場合は\nその人に権利が移動します。";
        
        SetButton("次へ");
        yield return StartCoroutine(WaitForButtonPress());
        if (spawnedRoad3) Destroy(spawnedRoad3);


        // Scene B
        if (SlideLargestArmy != null) BoardImage.sprite = SlideLargestArmy;
        if(BonusCardImage) BonusCardImage.gameObject.SetActive(false);

        GuideText.text = "次は「最大騎士力」です。\n\n騎士カードを【3回以上】使用すると\nこの権利を獲得できます。";
        yield return new WaitForSeconds(1.0f);
        GuideText.text = "現在、すでに2回の騎士を使用済みです。\n3回目を使ってみましょう。";

        SetButton("騎士カードを使う");
        yield return StartCoroutine(WaitForButtonPress());

        if (KnightCardPrefab && KnightAnchor_Army) {
            spawnedKnight = Instantiate(KnightCardPrefab, KnightAnchor_Army);
            spawnedKnight.transform.localPosition = Vector3.zero;
            Button btn = spawnedKnight.GetComponent<Button>();
            if(btn) btn.interactable = false;
        }

        yield return new WaitForSeconds(0.5f);

        // ★修正: BonusCardImageを表示
        ShowCard(BonusCardImage, SpriteLargestArmy);

        GuideText.text = "「最大騎士力」を獲得しました！\n\nこれも他の誰かが自分より多く騎士を使うと\n権利を奪われてしまいます。";

        SetButton("終了");
        yield return StartCoroutine(WaitForButtonPress());
        
        // ★追加: クリア画面を表示！
        var app = GetComponent<AppManager>();
        if (app != null) app.ShowClearPanel();
        
        GuideText.text = "実践3 クリア！";
    }

    // -----------------------------------------------------------------
    // ヘルパー関数群
    // -----------------------------------------------------------------
    // ★改良：どのImageに出すか指定できるように変更
    void ShowCard(Image targetImage, Sprite sprite) {
        if (targetImage != null && sprite != null) {
            targetImage.gameObject.SetActive(true);
            targetImage.sprite = sprite;
        }
    }

    void SetButton(string label) {
        if (ActionButton) {
            ActionButton.gameObject.SetActive(true);
            ActionButton.interactable = true;
            ActionButton.onClick.RemoveAllListeners();
            ActionButton.onClick.AddListener(OnClickAction);
        }
        if (ActionButtonText) ActionButtonText.text = label;
    }

    IEnumerator WaitForButtonPress() {
        if(ActionButton) ActionButton.interactable = true;
        isButtonClicked = false;
        yield return new WaitUntil(() => isButtonClicked);
        if(ActionButton) ActionButton.interactable = false;
        if(ActionButton) ActionButton.gameObject.SetActive(false); 
    }

    void SetHeaderTitle(string title) { if (HeaderTitleText != null) HeaderTitleText.text = title; }

    IEnumerator SpawnCard(int typeIndex) {
        if (ResourceCardPrefabs != null && typeIndex < ResourceCardPrefabs.Length) {
            GameObject p = ResourceCardPrefabs[typeIndex];
            if (CardSlotContainer && p) Instantiate(p, CardSlotContainer).transform.localScale = Vector3.one;
        }
        yield return new WaitForSeconds(0.2f);
    }

    void ClearHandCards() {
        if (CardSlotContainer) foreach (Transform child in CardSlotContainer) Destroy(child.gameObject);
    }

    public void OnClickAction() { isButtonClicked = true; }
}