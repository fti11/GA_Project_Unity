using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeAutoMove : MonoBehaviour
{
    [Header("Maze Size (홀수 추천)")]
    public int width = 21;
    public int height = 21;
    public float cellSize = 1f;
    public int maxAttempts = 100;  // 탈출 가능한 미로 찾기 최대 시도

    [Header("Player")]
    public Transform player; 

    int[,] map;                    // 1 = 벽, 0 = 길
    Vector2Int start = new Vector2Int(1, 1);
    Vector2Int goal;

    Vector2Int[] dirs =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0,-1),
    };
    bool[,] visitedDFS;

    bool[,] visitedBFS;
    Vector2Int?[,] parent;
    List<Vector2Int> shortestPath;

    // 시각화용
    GameObject mazeRoot;
    List<GameObject> pathMarks = new List<GameObject>();
    Coroutine moveRoutine;

    System.Random rnd = new System.Random();

    void Start()
    {
        GenerateUntilSolvable();
    }

    void Update()
    {
        // 키보드로도 테스트 가능
        if (Input.GetKeyDown(KeyCode.G)) ShowPath();      // 길 안내
        if (Input.GetKeyDown(KeyCode.M)) AutoMove();      // 자동 이동
        if (Input.GetKeyDown(KeyCode.Space)) GenerateUntilSolvable(); // 새 미로
    }

    void GenerateUntilSolvable()
    {
        // 홀수로 보정
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        goal = new Vector2Int(width - 2, height - 2);

        int tries = 0;
        bool ok = false;

        do
        {
            tries++;
            GenerateRandomMaze();             // 랜덤 미로 생성
            visitedDFS = new bool[width, height];
            ok = DFSCheck(start.x, start.y);   // DFS로 탈출 가능한지 확인
        }
        while (!ok && tries < maxAttempts);

        Debug.Log(ok ? $"탈출 가능한 미로 생성 (시도: {tries})"
                     : "실패: 탈출 가능한 미로를 찾지 못함");

        ClearMaze();
        VisualizeMaze();

        // 새 미로 생성 시 플레이어 위치 리셋
        PreparePlayer();

        // 최단 경로는 나중에 길 안내 / 자동 이동 버튼 누를 때 BFS로 계산
        shortestPath = null;
        ClearPathMarks();
    }

    void GenerateRandomMaze()
    {
        map = new int[width, height];

        // 전부 벽으로 초기화
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = 1;

        // 시작 지점에서 재귀로 길 파기
        map[start.x, start.y] = 0;
        Carve(start.x, start.y);

        // 시작/도착 지점은 길로 강제
        map[start.x, start.y] = 0;
        map[goal.x, goal.y] = 0;
    }

    // 재귀 DFS를 이용한 미로 생성 (백트래킹)
    void Carve(int x, int y)
    {
        // 2칸씩 이동 (중간 칸을 파면서 진행)
        Vector2Int[] carveDirs =
        {
            new Vector2Int( 2, 0),
            new Vector2Int(-2, 0),
            new Vector2Int( 0, 2),
            new Vector2Int( 0,-2),
        };

        // 방향 섞기 (랜덤성)
        for (int i = 0; i < carveDirs.Length; i++)
        {
            int j = rnd.Next(i, carveDirs.Length);
            Vector2Int temp = carveDirs[i];
            carveDirs[i] = carveDirs[j];
            carveDirs[j] = temp;
        }

        foreach (var d in carveDirs)
        {
            int nx = x + d.x;
            int ny = y + d.y;

            // 가장자리(외곽)는 벽으로 유지되도록 1~width-2, 1~height-2 범위만 사용
            if (nx <= 0 || ny <= 0 || nx >= width - 1 || ny >= height - 1)
                continue;

            if (map[nx, ny] == 1) // 아직 파지 않은 곳이면
            {
                // 중간 칸 + 다음 칸을 길로 만든다
                map[x + d.x / 2, y + d.y / 2] = 0;
                map[nx, ny] = 0;
                Carve(nx, ny);
            }
        }
    }

    // DFS로 탈출 가능 여부 검사
    bool DFSCheck(int x, int y)
    {
        if (!InBounds(x, y)) return false;
        if (map[x, y] == 1) return false;
        if (visitedDFS[x, y]) return false;

        visitedDFS[x, y] = true;

        if (x == goal.x && y == goal.y)
            return true;

        foreach (var d in dirs)
        {
            if (DFSCheck(x + d.x, y + d.y))
                return true;
        }

        return false;
    }

    void VisualizeMaze()
    {
        mazeRoot = new GameObject("Maze");

        // 바닥 (Plane)
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.transform.parent = mazeRoot.transform;
        floor.transform.localScale = new Vector3((width * cellSize) / 10f, 1, (height * cellSize) / 10f);
        floor.transform.position = new Vector3((width - 1) * cellSize / 2f, -0.01f, (height - 1) * cellSize / 2f);

        // 벽
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1)
                {
                    GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.parent = mazeRoot.transform;
                    wall.transform.position = new Vector3(x * cellSize, 0.5f, y * cellSize);
                    wall.transform.localScale = new Vector3(cellSize, 1f, cellSize);
                    wall.GetComponent<Renderer>().material.color = Color.black;
                }
            }
        }

        // Goal 표시용 구체 (빨간색)
        GameObject goalSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goalSphere.transform.parent = mazeRoot.transform;
        goalSphere.transform.position = GridToWorld(goal);
        goalSphere.transform.localScale = Vector3.one * cellSize * 0.6f;
        goalSphere.GetComponent<Renderer>().material.color = Color.red;
    }

    void ClearMaze()
    {
        if (mazeRoot != null)
            Destroy(mazeRoot);
    }

    void ComputeShortestPath()
    {
        int w = width;
        int h = height;

        visitedBFS = new bool[w, h];
        parent = new Vector2Int?[w, h];
        Queue<Vector2Int> q = new Queue<Vector2Int>();

        q.Enqueue(start);
        visitedBFS[start.x, start.y] = true;

        bool found = false;

        while (q.Count > 0)
        {
            Vector2Int cur = q.Dequeue();

            if (cur == goal)
            {
                found = true;
                break;
            }

            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (!InBounds(nx, ny)) continue;
                if (map[nx, ny] == 1) continue;
                if (visitedBFS[nx, ny]) continue;

                visitedBFS[nx, ny] = true;
                parent[nx, ny] = cur;
                q.Enqueue(new Vector2Int(nx, ny));
            }
        }

        if (!found)
        {
            Debug.Log("BFS: 경로 없음");
            shortestPath = null;
            return;
        }

        // 경로 복원
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int? curPos = goal;

        while (curPos.HasValue)
        {
            path.Add(curPos.Value);
            curPos = parent[curPos.Value.x, curPos.Value.y];
        }

        path.Reverse();
        shortestPath = path;

        Debug.Log($"BFS 최단 경로 길이: {shortestPath.Count}");
    }

    public void ShowPath()
    {
        // UI 버튼 / 키보드(G)에서 호출
        if (shortestPath == null)
            ComputeShortestPath();

        ClearPathMarks();

        if (shortestPath == null) return;

        foreach (var p in shortestPath)
        {
            if (p == start || p == goal) continue; // 시작/끝은 표시 안 해도 됨

            GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mark.transform.parent = mazeRoot.transform;
            mark.transform.position = new Vector3(p.x * cellSize, 0.05f, p.y * cellSize);
            mark.transform.localScale = new Vector3(cellSize * 0.6f, 0.05f, cellSize * 0.6f);
            mark.GetComponent<Renderer>().material.color = Color.yellow;
            pathMarks.Add(mark);
        }
    }

    void ClearPathMarks()
    {
        foreach (var m in pathMarks)
            if (m != null) Destroy(m);
        pathMarks.Clear();
    }

    public void AutoMove()
    {
        // UI 버튼 / 키보드(M)에서 호출
        if (shortestPath == null)
            ComputeShortestPath();

        if (shortestPath == null || player == null) return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveAlongPath());
    }

    IEnumerator MoveAlongPath()
    {
        // path[0]은 start, path[last]는 goal
        for (int i = 0; i < shortestPath.Count; i++)
        {
            Vector3 targetPos = GridToWorld(shortestPath[i]);
            // 부드럽게 이동
            while ((player.position - targetPos).sqrMagnitude > 0.001f)
            {
                player.position = Vector3.MoveTowards(player.position, targetPos, Time.deltaTime * 3f);
                yield return null;
            }
        }
        moveRoutine = null;
    }

    void PreparePlayer()
    {
        if (player == null)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.name = "Player";
            p.GetComponent<Renderer>().material.color = Color.blue;
            player = p.transform;
        }

        player.position = GridToWorld(start);
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    Vector3 GridToWorld(Vector2Int g)
    {
        return new Vector3(g.x * cellSize, 0.5f, g.y * cellSize);
    }
}
