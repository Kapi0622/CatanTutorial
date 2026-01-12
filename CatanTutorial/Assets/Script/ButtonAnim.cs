using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // イベント制御に必要
using System.Collections;

// ボタンに「押した時の凹み」を追加するスクリプト
[RequireComponent(typeof(Button))]
public class ButtonAnim : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animation Settings")]
    public float PressedScale = 0.95f; // 押した時の縮小率
    public float Duration = 0.1f;      // 変化にかかる時間

    private Vector3 originalScale;
    private Coroutine animateCoroutine;

    void Start()
    {
        originalScale = transform.localScale;
    }

    // 押した瞬間
    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsInteractable())
        {
            StartScale(originalScale * PressedScale);
        }
    }

    // 指を離した瞬間
    public void OnPointerUp(PointerEventData eventData)
    {
        StartScale(originalScale);
    }

    // ボタンが有効かチェック
    bool IsInteractable()
    {
        var btn = GetComponent<Button>();
        return btn != null && btn.interactable;
    }

    // アニメーション実行処理
    void StartScale(Vector3 targetScale)
    {
        if (animateCoroutine != null) StopCoroutine(animateCoroutine);
        animateCoroutine = StartCoroutine(ScaleTo(targetScale));
    }

    IEnumerator ScaleTo(Vector3 target)
    {
        float time = 0;
        Vector3 start = transform.localScale;

        while (time < Duration)
        {
            time += Time.unscaledDeltaTime; // unscaledならTime.timeScale=0でも動く
            transform.localScale = Vector3.Lerp(start, target, time / Duration);
            yield return null;
        }
        transform.localScale = target;
    }
    
    // オブジェクトが無効になったらサイズを戻す（バグ防止）
    void OnDisable()
    {
        transform.localScale = originalScale;
    }
}