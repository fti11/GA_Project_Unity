using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCombo : MonoBehaviour
{
    void Start()
    {
        int maxDmg = 0;
        int q = 0, h = 0, m = 0, t = 0; 

        for (int a = 0; a <= 2; a++)
            for (int b = 0; b <= 2; b++)
                for (int c = 0; c <= 1; c++)
                    for (int d = 0; d <= 1; d++)
                    {
                        int cost = a * 2 + b * 3 + c * 5 + d * 7;
                        if (cost > 15) continue;
                        int dmg = a * 6 + b * 8 + c * 16 + d * 24;
                        if (dmg > maxDmg)
                        {
                            maxDmg = dmg;
                            q = a; h = b; m = c; t = d;
                        }
                    }

        Debug.Log($"최대 데미지: {maxDmg} (퀵 {q}, 헤비 {h}, 멀티 {m}, 트리플 {t})");
    }
}
