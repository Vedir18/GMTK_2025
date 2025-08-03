using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class MapManager : MonoBehaviour
{
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private float chunkSize;
    [SerializeField] private int generationRange;
    [SerializeField] private int pointsMin, pointsMax;
    private Dictionary<Vector2Int, Chunk> generatedChunks;

    private void Start()
    {
        generatedChunks = new Dictionary<Vector2Int, Chunk>();
    }

    public void ProcessPlayerPos(Vector3 position)
    {
        Vector2Int playerChunk = PosToChunk(position);
        (Vector2, Vector2) bound = BLTRChunkCorners(playerChunk);
        Debug.DrawLine(bound.Item1, bound.Item2, Color.magenta, 120);
        for (int i = playerChunk.x - 2; i <= playerChunk.x + 2; i++)
        {
            for(int j = playerChunk.y - 2; j <= playerChunk.y + 2; j++)
            {
                if (generatedChunks.ContainsKey(new Vector2Int(i, j))) return;
                GenerateChunk(new Vector2Int(i, j));
                bound = BLTRChunkCorners(new Vector2Int(i,j));

                Debug.DrawLine(bound.Item1, bound.Item2, Color.yellow, 120);
            }
        }
    }

    private void GenerateChunk(Vector2Int pos)
    {
        int spawnedPoints = Random.Range(pointsMin, pointsMax);
        List<Point> spawned = new List<Point>();
        for (int i = 0; i < spawnedPoints; i++)
        {
            spawned.Add(SpawnPoint(pos));
        }
        generatedChunks.Add(pos, new Chunk(spawned.ToArray()));
    }

    private Point SpawnPoint(Vector2Int chunkPos)
    {
        (Vector2, Vector2) bounds = BLTRChunkCorners(chunkPos);
        float x = Random.Range(bounds.Item1.x, bounds.Item2.x);
        float y = Random.Range(bounds.Item1.y, bounds.Item2.y);
        Point newPoint = Instantiate(pointPrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<Point>();
        return newPoint;
    }

    private (Vector2, Vector2) BLTRChunkCorners(Vector2Int chunkPos)
    {
        float l = chunkPos.x * chunkSize;
        float r = (chunkPos.x + 1) * chunkSize;
        float b = chunkPos.y * chunkSize;
        float t = (chunkPos.y + 1) * chunkSize;
        return(new Vector2(l,b), new Vector2(r,t));
    }

    private Vector2Int PosToChunk(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / chunkSize), Mathf.FloorToInt(pos.y / chunkSize));
    }
}

public struct Chunk
{
    public Chunk(Point[] points)
    {
        this.points = points;
    }
    Point[] points;
}
