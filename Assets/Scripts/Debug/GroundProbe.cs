 using UnityEngine;
 public class GroundProbe : MonoBehaviour
{
    [SerializeField] private LayerMask _mask;
    [SerializeField] private float _distance = 1.5f;
    private void Update()
    {
        Vector2 origin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, _distance, _mask);
        Color c = hit ? Color.green : Color.red;
        Debug.DrawLine(origin, origin + Vector2.down * _distance, c);
        if (hit) Debug.Log($"Hit {hit.collider.name} at distance {hit.distance:F2}");
    }
 }