using UnityEngine;
using UnityEngine.EventSystems;

public static class TossVisuals
{
    public static LineRenderer Create(Transform parent, string name, Material material, float width, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        LineRenderer line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.material = material != null ? material : new Material(Shader.Find("Sprites/Default"));
        line.widthMultiplier = width;
        line.sortingOrder = sortingOrder;
        line.numCapVertices = 4;
        line.numCornerVertices = 2;
        line.enabled = false;
        return line;
    }

    public static void Circle(LineRenderer line, Vector2 centre, float radius, int segments = 64)
    {
        line.loop = true;
        line.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float a = i / (float)segments * Mathf.PI * 2f;
            line.SetPosition(i, new Vector3(centre.x + Mathf.Cos(a) * radius, centre.y + Mathf.Sin(a) * radius, 0f));
        }
    }

    public static void Box(LineRenderer line, Vector2 centre, Vector2 size)
    {
        Vector2 h = size * 0.5f;
        line.loop = true;
        line.positionCount = 4;
        line.SetPosition(0, new Vector3(centre.x - h.x, centre.y - h.y, 0f));
        line.SetPosition(1, new Vector3(centre.x - h.x, centre.y + h.y, 0f));
        line.SetPosition(2, new Vector3(centre.x + h.x, centre.y + h.y, 0f));
        line.SetPosition(3, new Vector3(centre.x + h.x, centre.y - h.y, 0f));
    }

    public static void Segment(LineRenderer line, Vector2 a, Vector2 b)
    {
        line.loop = false;
        line.positionCount = 2;
        line.SetPosition(0, new Vector3(a.x, a.y, 0f));
        line.SetPosition(1, new Vector3(b.x, b.y, 0f));
    }

    public static void SetColor(LineRenderer line, Color colour)
    {
        line.startColor = colour;
        line.endColor = colour;
    }

    public static Vector2 MouseWorld()
    {
        Camera cam = Camera.main;
        if (cam == null) return Vector2.zero;
        return cam.ScreenToWorldPoint(Input.mousePosition);
    }

    public static bool PointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
