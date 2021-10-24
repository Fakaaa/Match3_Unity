using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBehaviour : MonoBehaviour
{
    [SerializeField] NodeGrid prefabGridNode;

    public float spacingX;
    public float spacingY;
    [Range(6,14)]public int amountPiecesX;
    [Range(6,8)] public int amountPiecesY;
    public bool nodesGenerated;
    public int amountNodesGenerated;

    public NodeGrid[,] outGridNodes;
    public NodeGrid[,] gridNodes;
    public Vector3 initialPosition;
    public Transform nodesParent;

    void Start()
    {
        amountNodesGenerated = 0;
        gridNodes = new NodeGrid[amountPiecesX,amountPiecesY];
        outGridNodes = new NodeGrid[amountPiecesX, amountPiecesY +1];
        nodesGenerated = false;
        initialPosition = transform.position - new Vector3((spacingX * amountPiecesX)*0.5f, -(spacingY * amountPiecesY) * 0.5f, 1);
    }

    void Update()
    {
        if(!nodesGenerated)
        {
            StartCoroutine(GenerateGridNodes());
            nodesGenerated = true;
        }
    }

    public IEnumerator GenerateGridNodes()
    {
        for (int i = 0; i < amountPiecesY; i++)
        {
            for (int j = 0; j < amountPiecesX; j++)
            {
                gridNodes[j,i] = Instantiate(prefabGridNode, new Vector3(initialPosition.x + (j * spacingX),
                    initialPosition.y - (i * spacingY), 1), Quaternion.identity, nodesParent);

                //if(outGridNodes[j, amountPiecesY] == null)
                //    outGridNodes[j,amountPiecesY] = Instantiate(prefabGridNode, new Vector3(initialPosition.x + (j * spacingX),
                //    initialPosition.y + (i * spacingY), 1), Quaternion.identity, nodesParent);

                amountNodesGenerated++;

                yield return new WaitForEndOfFrame();
            }
        }
        yield return null;
    }
}
