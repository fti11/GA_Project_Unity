using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // 인벤토리 아이템 리스트 
    public List<Item> items = new List<Item>();

    // Start is called before the first frame update
    void Start()
    {
        // 예시 아이템 추가
        items.Add(new Item("Sward"));
        items.Add(new Item("Shield"));
        items.Add(new Item("Potion"));

        // 아이템 찾기 리스트
        Item found = FindItem("Potion");

        if (found != null)
        {
            Debug.Log("찾은 아이템 : " + found.itemName);
        }
        else
        {
            Debug.Log("아이템을 찾을 수 없습니다.");
        }
    }

    // 선형 탐색
    public Item FindItem(string _itemName)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemName == null)
                return items[i];
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
