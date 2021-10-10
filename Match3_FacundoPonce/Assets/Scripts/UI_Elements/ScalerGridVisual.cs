using UnityEngine;

public class ScalerGridVisual : MonoBehaviour
{
    [SerializeField] public GridManager gridHandle;
    private RectTransform rect;

    private float valueX;
    private float valueY;

    void Start()
    {
        valueX = 100f;
        valueY = 100f;

        rect = gameObject.GetComponent<RectTransform>();

        valueX = valueX * ((gridHandle.widthGridContainer / 10) + 0.5f);
        valueY = valueY * ((gridHandle.heightGridContainer / 10) + 0.5f);

        rect.sizeDelta = new Vector2(valueX, valueY);
    }
}
