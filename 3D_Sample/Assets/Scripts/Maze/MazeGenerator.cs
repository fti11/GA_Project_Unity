using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 21;    // 홀수 권장
    public int height = 21;
    public float cellSize = 1f;
    public int maxAttempts = 200;

    int[,] map;                        // 1=wall, 0=path
    List<Vector2Int> lastPath;
    GameObject mazeRoot;
    List<GameObject> pathMarks = new List<GameObject>();
    bool showPath = false;

    System.Random rnd = new System.Random();

    Vector2Int StartPos => new Vector2Int(1, 1);
    Vector2Int GoalPos => new Vector2Int(width - 2, height - 2);

    void Start() => Regenerate();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Regenerate();
        if (Input.GetKeyDown(KeyCode.R)) { showPath = !showPath; DrawPath(); }
    }

    void Regenerate()
    {
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        int tries = 0;
        do
        {
            tries++;
            GenerateRecursiveMaze();
            lastPath = FindPath(StartPos, GoalPos);
        } while (lastPath == null && tries < maxAttempts);

        Clear();
        Render();
        showPath = false;
        Debug.Log(lastPath != null ? $"Generated (tries:{tries}) path:{lastPath.Count}" : "Failed: no solvable map");
    }

    void GenerateRecursiveMaze()
    {
        map = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = 1;

        map[StartPos.x, StartPos.y] = 0;
        Carve(StartPos.x, StartPos.y);

        // 보장
        map[StartPos.x, StartPos.y] = 0;
        map[GoalPos.x, GoalPos.y] = 0;
    }

    void Carve(int cx, int cy)
    {
        // directions are 2-step to move between cells; we will carve the between cell too
        var dirs = new Vector2Int[] {
            new Vector2Int(2, 0),
            new Vector2Int(-2, 0),
            new Vector2Int(0, 2),
            new Vector2Int(0, -2)
        };

        // shuffle
        for (int i = 0; i < dirs.Length; i++)
        {
            int j = rnd.Next(i, dirs.Length);
            var tmp = dirs[i]; dirs[i] = dirs[j]; dirs[j] = tmp;
        }

        foreach (var d in dirs)
        {
            int nx = cx + d.x;
            int ny = cy + d.y;

            // bounds: keep outer border as walls
            if (nx <= 0 || nx >= width - 1 || ny <= 0 || ny >= height - 1) continue;
            if (map[nx, ny] == 1)
            {
                // carve between and the next cell
                map[cx + d.x / 2, cy + d.y / 2] = 0;
                map[nx, ny] = 0;
                Carve(nx, ny); // recursion
            }
        }
    }

    List<Vector2Int> FindPath(Vector2Int s, Vector2Int g)
    {
        if (map == null || map[s.x, s.y] == 1 || map[g.x, g.y] == 1) return null;

        var q = new Queue<Vector2Int>();
        bool[,] vis = new bool[width, height];
        Vector2Int[,] par = new Vector2Int[width, height];
        Vector2Int[] dirs = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        q.Enqueue(s); vis[s.x, s.y] = true;
        bool found = false;

        while (q.Count > 0)
        {
            var c = q.Dequeue();
            if (c == g) { found = true; break; }
            foreach (var d in dirs)
            {
                var n = new Vector2Int(c.x + d.x, c.y + d.y);
                if (n.x < 0 || n.y < 0 || n.x >= width || n.y >= height) continue;
                if (vis[n.x, n.y] || map[n.x, n.y] == 1) continue;
                vis[n.x, n.y] = true;
                par[n.x, n.y] = c;
                q.Enqueue(n);
            }
        }

        if (!found) return null;
        var path = new List<Vector2Int>();
        var cur = g;
        while (cur != s) { path.Add(cur); cur = par[cur.x, cur.y]; }
        path.Add(s);
        path.Reverse();
        return path;
    }

    void Render()
    {
        mazeRoot = new GameObject("Maze");
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.transform.parent = mazeRoot.transform;
        floor.transform.localScale = new Vector3((width * cellSize) / 10f, 1, (height * cellSize) / 10f);
        floor.transform.position = new Vector3((width - 1) * cellSize / 2f, -0.02f, (height - 1) * cellSize / 2f);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (map[x, y] == 1)
                {
                    var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    w.transform.parent = mazeRoot.transform;
                    w.transform.position = new Vector3(x * cellSize, 0.5f, y * cellSize);
                    w.transform.localScale = new Vector3(cellSize, 1f, cellSize);
                    w.GetComponent<Renderer>().material.color = Color.black;
                }

        var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        s.transform.parent = mazeRoot.transform;
        s.transform.position = new Vector3(StartPos.x * cellSize, 0.4f, StartPos.y * cellSize);
        s.transform.localScale = Vector3.one * cellSize * 0.6f;
        s.GetComponent<Renderer>().material.color = Color.green;

        var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.transform.parent = mazeRoot.transform;
        g.transform.position = new Vector3(GoalPos.x * cellSize, 0.4f, GoalPos.y * cellSize);
        g.transform.localScale = Vector3.one * cellSize * 0.6f;
        g.GetComponent<Renderer>().material.color = Color.red;
    }

    void DrawPath()
    {
        foreach (var o in pathMarks) if (o) Destroy(o);
        pathMarks.Clear();
        if (!showPath || lastPath == null) return;

        foreach (var v in lastPath)
        {
            if (v == StartPos || v == GoalPos) continue;
            var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
            p.transform.parent = mazeRoot.transform;
            p.transform.position = new Vector3(v.x * cellSize, 0.05f, v.y * cellSize);
            p.transform.localScale = new Vector3(cellSize * 0.6f, 0.05f, cellSize * 0.6f);
            p.GetComponent<Renderer>().material.color = Color.yellow;
            pathMarks.Add(p);
        }
    }

    void Clear()
    {
        if (mazeRoot) DestroyImmediate(mazeRoot);
        foreach (var o in pathMarks) if (o) DestroyImmediate(o);
        pathMarks.Clear();
    }
}
