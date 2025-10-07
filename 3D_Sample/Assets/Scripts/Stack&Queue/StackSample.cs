using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackSample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Stack<int> stack = new Stack<int>();

        // Push: 데이터 넣기
        stack.Push(10);
        stack.Push(20);
        stack.Push(30);

        Debug.Log("========== Stack 1 ==========");
        foreach (int num in stack)
            Debug.Log(num);
        Debug.Log("=============================");

        // Peek: 맨 위 데이터 확인
        Debug.Log("Peek: " + stack.Pop());  //  30 (제거 안 함)

        // Pop: 맨 위 데이터 꺼내기
        Debug.Log("Pop: " + stack.Pop());   // 30 꺼내고 제거
        Debug.Log("Pop: " + stack.Pop());   // 20 꺼내고 제거 

        Debug.Log("남은 데이터 수: " + stack.Count);  // 1

        Debug.Log("========== Stack 2 ==========");
        foreach (int num in stack)
            Debug.Log(num);
        Debug.Log("=============================");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
