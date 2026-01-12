using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapNode : MonoBehaviour
{
    public int NodeID;
    public Button myButton;
    private Image myImage; // 自分の画像コンポーネント

    private System.Action<int> onClickCallback;
    private Coroutine blinkCoroutine;

    void Awake()
    {
        myButton = GetComponent<Button>();
        myImage = GetComponent<Image>();
        
        // 初期状態は透明にしておく
        if (myImage != null)
        {
            Color c = myImage.color;
            c.a = 0f; // 透明度0
            myImage.color = c;
        }

        myButton.onClick.AddListener(() => onClickCallback?.Invoke(NodeID));
    }

    public void Setup(System.Action<int> callback)
    {
        onClickCallback = callback;
    }

    // ★追加：点滅開始
    public void StartBlinking(Sprite highlightSprite)
    {
        if (myImage == null) return;

        // 画像をセットして見えるようにする
        myImage.sprite = highlightSprite;
        myImage.raycastTarget = true; // クリック判定ON

        // すでに点滅中ならリセット
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    // ★追加：点滅停止
    public void StopBlinking()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = null;

        if (myImage != null)
        {
            // 透明に戻す
            Color c = myImage.color;
            c.a = 0f;
            myImage.color = c;
            
            // 画像も空にしておく（念のため）
            myImage.sprite = null;
        }
    }

    // 点滅アニメーションの中身
    IEnumerator BlinkRoutine()
    {
        float speed = 3.0f; // 点滅の速さ
        while (true)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * speed)); // 0〜1を行き来する
            // 最低でも0.3くらいは見せたい（完全に消えると場所を見失うため）
            alpha = 0.3f + (alpha * 0.7f); 

            Color c = Color.yellow; // ★黄色く光らせる
            c.a = alpha;
            myImage.color = c;

            yield return null;
        }
    }
}