using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileableBase : MonoBehaviour
{
    [SerializeField] Vector2Int chunkPosition;
    [SerializeField] Vector2Int cellPosition;
    [SerializeField] Vector2Int tilePosition;
    [SerializeField] LocationManager location;

    public void SnapToRoot()
    {
        transform.SetParent(GameObject.Find("Map Root").transform, false);
        transform.localPosition = new Vector3(
            chunkPosition.x * 4000 + cellPosition.x * 200 + tilePosition.x * 10,
            location.GetTileY(chunkPosition, cellPosition, tilePosition),
            chunkPosition.y * 4000 + cellPosition.y * 200 + tilePosition.y * 10
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(transform.position + new Vector3(5, -0.5f, 5), new Vector3(10, 1, 10));
    }
}
