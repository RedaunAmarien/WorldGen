using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileableBase : MonoBehaviour
{
    [SerializeField] Vector2Int chunkIndex;
    [SerializeField] Vector2Int cellIndex;
    [SerializeField] Vector2Int tileIndex;
    GameplayManager manager;

    private void Start()
    {
        manager = GameObject.Find("GameplayManager").GetComponent<GameplayManager>();
        SnapToRoot();
    }

    public void SnapToRoot()
    {
        transform.SetParent(GameObject.Find("Map Root").transform, false);
        transform.localPosition = manager.GetComponent<Grid>().CellToLocal((Vector3Int)tileIndex);
        //transform.localPosition = new Vector3(
        //    chunkIndex.x * manager.chunkEdgeLength + cellIndex.x * manager.cellEdgeLength + tileIndex.x * manager.tileEdgeLength,
        //    manager.GetTileY(chunkIndex, cellIndex, tileIndex),
        //    chunkIndex.y * manager.chunkEdgeLength + cellIndex.y * manager.cellEdgeLength + tileIndex.y * manager.tileEdgeLength
        //);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(transform.position + new Vector3(5, 1.5f, 5), new Vector3(10, 3, 10));
    }
}
