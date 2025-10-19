using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PQueueTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var queue = new SimplePriorityQueue<string>();
        queue.Enqueue("PlayerA", 3);
        queue.Enqueue("PlayerB", 1);
        queue.Enqueue("PlayerC", 2);

        while (queue.Count > 0)
        {
            Debug.Log(queue.Dequeue());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
