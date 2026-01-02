using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Chapter1Manager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject PracticePanel;
    public TextMeshProUGUI GuideText;
    
    [Header("Practice Containers")]
    public GameObject DiceContainer;   // 実践1用
    public GameObject BoardContainer;  // 実践2用

    [Header("Dice Elements")]
    public Image DiceImage1;           // 左のサイコロ画像
    public Image DiceImage2;           // 右のサイコロ画像
    public Button RollButton;          // 「サイコロを振る」ボタン
    public Sprite[] DiceSprites;       // サイコロの目画像（0:1の目, 1:2の目... 5:6の目）

    // 内部フラグ
    private bool isRolling = false;

    // --- 起動処理 ---
    public void StartPractice(int sectionIndex)
    {
        DiceContainer.SetActive(false);
        BoardContainer.SetActive(false);

        if (sectionIndex == 0)
        {
            // 実践1：順番決め
            StartCoroutine(Flow_DicePractice());
        }
        else if (sectionIndex == 1)
        {
            // 実践2：開拓地設置（※次回実装）
            // StartCoroutine(Flow_SettlementPractice());
        }
    }

    // --- 実践1：サイコロのフロー ---
    IEnumerator Flow_DicePractice()
    {
        DiceContainer.SetActive(true);
        GuideText.text = "【実践1】順番決め\nカタンでは、最初にサイコロを振って\n数字が一番大きい人がスタートプレイヤーになります。\n\n「振る」ボタンを押してください。";

        // ボタンを表示して待機
        RollButton.gameObject.SetActive(true);
        RollButton.interactable = true;
        
        // ユーザーがボタンを押すまでここで待機（isRollingがtrueになるのを待つ）
        yield return new WaitUntil(() => isRolling);

        // ボタンを隠す
        RollButton.gameObject.SetActive(false);
        GuideText.text = "サイコロを振っています...";

        // サイコロがパラパラ変わる演出（1.5秒間）
        float duration = 1.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // ランダムな画像を表示
            DiceImage1.sprite = DiceSprites[Random.Range(0, 6)];
            DiceImage2.sprite = DiceSprites[Random.Range(0, 6)];
            
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f); // 0.1秒ごとに切り替え
        }

        // ★イカサマ発動：必ず合計「11」か「12」が出るようにする
        // （例：左5, 右6 で11にする）
        DiceImage1.sprite = DiceSprites[4]; // 5の目
        DiceImage2.sprite = DiceSprites[5]; // 6の目

        // 結果発表
        GuideText.text = "「11」が出ました！\n最も大きい数字です。\nあなたが1番手のプレイヤーになりました。";

        // 少し待ってからクリア画面へ（または次の案内）
        yield return new WaitForSeconds(2.0f);
        
        // ここに「クリア」の処理を入れます
        Debug.Log("実践1 クリア！");
        // AppManager.Instance.ShowClearScreen(); // ※クリア画面の実装は後ほど
    }

    // ボタンから呼ばれる関数
    public void OnClickRollButton()
    {
        isRolling = true;
    }
}