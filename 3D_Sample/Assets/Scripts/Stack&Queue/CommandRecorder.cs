using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EchoMoving : MonoBehaviour
{
    [Header("Player Settings")]
    public float speed = 5f;
    public Material echoMaterial; // �ǵ��� Material (Inspector �Ҵ�)

    [Header("UI Elements")]
    public Button recordButton; // ��ȭ ����/���� ��ư
    public Button playButton;   // ��� ��ư
    public TextMeshProUGUI queueCountText; // ť ���� ǥ��

    private Queue<Vector3> commandQueue = new Queue<Vector3>();
    private Stack<(Vector3 position, float time)> moveHistory = new Stack<(Vector3 position, float time)>();
    private Renderer rend;
    private Material originalMaterial; // ���� Material (�ڵ� ����)
    private bool isRecording = false;
    private bool isExecuting = false;
    private bool isRewinding = false;
    private float rewindTimeLimit = 2.5f; // �ǵ��� �ð� ���� (2.5��)

    void Start()
    {
        // Renderer �ʱ�ȭ
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("Renderer�� �����ϴ�! MeshRenderer/SpriteRenderer�� �߰��ϼ���.");
            rend = GetComponentInChildren<Renderer>();
        }
        if (rend != null)
        {
            originalMaterial = rend.material;
            Debug.Log($"Renderer �ʱ�ȭ. Original Material: {originalMaterial?.name}");
        }

        // echoMaterial Ȯ��
        if (echoMaterial == null)
        {
            Debug.LogWarning("echoMaterial�� �Ҵ���� �ʾҽ��ϴ�! ���� ������ �۵����� ���� �� �ֽ��ϴ�.");
        }

        // UI �ʱ�ȭ
        if (recordButton == null) recordButton = GameObject.Find("RecordButton")?.GetComponent<Button>();
        if (playButton == null) playButton = GameObject.Find("PlayButton")?.GetComponent<Button>();
        if (queueCountText == null) queueCountText = GameObject.Find("QueueCountText")?.GetComponent<TextMeshProUGUI>();

        if (recordButton == null || playButton == null || queueCountText == null)
        {
            Debug.LogWarning("UI ���(recordButton, playButton, queueCountText) �� �ϳ��� �����ϴ�!");
        }

        // ��ư ������
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
        // ť ���� UI ������Ʈ
        if (queueCountText != null)
        {
            queueCountText.text = $"Queue Count: {commandQueue.Count}";
        }

        // �Է� �� ��ġ ��� (�ǵ��� ���� �ƴ� ��)
        if (!isRewinding && !isExecuting)
        {
            HandleInput();
        }

        // ���� �� ��ġ ���
        if (isExecuting && !isRewinding)
        {
            RecordPosition();
        }

        UpdateButtonStates();
    }

    // ��ȭ ���
    void ToggleRecording()
    {
        isRecording = !isRecording;
        if (isRecording)
        {
            commandQueue.Clear();
            moveHistory.Clear();
            Debug.Log("��ȭ ����");
        }
        else
        {
            Debug.Log($"��ȭ ����. ť ����: {commandQueue.Count}");
        }
    }

    // ��� ����
    void StartPlayback()
    {
        if (commandQueue.Count > 0)
        {
            StartCoroutine(ExecuteCommands());
        }
        else
        {
            Debug.Log("ť�� ��� �־� ��� �Ұ�");
        }
    }

    // �Է� ó��
    void HandleInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (x != 0 || y != 0)
        {
            // �̵�
            Vector3 move = new Vector3(x, y, 0).normalized * speed * Time.deltaTime;
            transform.position += move;

            // ��ġ ���
            RecordPosition();

            // ��ȭ ���̸� ť�� ����
            if (isRecording)
            {
                commandQueue.Enqueue(transform.position);
                Debug.Log($"ť�� �߰�: {transform.position}");
            }

            // R �Է� (��ȭ �߿��� ť�� ����)
            if (Input.GetKeyDown(KeyCode.R) && isRecording)
            {
                commandQueue.Enqueue(Vector3.zero); // R�� Ư���� ��Ŀ�� Vector3.zero ���
                Debug.Log("R ��� ť�� �߰�");
            }
        }
    }

    // ��ġ ���
    void RecordPosition()
    {
        moveHistory.Push((transform.position, Time.time));
        PruneOldPositions();
    }

    // ������ ��ġ ���� (2.5�� �ʰ�)
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
        Debug.Log($"Prune �� moveHistory ũ��: {moveHistory.Count}");
    }

    // ��ư ���� ������Ʈ
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

    // ��� ����
    IEnumerator ExecuteCommands()
    {
        if (isExecuting || isRewinding) yield break;

        isExecuting = true;
        Debug.Log($"��� ���� ����. ť ����: {commandQueue.Count}");

        while (commandQueue.Count > 0)
        {
            Vector3 target = commandQueue.Dequeue();

            // R ��� Ȯ�� (Vector3.zero�� R ��Ŀ)
            if (target == Vector3.zero)
            {
                if (moveHistory.Count > 0)
                {
                    Debug.Log("R ���: Rewind ����");
                    yield return StartCoroutine(Rewind());
                }
                else
                {
                    Debug.Log("R ���: moveHistory ��� ����, ��ŵ");
                }
                continue;
            }

            // ��ġ�� �̵� (�ε巴��)
            float elapsed = 0f;
            float duration = 0.2f;
            Vector3 startPos = transform.position;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, target, elapsed / duration);
                yield return null;
            }
            transform.position = target; // ��Ȯ�� ��ǥ ��ġ��
            Debug.Log($"�̵� �Ϸ�: {target}");
        }

        Debug.Log("��� ���� ����");
        isExecuting = false;
    }

    // �ǵ�����
    IEnumerator Rewind()
    {
        if (isRewinding) yield break;

        isRewinding = true;
        Debug.Log("�ǵ����� ����");

        // Material ����
        if (rend != null && echoMaterial != null)
        {
            Debug.Log("Material ����: echoMaterial�� ��ȯ");
            rend.material = echoMaterial;
        }
        else
        {
            Debug.LogWarning($"Material ���� ����: rend={(rend != null ? "����" : "null")}, echoMaterial={(echoMaterial != null ? "����" : "null")}");
        }

        while (moveHistory.Count > 0)
        {
            var entry = moveHistory.Pop();
            transform.position = entry.position;
            Debug.Log($"�ǵ��� ��ġ: {entry.position}");
            yield return new WaitForSeconds(0.05f);
        }

        // Material ����
        if (rend != null && originalMaterial != null)
        {
            Debug.Log("Material ����: originalMaterial�� ��ȯ");
            rend.material = originalMaterial;
        }
        else
        {
            Debug.LogWarning($"Material ���� ����: rend={(rend != null ? "����" : "null")}, originalMaterial={(originalMaterial != null ? "����" : "null")}");
        }

        Debug.Log("�ǵ����� ����");
        isRewinding = false;
    }
}