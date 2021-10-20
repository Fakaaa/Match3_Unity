using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeGrid : MonoBehaviour
{
    [SerializeField] PieceType pieceOverNode;
    [SerializeField] Vector2 gridPosition;

    public PieceType GetPiece()
    {
        return pieceOverNode;
    }

    public Vector2 GetGridPos()
    {
        return gridPosition;
    }

    public void SetPieceOnNode(PieceType piece, int posColumns, int posRows)
    {
        pieceOverNode = piece;
        gridPosition = new Vector2(posColumns, posRows);
    }

    public void SetPieceOnNode(PieceType piece)
    {
        pieceOverNode = piece;
    }

    public void SetNodePosition(int gridColumn, int gridRow)
    {
        gridPosition = new Vector2(gridColumn, gridRow);
    }
}
