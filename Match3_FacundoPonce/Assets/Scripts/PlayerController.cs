using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    public bool draging;
    CircleCollider2D coll;

    private void Start()
    {
        Cursor.visible = false;
        coll = gameObject.GetComponent<CircleCollider2D>();
    }
    void Update()
    {
        UpdatePosition();

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if(PiecesManager.Instance != null)
            {
                draging = false;
                PiecesManager.Instance.EndChain();
            }
        }
    }

    public void BlockPlayerInteraction()
    {
        coll.enabled = false;
    }

    public void UnblockPlayerInteraction()
    {
        coll.enabled = true;
    }

    public void UpdatePosition()
    {
        transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PieceHandler piece;
        if (collision.TryGetComponent<PieceHandler>(out piece))
        {
            piece.HoverAndChainPieces(draging);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        PieceHandler piece;
        if (collision.TryGetComponent<PieceHandler>(out piece))
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                draging = true;
                piece.StartChainAndPressPiece();
            }
            //if (!draging && PiecesManager.Instance.matchingPieces.Count < 1)
            //    piece.RemoveFromChain();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PieceHandler piece;
        if (collision.TryGetComponent<PieceHandler>(out piece))
        {
            piece.ExitPieceHover();
        }
    }
}
