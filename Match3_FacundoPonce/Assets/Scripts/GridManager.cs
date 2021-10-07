using UnityEngine;

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    RectTransform gridContainer;

    [Header("SIZE GRID")]
    [SerializeField][Range(30, 60)] public int widthGrid;
    [SerializeField][Range(18, 36)] public int heightGrid;

    void Start()
    {
        gridContainer = gameObject.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (gridContainer == null)
            return;

        gridContainer.sizeDelta = new Vector2(widthGrid, heightGrid);
    }
}
