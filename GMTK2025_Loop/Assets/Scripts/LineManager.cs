using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private List<Vector3> points;
    [SerializeField] private float spawnPointDistance;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private LayerMask pointLayer;

    private void Start()
    {
        points = new List<Vector3>();
        lineRenderer.positionCount = 2;
        points.Add(transform.position);
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
        Test();
    }

    private void Update()
    {
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, transform.position);
        CheckForLoop();

        if (Vector3.Distance(transform.position, points[points.Count - 1]) > spawnPointDistance)
        {
            AddPoint(transform.position);
        }
    }

    private void AddPoint(Vector3 point)
    {
        points.Add(point);
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        lineRenderer.positionCount = points.Count + 1;
        lineRenderer.SetPositions(points.ToArray());
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, transform.position);
    }

    private void CheckForLoop()
    {
        if (points.Count < 3) return;
        Vector2 lastSegmentp1 = new Vector2(points[points.Count - 1].x, points[points.Count - 1].y);
        Vector2 lastSegmentp2 = new Vector2(transform.position.x, transform.position.y);

        for (int i = 0; i < points.Count - 3; i++)
        {
            Vector2 currentSegmentp1 = new Vector2(points[i].x, points[i].y);
            Vector2 currentSegmentp2 = new Vector2(points[i + 1].x, points[i + 1].y);
            if (TryGetSegmentIntersection(lastSegmentp1, lastSegmentp2, currentSegmentp1, currentSegmentp2, out var point))
            {
                Debug.DrawLine(currentSegmentp1, currentSegmentp2, Color.red, 5);
                Debug.DrawLine(lastSegmentp1, lastSegmentp2, Color.blue, 5);
                List<Vector3> loopPoints = new List<Vector3>();
                for (int j = points.Count - 1; j >= i; j--)
                {
                    loopPoints.Add(points[j]);
                    points.Remove(points[j]);
                }
                AddPoint(point);
                ProcessShitInside(loopPoints);
                return;
            }
        }
    }

    private void ProcessShitInside(List<Vector3> loopPoints)
    {
        (Vector3, float) circle = GetSurroundingCircle(loopPoints.ToArray());
        Collider2D[] hits = Physics2D.OverlapCircleAll(new Vector2(circle.Item1.x, circle.Item1.y), circle.Item2, pointLayer);
        int counter1 = 0;
        int counter2 = 0;
        int counter3 = 0;
        foreach (var hit in hits)
        {
            if (IsPointInPolygon(loopPoints.ToArray(), hit.transform.position))
            {
                Point pt = hit.GetComponent<Point>();
                switch (pt.type)
                {
                    case 1: counter1++; break;
                    case 2: counter2++; break;
                    case 3: counter3++; break;
                }
                pt.Collect();
            }
        }
        int gained = 0;
        if (counter1 >= counter2 && counter1 >= counter3)
        {
            gained = counter1 - (counter2 > counter3 ? counter2 : counter3);
        }
        else if (counter2 >= counter1 && counter2 >= counter3)
        {
            gained = counter2 - (counter1 > counter3 ? counter1 : counter3);
        }
        else
        {
            gained = counter3 - (counter1 > counter2 ? counter1 : counter2);
        }
        gained = Mathf.Max(gained, 0);
        playerManager.AddPoints(gained);
    }

    private bool IsPointInPolygon(Vector3[] polygonPoints, Vector3 testPoint)
    {
        if (polygonPoints == null || polygonPoints.Length < 3)
        {
            Debug.LogError("Polygon must have at least 3 points.");
            return false;
        }

        bool isInside = false;
        int numVertices = polygonPoints.Length;

        // Iterate through each edge of the polygon
        for (int i = 0, j = numVertices - 1; i < numVertices; j = i++)
        {
            Vector3 p1 = polygonPoints[i];
            Vector3 p2 = polygonPoints[j];

            // Check if the ray from testPoint intersects the edge (p1, p2)
            // We cast a ray horizontally to the right from testPoint.
            // An intersection occurs if:
            // 1. The y-coordinate of the testPoint is between the y-coordinates of p1 and p2 (exclusive of one end to handle horizontal edges).
            // 2. The x-coordinate of the testPoint is to the left of the intersection point of the ray with the line segment.

            if (((p1.y <= testPoint.y && testPoint.y < p2.y) || (p2.y <= testPoint.y && testPoint.y < p1.y)) &&
                (testPoint.x < (p2.x - p1.x) * (testPoint.y - p1.y) / (p2.y - p1.y) + p1.x))
            {
                isInside = !isInside;
            }
        }
        return isInside;
    }
    private (Vector3, float) GetSurroundingCircle(Vector3[] polygonPoints)
    {
        Vector3 medium = Vector3.zero;
        foreach (var point in polygonPoints)
        {
            medium += point;
        }
        medium /= polygonPoints.Length;
        float furthestPointDistance = 0;
        foreach (var point in polygonPoints)
        {
            float dist = Vector3.Distance(medium, point);
            if (dist > furthestPointDistance) furthestPointDistance = dist;
        }
        return (medium, furthestPointDistance);
    }

    #region Intersect
    public bool TryGetSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersectionPoint)
    {
        intersectionPoint = Vector2.zero;

        Vector2 r = p2 - p1;
        Vector2 s = p4 - p3;

        float denominator = CrossProduct(r, s);

        // If denominator is 0, lines are parallel or collinear.
        if (Mathf.Approximately(denominator, 0))
        {
            // Check for collinearity and overlap
            if (Mathf.Approximately(CrossProduct(p3 - p1, r), 0))
            {
                // Lines are collinear. Check if they overlap.
                // This means projecting p3 and p4 onto the p1-p2 line
                // and checking if their projections overlap with p1-p2.

                // Simplified overlap check for collinear segments:
                // Check if one segment's end points are within the other segment.
                // Or if their bounding boxes overlap.

                // A more robust collinear overlap check:
                float t0 = Vector2.Dot(p3 - p1, r) / Vector2.Dot(r, r);
                float t1 = Vector2.Dot(p4 - p1, r) / Vector2.Dot(r, r);

                // Sort t0 and t1 to represent the 'min' and 'max' projection along r
                if (t0 > t1) { float temp = t0; t0 = t1; t1 = temp; }

                // Check for overlap of [0, 1] and [t0, t1]
                if (Mathf.Max(0f, t0) <= Mathf.Min(1f, t1))
                {
                    // They overlap. We can return an arbitrary intersection point
                    // For example, the start of the overlap.
                    float overlapStartT = Mathf.Max(0f, t0);
                    intersectionPoint = p1 + overlapStartT * r;
                    return true;
                }
            }
            return false; // Parallel and not collinear/overlapping
        }

        float t = CrossProduct(p3 - p1, s) / denominator;
        float u = CrossProduct(p3 - p1, r) / denominator;

        // If 0 <= t <= 1 and 0 <= u <= 1, then segments intersect
        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            intersectionPoint = p1 + t * r;
            return true;
        }

        return false; // No intersection within segments
    }

    // Reuse the CrossProduct helper from Method 1
    public static float CrossProduct(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }


    private void Test()
    {
        Vector3 p1 = new Vector3(0, 0, 0);
        Vector3 p2 = new Vector3(1, 1, 0);
        Vector3 p3 = new Vector3(0, 1, 0);
        Vector3 p4 = new Vector3(1, 0, 0);
        if (TryGetSegmentIntersection(p1, p2, p3, p4, out var s))
        {
            Debug.Log($"{p1} {p2} {p3} {p4} => {true}, {s}");
        }
        else Debug.Log($"{p1} {p2} {p3} {p4} => {false}");
    }
    #endregion
}
