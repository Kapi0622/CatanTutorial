using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSE : MonoBehaviour
{
    [Header("Optional")]
    public AudioClip CustomSE; // ★追加：ここに入れたらその音が鳴る

    void Start()
    {
        Button btn = GetComponent<Button>();
        AppManager app = FindObjectOfType<AppManager>(); // AppManagerを探す

        if (btn != null && app != null)
        {
            // ボタンが押された時の処理を登録
            btn.onClick.AddListener(() => 
            {
                // もし個別の音が設定されていたら、それを鳴らす
                if (CustomSE != null)
                {
                    app.PlayCustomSE(CustomSE);
                }
                // 設定されていなければ、いつもの共通音を鳴らす
                else
                {
                    app.PlayClickSE();
                }
            });
        }
    }
}