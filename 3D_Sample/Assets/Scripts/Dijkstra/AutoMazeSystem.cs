using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMazeSystem : MonoBehaviour
{
    public int width = 21;
    public int height = 21;
    public float cellSize = 1f;
    public Transform player;

    int[,] map;                    // 0=벽, 1=평지, 2=숲, 3=진흙
    bool[,] visitedDFS;
    Vector2Int start = new Vector2Int(1, 1);
    Vector2Int goal;

    GameObject mazeRoot;
    List<GameObject> pathMarks = new List<GameObject>();

    System.Random rnd = new System.Random();

    void Start()
    {
        GenerateUntilSolvable();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            ShowDijkstraPath();     // 길 안내

        if (Input.GetKeyDown(KeyCode.Space))
            GenerateUntilSolvable(); // 새 미로

        if (Input.GetKeyDown(KeyCode.M))
            AutoMove();
    }

    void GenerateUntilSolvable()
    {
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        goal = new Vector2Int(width - 2, height - 2);

        bool ok = false;

        while (!ok)
        {
            GenerateRandomMaze();
            visitedDFS = new bool[width, height];
            ok = DFS(start.x, start.y);
        }

        Debug.Log("미로 생성 완료(탈출 가능)");

        ClearMaze();
        VisualizeMaze();
        ClearPathMarks();
    }

    void GenerateRandomMaze()
    {
        // 0=벽, 1=길 형태로 먼저 생성
        int[,] maze01 = new int[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze01[x, y] = 0;

        maze01[start.x, start.y] = 1;
        Carve(maze01, start.x, start.y);

        // 1(길) 부분을 코스트 있는 타일로 변환
        map = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (maze01[x, y] == 0)
                {
                    map[x, y] = 0; // 벽
                }
                else
                {
                    int r = rnd.Next(0, 100);

                    if (r < 60) map[x, y] = 1; // 땅(1)
                    else if (r < 85) map[x, y] = 2; // 숲(3)
                    else map[x, y] = 3; // 진흙(5)
                }
            }
        }

        map[start.x, start.y] = 1;
        map[goal.x, goal.y] = 1;
    }

    void Carve(int[,] maze, int x, int y)
    {
        Vector2Int[] dirs =
        {
            new Vector2Int( 2, 0),
            new Vector2Int(-2, 0),
            new Vector2Int( 0, 2),
            new Vector2Int( 0,-2),
        };

        // 랜덤 섞기
        for (int i = 0; i < dirs.Length; i++)
        {
            int j = rnd.Next(i, dirs.Length);
            var temp = dirs[i]; dirs[i] = dirs[j]; dirs[j] = temp;
        }

        foreach (var d in dirs)
        {
            int nx = x + d.x;
            int ny = y + d.y;

            if (nx <= 0 || ny <= 0 || nx >= width - 1 || ny >= height - 1)
                continue;

            if (maze[nx, ny] == 0)
            {
                maze[x + d.x / 2, y + d.y / 2] = 1;
                maze[nx, ny] = 1;
                Carve(maze, nx, ny);
            }
        }
    }

    bool DFS(int x, int y)
    {
        if (!InBounds(x, y)) return false;
        if (map[x, y] == 0) return false; 
        if (visitedDFS[x, y]) return false;

        visitedDFS[x, y] = true;

        if (x == goal.x && y == goal.y)
            return true;

        Vector2Int[] dirs =
        {
            new Vector2Int(1,0),
            new Vector2Int(-1,0),
            new Vector2Int(0,1),
            new Vector2Int(0,-1),
        };

        foreach (var d in dirs)
        {
            if (DFS(x + d.x, y + d.y))
                return true;
        }

        return false;
    }

    void VisualizeMaze()
    {
        mazeRoot = new GameObject("Maze");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int tile = map[x, y];

                GameObject t = GameObject.CreatePrimitive(PrimitiveType.Cube);
                t.transform.parent = mazeRoot.transform;
                t.transform.position = new Vector3(x * cellSize, 0, y * cellSize);

                if (tile == 0)
                {
                    // 벽(검정)
                    t.transform.localScale = new Vector3(cellSize, 1, cellSize);
                    t.GetComponent<Renderer>().material.color = new Color(0.2f, 0.1f, 0.1f);
                }
                else
                {
                    // 바닥 타일
                    t.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);

                    if (tile == 1) t.GetComponent<Renderer>().material.color = Color.white;
                    if (tile == 2) t.GetComponent<Renderer>().material.color = new Color(0.6f, 1f, 0.6f);
                    if (tile == 3) t.GetComponent<Renderer>().material.color = new Color(1f, 0.7f, 0.5f);
                }
            }
        }
    }

    void ClearMaze()
    {
        if (mazeRoot != null)
            Destroy(mazeRoot);
    }

    public void ShowDijkstraPath()
    {
        List<Vector2Int> path = Dijkstra(start, goal);
        if (path == null) return;

        ClearPathMarks();

        foreach (var p in path)
        {
            GameObject m = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m.transform.parent = mazeRoot.transform;
            m.transform.position = new Vector3(p.x * cellSize, 0.3f, p.y * cellSize);
            m.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            m.GetComponent<Renderer>().material.color = Color.cyan;
            pathMarks.Add(m);
        }
    }

    List<Vector2Int> Dijkstra(Vector2Int start, Vector2Int goal)
    {
        int w = width;
        int h = height;

        int[,] dist = new int[w, h];
        Vector2Int?[,] parent = new Vector2Int?[w, h];
        bool[,] visited = new bool[w, h];

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                dist[x, y] = int.MaxValue;

        dist[start.x, start.y] = 0;

        SimplePriorityQueue<Vector2Int> pq = new SimplePriorityQueue<Vector2Int>();
        pq.Enqueue(start, 0);

        Vector2Int[] dirs =
        {
            new Vector2Int(1,0),
            new Vector2Int(-1,0),
            new Vector2Int(0,1),
            new Vector2Int(0,-1),
        };

        while (pq.Count > 0)
        {
            Vector2Int cur = pq.Dequeue();
            if (visited[cur.x, cur.y]) continue;
            visited[cur.x, cur.y] = true;

            if (cur == goal)
                return Reconstruct(parent, start, goal);

            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (!InBounds(nx, ny)) continue;
                if (map[nx, ny] == 0) continue;

                int cost = TileCost(map[nx, ny]);
                int nd = dist[cur.x, cur.y] + cost;

                if (nd < dist[nx, ny])
                {
                    dist[nx, ny] = nd;
                    parent[nx, ny] = cur;
                    pq.Enqueue(new Vector2Int(nx, ny), nd);
                }
            }
        }

        return null;
    }

    int TileCost(int t)
    {
        if (t == 1) return 1;
        if (t == 2) return 3;
        if (t == 3) return 5;
        return int.MaxValue; // 벽
    }

    List<Vector2Int> Reconstruct(Vector2Int?[,] parent, Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int? cur = goal;

        while (cur.HasValue)
        {
            path.Add(cur.Value);
            if (cur.Value == start) break;
            cur = parent[cur.Value.x, cur.Value.y];
        }

        path.Reverse();
        return path;
    }

    void ClearPathMarks()
    {
        foreach (var o in pathMarks)
            if (o != null) Destroy(o);
        pathMarks.Clear();
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && x < width &&
               y >= 0 && y < height;
    }

    void AutoMove()
    {
        List<Vector2Int> path = Dijkstra(start, goal);
        if (path == null) return;

        StartCoroutine(MovePlayer(path));
    }

    IEnumerator MovePlayer(List<Vector2Int> path)
    {
        // 플레이어 없으면 자동 생성
        if (player == null)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.GetComponent<Renderer>().material.color = Color.blue;
            player = p.transform;
        }

        player.position = new Vector3(start.x, 0.5f, start.y);

        foreach (var step in path)
        {
            Vector3 target = new Vector3(step.x, 0.5f, step.y);

            while ((player.position - target).sqrMagnitude > 0.001f)
            {
                player.position = Vector3.MoveTowards(
                    player.position,
                    target,
                    3f * Time.deltaTime
                );
                yield return null;
            }
        }
    }
}
