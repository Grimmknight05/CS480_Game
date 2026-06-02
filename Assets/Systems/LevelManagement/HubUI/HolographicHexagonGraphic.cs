using UnityEngine;
using UnityEngine.UI;

public class HolographicHexagonGraphic : MaskableGraphic
{
    public bool OutlineOnly { get; set; }
    public float OutlineThickness { get; set; } = 8f;

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        Rect rect = rectTransform.rect;
        float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
        Vector2 center = rect.center;
        Vector2[] outer = BuildHexagon(center, radius);

        if (!OutlineOnly)
        {
            int centerIndex = AddVertex(vertexHelper, center);
            for (int i = 0; i < outer.Length; i++)
            {
                int current = AddVertex(vertexHelper, outer[i]);
                int next = AddVertex(vertexHelper, outer[(i + 1) % outer.Length]);
                vertexHelper.AddTriangle(centerIndex, current, next);
            }
            return;
        }

        float innerRadius = Mathf.Max(radius - OutlineThickness, 0f);
        Vector2[] inner = BuildHexagon(center, innerRadius);

        for (int i = 0; i < outer.Length; i++)
        {
            int next = (i + 1) % outer.Length;
            int outerCurrent = AddVertex(vertexHelper, outer[i]);
            int outerNext = AddVertex(vertexHelper, outer[next]);
            int innerCurrent = AddVertex(vertexHelper, inner[i]);
            int innerNext = AddVertex(vertexHelper, inner[next]);

            vertexHelper.AddTriangle(outerCurrent, outerNext, innerNext);
            vertexHelper.AddTriangle(outerCurrent, innerNext, innerCurrent);
        }
    }

    private Vector2[] BuildHexagon(Vector2 center, float radius)
    {
        Vector2[] points = new Vector2[6];
        for (int i = 0; i < points.Length; i++)
        {
            float angle = Mathf.Deg2Rad * (90f + i * 60f);
            points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        return points;
    }

    private int AddVertex(VertexHelper vertexHelper, Vector2 position)
    {
        int index = vertexHelper.currentVertCount;
        vertexHelper.AddVert(position, color, Vector2.zero);
        return index;
    }
}
