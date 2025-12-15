using UnityEngine;
using System.Collections.Generic;

// 1画面分のデータ構造
[System.Serializable]
public class ScenarioStep
{
    [Header("基本設定")]
    public string SpeakerName;      // 話す人の名前
    [TextArea(3, 10)]               // テキストエリアを広げる
    public string MainText;         // セリフ

    [Header("表示・再生素材")]
    public Sprite BgImage;          // 背景画像 (直接D&D)
    public Sprite CenterImage;      // 中央の画像 (直接D&D)
    public AudioClip VoiceClip;     // ボイス (直接D&D)
    public AudioClip SeClip;        // 効果音 (直接D&D)

    [Header("アクション設定")]
    public ActionType Action;       // Next, Click, Drag
    public string TargetObjectName; // 操作対象の名前 (ここだけは文字列で指定)
    
    // リストのインデックスで管理するため、IDは自動的に「リストの並び順」になります
}

public enum ActionType
{
    Next,  // 読むだけ
    Click, // クリック待ち
    Drag   // ドラッグ待ち
}

// これをアセットとして作成できるようにする
[CreateAssetMenu(fileName = "NewScenario", menuName = "Catan/ScenarioData")]
public class ScenarioData : ScriptableObject
{
    public List<ScenarioStep> Steps; // ここにステップをどんどん追加していく
}