using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CrusadeMap : MonoBehaviour
{
    public static CrusadeMap Instance;

    private Node rootNode;
    private Node endNode;

    [SerializeField] private TreeStructureData treeStructureData;
    [SerializeField] private Transform playerIcon;

    [Header("Debug Read")]
    [Space(10)]
    [SerializeField] private List<Node> nodes = new List<Node>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        //Create the final node where the tree will start
        Node firstNode = CreateNode("Root", 0);
        rootNode = firstNode;
        rootNode.SetCrusadeEvent(CrusadeEvent.START);
        rootNode.nivel = 0;
        rootNode.SetBlocked(true);
        nodes.Add(rootNode);

        //Create the final node, where all the all will connect
        Node lastNode = CreateNode("End", treeStructureData.treeHeight); ;
        endNode = lastNode;
        endNode.SetCrusadeEvent(CrusadeEvent.BOSS);
        endNode.nivel = treeStructureData.treeHeight;
        nodes.Add(endNode);

        CreateTree(rootNode, 1);
        OrganizeTree();
        ReplaceGapConnections();
        AddTreePaths();
        CenterTreeInScene();
        RemoveDuplicatesNodes();
        SetCurrentNode(rootNode);

    }

    private void CreateTree(Node node, int currentHeight)
    {
        if (currentHeight >= treeStructureData.treeHeight)
        {
            node.AddConnection(endNode); // Adiciona o último nó (BOSS) da fase
            return;
        }

        if (!treeStructureData.fixedWidthTree)
        {
            int numConnections = Random.Range(1, treeStructureData.maxNodeConnections + 1); // Número aleatório de conexões para o nó atual        
            if (currentHeight == 1)
                numConnections = Random.Range(2, treeStructureData.maxNodeConnections + 1); // Número aleatório de conexões para o nó atual    
            AddConnections(numConnections, currentHeight, node);
        }
        else
        {
            int nodesLeft = treeStructureData.maxTreeWidth - GetWidthOfNivelTree(currentHeight);
            int numConnections = Random.Range(1, nodesLeft + 1);
            if (nodesLeft <= 0)
                numConnections = 0;

            if (currentHeight == 1)
                numConnections = treeStructureData.maxTreeWidth;
            AddConnections(numConnections, currentHeight, node);
        }
    }


    private void AddConnections(int numConnections, int currentHeight, Node node)
    {
        if (GetWidthOfNivelTree(currentHeight) < treeStructureData.maxTreeWidth)
        {
            for (int i = 0; i < numConnections; i++)
            {
                Node childNode = CreateNode($"Nível {currentHeight} - Nó {i}", currentHeight);
                childNode.nivel = currentHeight;
                node.AddConnection(childNode);
                childNode.transform.SetParent(node.transform);
                childNode.parents.Add(node);
                nodes.Add(childNode);
                CreateTree(childNode, currentHeight + 1);
            }
        }
    }

    private int CountTotalConnections(Node node)
    {
        int totalConnections = 0;

        if (node.connections != null)
        {
            totalConnections += node.connections.Length;

            foreach (Node connection in node.connections)
            {
                totalConnections += CountTotalConnections(connection);
            }
        }

        return totalConnections;
    }

    private void ReplaceGapConnections()
    {
        foreach (Node node in nodes)
        {
            if (node.connections.Length == 0)
            {
                var n = GetNodesOfNivel(node.nivel + 1);
                if (n.Count > 0)
                {
                    node.AddConnection(n[n.Count - 1]);
                    n[n.Count - 1].parents.Add(node);
                }

            }
        }
    }

    private Node CreateNode(string eventName, int height)
    {
        Node node = Instantiate(treeStructureData.nodePrefab);
        node.gameObject.name = eventName;
        node.eventName = eventName;
        node.SetCrusadeEvent(CrusadeEvent.COMBAT);

        if (height >= 2)
        {
            if (Random.value > 0.3f)
                node.SetCrusadeEvent(CrusadeEvent.COMBAT);
            else
                node.SetCrusadeEvent(CrusadeEvent.HEART);
        }
        return node;
    }

    private void TraverseConnections(Node node)
    {
        foreach (Node connection in node.connections)
        {
            connection.SetBlocked(false);
            TraverseConnections(connection);
        }
    }

    private int GetWidthOfNivelTree(int nivel)
    {
        int count = 0;
        foreach (var node in nodes)
        {
            if (node.nivel == nivel)
                count++;
        }
        return count;
    }

    private List<Node> GetNodesOfNivel(int nivel)
    {
        List<Node> nodesOnNivel = new List<Node>();
        foreach (var node in nodes)
        {
            if (node.nivel == nivel)
                nodesOnNivel.Add(node);
        }
        return nodesOnNivel;
    }


    private void OrganizeTree()
    {
        for (int i = 0; i <= treeStructureData.treeHeight; i++)
        {
            List<Node> nivelNodes = nodes.Where(x => x.nivel == i).ToList();
            if (nivelNodes.Count > 0)
                OrganizeNivel(nivelNodes);
        }
    }

    private void OrganizeNivel(List<Node> treeNodes)
    {
        int centerIndex = 0;
        if (treeNodes.Count % 2 == 0)
            centerIndex = Mathf.RoundToInt(treeNodes.Count / 2) - 1;
        else
            centerIndex = Mathf.RoundToInt(treeNodes.Count / 2);

        for (int i = 0; i < treeNodes.Count; i++)
        {
            float posX = (i - centerIndex);
            if (treeNodes[i].crusadeEvent != CrusadeEvent.BOSS)
            {
                float noiseV = Random.Range(treeStructureData.noiseVertical.x, treeStructureData.noiseVertical.y);
                float noiseH = Random.Range(treeStructureData.noiseHorizontal.x, treeStructureData.noiseHorizontal.y);
                Vector3 pos = new Vector3(posX + (treeStructureData.nivelOffset.x * posX) + noiseH,
                    treeNodes[i].nivel + (treeNodes[i].nivel * treeStructureData.nivelOffset.y) + noiseV,
                    -(0.01f * i));
                treeNodes[i].transform.position = pos;
            }
            else
            {
                Vector3 pos = new Vector3(posX +
                    (treeStructureData.nivelOffset.x * posX),
                    treeNodes[i].nivel + (treeNodes[i].nivel * treeStructureData.endNodeOffset.y),
                    -(0.01f * i));
                treeNodes[i].transform.position = pos;
            }
        }
    }

    private void AddTreePaths()
    {
        foreach (var node in nodes)
            node.AddPaths();
    }

    private void CenterTreeInScene()
    {
        endNode.transform.SetParent(rootNode.transform);
        rootNode.transform.position = new Vector3(0, -(endNode.transform.position.y - rootNode.transform.position.y) / 2, 0);
    }

    private void RemoveDuplicatesNodes()
    {
        foreach (var node in nodes)
        {
            if (node.nivel > 1 && node.nivel < treeStructureData.treeHeight)
            {
                foreach (var connection in node.connections)
                {
                    if (node.crusadeEvent != CrusadeEvent.COMBAT && node.crusadeEvent == connection.crusadeEvent)
                    {
                        connection.SetCrusadeEvent(CrusadeEvent.COMBAT);
                    }
                }
            }
        }
    }

    public void SetCurrentNode(Node selectNode)
    {
        foreach (var node in nodes)
        {
            node.currentNode = false;
            node.SetSelectable(false);
        }
        selectNode.currentNode = true;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(playerIcon.DOMove(selectNode.transform.position, 0.5f));
        sequence.OnComplete(() => {
            ValidNodesSelectable();
            //Call scene of level
        });
    }

 

    public void ValidNodesSelectable()
    {
        foreach (var node in nodes)
        {
            node.SetBlocked(true);
            node.SetSelectable(false);
        }

        foreach (var node in nodes)
        {
            if (node.currentNode)
            {
                node.SetBlocked(false);
                node.SetSelectable(false);
                foreach (var childNode in node.connections)
                {
                    childNode.SetBlocked(false);
                    childNode.SetSelectable(true);
                }
                break;
            }
        }

        foreach (var node in nodes)
        {
            if (node.selectable == true)
                TraverseConnections(node);
        }
    }
}