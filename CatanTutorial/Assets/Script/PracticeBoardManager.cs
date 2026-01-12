using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PracticeBoardManager : MonoBehaviour
{
    [Header("Settings")]
    public Transform NodeContainer; 
    public GameObject SettlementPrefab; // 家のプレハブ
    public GameObject RoadPrefab;       // ★追加：道のプレハブ
    public GameObject CityPrefab;
    public Sprite HighlightSprite;
    
    private List<MapNode> allNodes = new List<MapNode>();
    
    private List<GameObject> spawnedPieces = new List<GameObject>();

    public void Initialize()
    {
        allNodes.Clear();
        foreach (Transform child in NodeContainer)
        {
            MapNode node = child.GetComponent<MapNode>();
            if (node != null) allNodes.Add(node);
        }
    }
    
    public void UpgradeToCity(int nodeId, Color color)
    {
        MapNode targetNode = allNodes.Find(n => n.NodeID == nodeId);

        if (targetNode != null)
        {
            // 1. その場所にすでにある駒（開拓地）を探して削除
            // Nodeの子オブジェクトになっているはず
            foreach (Transform child in targetNode.transform)
            {
                Destroy(child.gameObject);
            }

            // 2. 都市を生成
            GameObject newPiece = Instantiate(CityPrefab, targetNode.transform);
            
            // お片付けリストに追加
            spawnedPieces.Add(newPiece);

            Image img = newPiece.GetComponent<Image>();
            if (img != null) 
            {
                img.color = color;
                img.raycastTarget = false;
            }
        }
    }

    // 種類を指定してピースを置く（type 0:開拓地, 1:街道）
    public void SpawnPiece(int nodeId, Color color, int type)
    {
        MapNode targetNode = allNodes.Find(n => n.NodeID == nodeId);

        if (targetNode != null)
        {
            GameObject prefabToSpawn = (type == 0) ? SettlementPrefab : RoadPrefab;
            
            // ノードの位置・回転に合わせて生成
            GameObject newPiece = Instantiate(prefabToSpawn, targetNode.transform);
            
            spawnedPieces.Add(newPiece);
            
            // ★重要：道の場合、ボタンのサイズに引き伸ばされてしまうのを防ぐためサイズをリセット
            // もし表示がおかしい場合はここを調整します
            newPiece.transform.localScale = Vector3.one; 
            newPiece.transform.localPosition = Vector3.zero;
            newPiece.transform.localRotation = Quaternion.identity; // 親（ボタン）の回転に従う

            // 色を変える
            Image img = newPiece.GetComponent<Image>();
            if (img != null) 
            {
                img.color = color;
                img.raycastTarget = false;
            }
        }
    }
    
    public void ClearBoard()
    {
        // 1. 生成したピースを全部破壊する
        foreach (GameObject piece in spawnedPieces)
        {
            if (piece != null) Destroy(piece);
        }
        spawnedPieces.Clear(); // リストを空にする

        // 2. 点滅なども止める
        DisableAllNodes();
    }

    // (HighlightNode, DisableAllNodes, ClearBoard はそのまま)
    public void HighlightNode(int nodeId, System.Action<int> onClickAction)
    {
        foreach (var node in allNodes)
        {
            if (node.NodeID == nodeId)
            {
                // 正解の場所：押せるようにして、光らせる！
                node.myButton.interactable = true;
                node.Setup(onClickAction);

                // ★追加：点滅命令
                node.StartBlinking(HighlightSprite);
            }
            else
            {
                // 不正解の場所
                node.myButton.interactable = false;
                node.StopBlinking(); // 光っていたら消す
            }
        }
    }

    public void DisableAllNodes()
    {
        foreach (var node in allNodes)
        {
            node.myButton.interactable = false;
            node.StopBlinking(); // ★全員消灯
        }
    }
}