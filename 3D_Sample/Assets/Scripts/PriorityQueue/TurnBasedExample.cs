using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnBasedExample : MonoBehaviour
{
    [System.Serializable]
    public class Unit
    {
        public string name;
        public int speed; 
        public float cooldown; 
        public float nextReadyTime;
        public int actedCount; 
    }

    [Header("세팅")]
    [Tooltip("쿨타임 계산용. 클수록 전체 진행이 느려짐")]
    public float baseInterval = 10f;

    [Tooltip("전사(5), 마법사(7), 궁수(10), 도적(12) 순으로 채워두면 편함")]
    public List<Unit> units = new List<Unit>();

    private float currentTime = 0f; 
    private int turnNumber = 0; 

    void Reset()
    {
        units = new List<Unit>
        {
            new Unit{ name="전사", speed=5 },
            new Unit{ name="마법사", speed=7 },
            new Unit{ name="궁수", speed=10 },
            new Unit{ name="도적", speed=12 },
        };
    }

    void Start()
    {
        foreach (var u in units)
        {
            u.cooldown = baseInterval / Mathf.Max(1, u.speed); 
            u.nextReadyTime = 0f; 
            u.actedCount = 0;
        }

        Log("스페이스를 누르면 턴이 진행됩니다.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DoOneTurn();
        }
    }

    void DoOneTurn()
    {
        turnNumber++;

        Unit actor = units.FirstOrDefault(u => u.actedCount == 0);
        if (actor == null)
        {
            actor = units.OrderBy(u => u.nextReadyTime).First();
            
            currentTime = actor.nextReadyTime;
        }

        
        Debug.Log($"[{TimeStamp()}] {turnNumber}턴 / {actor.name}의 턴입니다.");

        
        actor.actedCount++;
        actor.nextReadyTime = currentTime + actor.cooldown;
    }

    string TimeStamp()
    {
        int sec = Mathf.FloorToInt(currentTime);
        return $"{sec / 60:D2}:{sec % 60:D2}";
    }

    void Log(string msg) => Debug.Log(msg);
}
