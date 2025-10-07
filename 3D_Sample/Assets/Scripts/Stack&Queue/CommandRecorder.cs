using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EchoMoving : MonoBehaviour
{
    [Header("Player Settings")]
    public float speed = 5f;
    public Material echoMaterial; // 되돌림 Material (Inspector 할당)

    [Header("UI Elements")]
    public Button recordButton; // 녹화 시작/중지 버튼
    public Button playButton;   // 재생 버튼
    public TextMeshProUGUI queueCountText; // 큐 개수 표시

    private Queue<Vector3> commandQueue = new Queue<Vector3>();
    private Stack<(Vector3 position, float time)> moveHistory = new Stack<(Vector3 position, float time)>();
    private Renderer rend;
    private Material originalMaterial; // 원래 Material (자동 저장)
    private bool isRecording = false;
    private bool isExecuting = false;
    private bool isRewinding = false;
    private float rewindTimeLimit = 2.5f; // 되돌릴 시간 제한 (2.5초)

    void Start()
    {
        // Renderer 초기화
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("Renderer가 없습니다! MeshRenderer/SpriteRenderer를 추가하세요.");
            rend = GetComponentInChildren<Renderer>();
        }
        if (rend != null)
        {
            originalMaterial = rend.material;
            Debug.Log($"Renderer 초기화. Original Material: {originalMaterial?.name}");
        }

        // echoMaterial 확인
        if (echoMaterial == null)
        {
            Debug.LogWarning("echoMaterial이 할당되지 않았습니다! 색상 변경이 작동하지 않을 수 있습니다.");
        }

        // UI 초기화
        if (recordButton == null) recordButton = GameObject.Find("RecordButton")?.GetComponent<Button>();
        if (playButton == null) playButton = GameObject.Find("PlayButton")?.GetComponent<Button>();
        if (queueCountText == null) queueCountText = GameObject.Find("QueueCountText")?.GetComponent<TextMeshProUGUI>();

        if (recordButton == null || playButton == null || queueCountText == null)
        {
            Debug.LogWarning("UI 요소(recordButton, playButton, queueCountText) 중 하나가 없습니다!");
        }

        // 버튼 리스너
        if (recordButton != null)
        {
            recordButton.onClick.AddListener(ToggleRecording);
        }
        if (playButton != null)
        {
            playButton.onClick.AddListener(() => { if (!isExecuting && !isRewinding) StartPlayback(); });
        }

        UpdateButtonStates();
    }

    void Update()
    {
        // 큐 개수 UI 업데이트
        if (queueCountText != null)
        {
            queueCountText.text = $"Queue Count: {commandQueue.Count}";
        }

        // 입력 및 위치 기록 (되돌림 중이 아닐 때)
        if (!isRewinding && !isExecuting)
        {
            HandleInput();
        }

        // 실행 중 위치 기록
        if (isExecuting && !isRewinding)
        {
            RecordPosition();
        }

        UpdateButtonStates();
    }

    // 녹화 토글
    void ToggleRecording()
    {
        isRecording = !isRecording;
        if (isRecording)
        {
            commandQueue.Clear();
            moveHistory.Clear();
            Debug.Log("녹화 시작");
        }
        else
        {
            Debug.Log($"녹화 중지. 큐 개수: {commandQueue.Count}");
        }
    }

    // 재생 시작
    void StartPlayback()
    {
        if (commandQueue.Count > 0)
        {
            StartCoroutine(ExecuteCommands());
        }
        else
        {
            Debug.Log("큐가 비어 있어 재생 불가");
        }
    }

    // 입력 처리
    void HandleInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (x != 0 || y != 0)
        {
            // 이동
            Vector3 move = new Vector3(x, y, 0).normalized * speed * Time.deltaTime;
            transform.position += move;

            // 위치 기록
            RecordPosition();

            // 녹화 중이면 큐에 저장
            if (isRecording)
            {
                commandQueue.Enqueue(transform.position);
                Debug.Log($"큐에 추가: {transform.position}");
            }

            // R 입력 (녹화 중에만 큐에 저장)
            if (Input.GetKeyDown(KeyCode.R) && isRecording)
            {
                commandQueue.Enqueue(Vector3.zero); // R은 특별한 마커로 Vector3.zero 사용
                Debug.Log("R 명령 큐에 추가");
            }
        }
    }

    // 위치 기록
    void RecordPosition()
    {
        moveHistory.Push((transform.position, Time.time));
        PruneOldPositions();
    }

    // 오래된 위치 제거 (2.5초 초과)
    private void PruneOldPositions()
    {
        if (moveHistory.Count == 0) return;

        float currentTime = Time.time;
        Stack<(Vector3, float)> tempStack = new Stack<(Vector3, float)>();

        while (moveHistory.Count > 0)
        {
            var entry = moveHistory.Pop();
            if (currentTime - entry.time <= rewindTimeLimit)
            {
                tempStack.Push(entry);
            }
        }

        while (tempStack.Count > 0)
        {
            moveHistory.Push(tempStack.Pop());
        }
        Debug.Log($"Prune 후 moveHistory 크기: {moveHistory.Count}");
    }

    // 버튼 상태 업데이트
    private void UpdateButtonStates()
    {
        if (recordButton != null)
        {
            TextMeshProUGUI buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isRecording ? "Stop Record" : "Start Record";
            }
            recordButton.interactable = !isExecuting && !isRewinding;
        }

        if (playButton != null)
        {
            playButton.interactable = !isRecording && !isExecuting && !isRewinding && commandQueue.Count > 0;
        }
    }

    // 명령 실행
    IEnumerator ExecuteCommands()
    {
        if (isExecuting || isRewinding) yield break;

        isExecuting = true;
        Debug.Log($"명령 실행 시작. 큐 개수: {commandQueue.Count}");

        while (commandQueue.Count > 0)
        {
            Vector3 target = commandQueue.Dequeue();

            // R 명령 확인 (Vector3.zero는 R 마커)
            if (target == Vector3.zero)
            {
                if (moveHistory.Count > 0)
                {
                    Debug.Log("R 명령: Rewind 시작");
                    yield return StartCoroutine(Rewind());
                }
                else
                {
                    Debug.Log("R 명령: moveHistory 비어 있음, 스킵");
                }
                continue;
            }

            // 위치로 이동 (부드럽게)
            float elapsed = 0f;
            float duration = 0.2f;
            Vector3 startPos = transform.position;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, target, elapsed / duration);
                yield return null;
            }
            transform.position = target; // 정확히 목표 위치로
            Debug.Log($"이동 완료: {target}");
        }

        Debug.Log("명령 실행 종료");
        isExecuting = false;
    }

    // 되돌리기
    IEnumerator Rewind()
    {
        if (isRewinding) yield break;

        isRewinding = true;
        Debug.Log("되돌리기 시작");

        // Material 변경
        if (rend != null && echoMaterial != null)
        {
            Debug.Log("Material 변경: echoMaterial로 전환");
            rend.material = echoMaterial;
        }
        else
        {
            Debug.LogWarning($"Material 변경 실패: rend={(rend != null ? "있음" : "null")}, echoMaterial={(echoMaterial != null ? "있음" : "null")}");
        }

        while (moveHistory.Count > 0)
        {
            var entry = moveHistory.Pop();
            transform.position = entry.position;
            Debug.Log($"되돌림 위치: {entry.position}");
            yield return new WaitForSeconds(0.05f);
        }

        // Material 복구
        if (rend != null && originalMaterial != null)
        {
            Debug.Log("Material 복구: originalMaterial로 전환");
            rend.material = originalMaterial;
        }
        else
        {
            Debug.LogWarning($"Material 복구 실패: rend={(rend != null ? "있음" : "null")}, originalMaterial={(originalMaterial != null ? "있음" : "null")}");
        }

        Debug.Log("되돌리기 종료");
        isRewinding = false;
    }
}