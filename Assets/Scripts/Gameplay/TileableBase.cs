using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileableBase : MonoBehaviour
{
    [SerializeField] Vector2Int chunkIndex;
    [SerializeField] Vector2Int cellIndex;
    [SerializeField] Vector2Int tileIndex;
    GameplayManager manager;
    bool isSnapped;

    private void Start()
    {
        manager = GameObject.Find("Player").GetComponent<GameplayManager>();
    }

    private void Update()
    {
        if (manager.fullyInitialized && !isSnapped)
        {
            SnapToRoot();
        }
    }

    public void SnapToRoot()
    {
        transform.SetParent(GameObject.Find("ObjectRoot").transform, false);
        transform.localPosition = manager.Indices2Position(chunkIndex, cellIndex, tileIndex);
        isSnapped = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(transform.position + new Vector3(5, 1.5f, 5), new Vector3(10, 3, 10));
    }
}
