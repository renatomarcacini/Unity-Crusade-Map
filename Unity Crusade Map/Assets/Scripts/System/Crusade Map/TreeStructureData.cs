using UnityEngine;

[CreateAssetMenu(fileName = "NewTreeMapStructure", menuName = "ScriptableObjects/CrusadeMap/TreeMapStructure")]
public class TreeStructureData : ScriptableObject
{
    [Header("Tree Structure")]
    [Space(10)]
    public Node nodePrefab;
    public bool fixedWidthTree = false;
    public int treeHeight = 3;
    public int maxTreeWidth = 5;
    public int maxNodeConnections = 4; 

    [Header("Tree Visual Settings")]
    [Space(10)]
    public Vector2 nivelOffset = new Vector2(0.5f, 0);
    public Vector2 noiseVertical = new Vector2(-0.15f, 0.15f);
    public Vector2 noiseHorizontal = new Vector2(-0.15f, 0.15f);
    public Vector2 endNodeOffset = new Vector2(0, 1f);
}
