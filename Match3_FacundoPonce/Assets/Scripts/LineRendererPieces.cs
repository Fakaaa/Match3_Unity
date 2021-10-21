using UnityEngine;

public class LineRendererPieces : MonoBehaviour
{
    LineRenderer rendererLines;

    void Start()
    {
        rendererLines = GetComponent<LineRenderer>();
        rendererLines.positionCount = 0;

        if (PiecesManager.Instance != null)
        {
            PiecesManager.Instance.newPointLine += AddPointOnPiece;
            PiecesManager.Instance.clearLineMatches += ClearLine;
            PiecesManager.Instance.removePointStack += RemovePointOnPiece;
        }
    }

    private void OnDisable()
    {
        if(PiecesManager.Instance != null)
        {
            PiecesManager.Instance.newPointLine -= AddPointOnPiece;
            PiecesManager.Instance.clearLineMatches -= ClearLine;
            PiecesManager.Instance.removePointStack -= RemovePointOnPiece;
        }
    }

    public void AddPointOnPiece(int indexPoint)
    {
        if (rendererLines == null || PiecesManager.Instance == null)
            return;

        rendererLines.positionCount = PiecesManager.Instance.matchingPieces.Count;

        rendererLines.SetPosition(indexPoint-1, PiecesManager.Instance.matchingPieces.Peek().transform.position);
    }

    public void RemovePointOnPiece()
    {
        if (rendererLines == null || PiecesManager.Instance == null)
            return;

        rendererLines.positionCount = PiecesManager.Instance.matchingPieces.Count;
    }

    public void ClearLine()
    {
        rendererLines.positionCount = 0;
    }
}
