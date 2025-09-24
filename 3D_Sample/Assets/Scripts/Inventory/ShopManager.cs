using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputSearch;        // 검색창
    public Transform contentParent;           // ScrollView → Viewport → Content
    public GameObject itemSlotPrefab;         // 아이템 슬롯 프리팹

    private List<ShopItem> items = new List<ShopItem>();
    private List<GameObject> spawnedSlots = new List<GameObject>();

    private void Start()
    {
        InitItems();
        ShowAllItems();
    }

    private void InitItems()
    {
        items.Clear();
        for (int i = 0; i < 100; i++)
        {
            items.Add(new ShopItem($"Item_{i:D2}"));
        }
    }

    // 모든 아이템 표시
    private void ShowAllItems()
    {
        ClearSlots();
        foreach (var item in items)
        {
            CreateSlot(item.itemName);
        }
    }

    // 슬롯 생성
    private void CreateSlot(string itemName)
    {
        GameObject slot = Instantiate(itemSlotPrefab, contentParent);
        slot.GetComponentInChildren<TMP_Text>().text = itemName;
        spawnedSlots.Add(slot);
    }

    private void ClearSlots()
    {
        foreach (var slot in spawnedSlots)
        {
            Destroy(slot);
        }
        spawnedSlots.Clear();
    }

    // 선형 탐색
    public void OnLinearSearch()
    {
        string target = inputSearch.text.Trim();
        if (string.IsNullOrEmpty(target)) return;

        ClearSlots();

        int steps = 0;
        foreach (var item in items)
        {
            steps++;
            if (item.itemName == target)
            {
                CreateSlot(item.itemName);
                Debug.Log($"[Linear] {target} 찾음! 비교 {steps}회");
                return;
            }
        }

        Debug.Log($"[Linear] {target} 없음. 비교 {steps}회");
    }

    // 이진 탐색
    public void OnBinarySearch()
    {
        string target = inputSearch.text.Trim();
        if (string.IsNullOrEmpty(target)) return;

        items.Sort((a, b) => a.itemName.CompareTo(b.itemName));

        int left = 0, right = items.Count - 1;
        int steps = 0;

        ClearSlots();

        while (left <= right)
        {
            steps++;
            int mid = (left + right) / 2;
            int cmp = items[mid].itemName.CompareTo(target);

            if (cmp == 0)
            {
                CreateSlot(items[mid].itemName);
                Debug.Log($"[Binary] {target} 찾음! 비교 {steps}회");
                return;
            }
            else if (cmp < 0) left = mid + 1;
            else right = mid - 1;
        }

        Debug.Log($"[Binary] {target} 없음. 비교 {steps}회");
    }
}

public class ShopItem
{
    public string itemName;
    public ShopItem(string name) { itemName = name; }
}