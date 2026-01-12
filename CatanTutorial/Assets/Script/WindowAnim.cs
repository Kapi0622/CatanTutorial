using UnityEngine;
using System.Collections;

public class WindowAnim : MonoBehaviour
{
    [Header("Animation Settings")]
    public float Duration = 0.4f;       // アニメーションにかかる時間
    public float MaxScale = 1.1f;       // 一瞬大きくなるサイズ（1.1倍）

    // パネルが表示されるたびに自動で呼ばれる
    void OnEnable()
    {
        // アニメーション開始
        StartCoroutine(AnimatePopup());
    }

    IEnumerator AnimatePopup()
    {
        float time = 0f;

        // 1. 拡大フェーズ（0 → 1.1倍まで）
        // 勢いよく飛び出す感じ
        while (time < Duration * 0.7f)
        {
            time += Time.unscaledDeltaTime;
            float t = time / (Duration * 0.7f);
            // EaseOutBackのような動き（急激に大きくなる）
            float scale = Mathf.Lerp(0f, MaxScale, Mathf.SmoothStep(0f, 1f, t));
            transform.localScale = Vector3.one * scale;
            yield return null;
        }

        // 2. 縮小フェーズ（1.1倍 → 1.0倍へ）
        // 最後に少し戻ることで「弾力」を表現
        time = 0f;
        while (time < Duration * 0.3f)
        {
            time += Time.unscaledDeltaTime;
            float t = time / (Duration * 0.3f);
            float scale = Mathf.Lerp(MaxScale, 1f, t);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }

        // 最終的にサイズを1.0に確定
        transform.localScale = Vector3.one;
    }
}