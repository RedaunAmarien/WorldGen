using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] int riverLength = 16;

    void OnSelect()
    {
        GameplayManager manager = GetComponent<GameplayManager>();
        manager.AddRiver(manager.Position2Indices(transform.position), riverLength);
    }
}
