using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    RectTransform gridContainer;

    [Header("SIZE GRID")]
    [SerializeField][Range(5, 14)] public int amountPiecesX;
    [SerializeField][Range(5, 9)] public int amountPiecesY;
    
    [HideInInspector] public GridLayoutGroup layoutGrid;

    float fixOffsetX;
    float fixOffsetY;

    [HideInInspector] public int widthGridContainer;
    [HideInInspector] public int heightGridContainer;

    void Start()
    {
        layoutGrid = gameObject.GetComponent<GridLayoutGroup>();
        gridContainer = gameObject.GetComponent<RectTransform>();

        fixOffsetX = layoutGrid.cellSize.x + layoutGrid.spacing.x;
        fixOffsetY = layoutGrid.cellSize.y + layoutGrid.spacing.y;

        widthGridContainer = (int)(amountPiecesX * fixOffsetX);
        heightGridContainer = (int)(amountPiecesY * fixOffsetY);

        gridContainer.sizeDelta = new Vector2(widthGridContainer, heightGridContainer);
    }
}
