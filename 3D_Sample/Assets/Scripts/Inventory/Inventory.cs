using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // �κ��丮 ������ ����Ʈ 
    public List<Item> items = new List<Item>();

    // Start is called before the first frame update
    void Start()
    {
        // ���� ������ �߰�
        items.Add(new Item("Sward"));
        items.Add(new Item("Shield"));
        items.Add(new Item("Potion"));

        // ������ ã�� ����Ʈ
        Item found = FindItem("Potion");

        if (found != null)
        {
            Debug.Log("ã�� ������ : " + found.itemName);
        }
        else
        {
            Debug.Log("�������� ã�� �� �����ϴ�.");
        }
    }

    // ���� Ž��
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
