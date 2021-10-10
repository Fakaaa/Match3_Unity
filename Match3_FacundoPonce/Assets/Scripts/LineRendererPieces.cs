using UnityEngine;

public class LineRendererPieces : MonoBehaviour
{
    [SerializeField] PiecesManager matchedPieces;
    LineRenderer rendererLines;

    void Start()
    {
        rendererLines = GetComponent<LineRenderer>();
        rendererLines.positionCount = 0;

        matchedPieces.newPointLine += AddPointOnPiece;
        matchedPieces.clearLineMatches += ClearLine;
        matchedPieces.removePointStack += RemovePointOnPiece;
    }

    private void OnDisable()
    {
        matchedPieces.newPointLine -= AddPointOnPiece;
        matchedPieces.clearLineMatches -= ClearLine;
        matchedPieces.removePointStack -= RemovePointOnPiece;
    }

    public void AddPointOnPiece(int indexPoint)
    {
        if (rendererLines == null || matchedPieces == null)
            return;

        rendererLines.positionCount = matchedPieces.matchingPieces.Count;

        rendererLines.SetPosition(indexPoint-1, matchedPieces.matchingPieces.Peek().transform.position + new Vector3(0,0,2));
    }

    public void RemovePointOnPiece()
    {
        if (rendererLines == null || matchedPieces == null)
            return;

        rendererLines.positionCount = matchedPieces.matchingPieces.Count;
    }

    public void ClearLine()
    {
        rendererLines.positionCount = 0;
    }
}
