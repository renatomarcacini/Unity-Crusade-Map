using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> parents = new List<Node>();
    public bool currentNode;
    public bool selectable;
    public bool blocked;

    public int nivel;
    public CrusadeEvent crusadeEvent = CrusadeEvent.NONE;
    public string eventName; // Nome do evento
    public Node[] connections; // Array de nós para armazenar as conexões

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite start, combat, heart, boss;
    [SerializeField] private Material pathMaterial;
    [SerializeField] private Material desactivePathMaterial;

    [SerializeField] private GameObject selectableBackground;

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Método para adicionar uma conexão a este nó
    public void AddConnection(Node node)
    {
        // Verifica se o array de conexões já existe
        if (connections == null)
        {
            connections = new Node[] { node };
        }
        else
        {
            // Cria um novo array com tamanho maior para adicionar a nova conexão
            Node[] newConnections = new Node[connections.Length + 1];

            // Copia as conexões existentes para o novo array
            for (int i = 0; i < connections.Length; i++)
            {
                newConnections[i] = connections[i];
            }

            // Adiciona a nova conexão ao final do novo array
            newConnections[connections.Length] = node;

            // Substitui o array de conexões pelo novo array
            connections = newConnections;
        }
    }

    public void SetCrusadeEvent(CrusadeEvent crusade)
    {
        crusadeEvent = crusade;
        switch (crusadeEvent)
        {
            case CrusadeEvent.START:
                spriteRenderer.sprite = start;
                break;
            case CrusadeEvent.COMBAT:
                spriteRenderer.sprite = combat;
                break;
            case CrusadeEvent.HEART:
                spriteRenderer.sprite = heart;
                break;
            case CrusadeEvent.BOSS:
                spriteRenderer.sprite = boss;
                break;
        }
    }

    public void AddPaths()
    {
        float posZ = 0.1f;
        foreach (var node in connections)
        {
            GameObject render = new GameObject();
            LineRenderer renderer = render.AddComponent<LineRenderer>();
            renderer.material = pathMaterial;
            renderer.useWorldSpace = false;
            renderer.startWidth = 0.1f;
            renderer.endWidth = 0.1f;
            renderer.positionCount = 2;
            renderer.textureMode = LineTextureMode.Tile;
            renderer.textureScale = new Vector2(3f, 1);
            renderer.SetPosition(0, new Vector3(transform.position.x, transform.position.y, posZ));
            renderer.SetPosition(1, new Vector3(node.transform.position.x, node.transform.position.y, posZ));
            render.transform.SetParent(transform);
            lineRenderers.Add(renderer);
        }
    }

    public void SetSelectable(bool value)
    {
        selectable = value;
        selectableBackground.gameObject.SetActive(value);
    }

    public void SetBlocked(bool value)
    {
        if (value)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.05f);
            foreach (var item in lineRenderers)
                item.material = desactivePathMaterial;
        }
        else
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
            foreach (var item in lineRenderers)
                item.material = pathMaterial;
        }

    }

    private void OnMouseDown()
    {
        if (selectable)
        {
            CrusadeMap.Instance.SetCurrentNode(this);
        }
    }
}
