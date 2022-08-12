using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathFinder : MonoBehaviour
{
    [SerializeField] float minSpeed, maxSpeed, speed, turnSpeed;
    GameObject[] waypoints;
    int targetIndex;
    [SerializeField] float targetRange;
    [SerializeField] Vector3 targetPosition;
    bool avoiding;
    [SerializeField] string avoidingTarget;

    // Start is called before the first frame update
    void Start()
    {
        waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
        targetIndex = Random.Range(0, waypoints.Length);
        targetPosition = waypoints[targetIndex].transform.position;
        avoiding = false;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, targetRange);
        if (hit.collider != null && hit.collider.gameObject != waypoints[targetIndex] && hit.collider.gameObject.tag != "Road")
        {
            avoiding = true;
            avoidingTarget = hit.collider.gameObject.name;
        }
        else
        {
            avoiding = false;
            avoidingTarget = "None";
        }

        if (avoiding)
        {
            transform.Rotate(new Vector3(0, turnSpeed * Time.deltaTime, 0));
            transform.Translate(Vector3.forward * Time.deltaTime * speed/2);
        }
        else
        {
            transform.LookAt(targetPosition, Vector3.up);
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }

        if (Vector3.Distance(waypoints[targetIndex].transform.position, gameObject.transform.position) < targetRange)
        {
            targetIndex = Random.Range(0, waypoints.Length);
            targetPosition = waypoints[targetIndex].transform.position;
        }
        //Debug.DrawRay(transform.position, transform.forward * targetRange, Color.yellow, 0.1f);
        //Debug.DrawLine(transform.position, targetPosition, Color.blue, 0.1f);
    }

    public void SetSpeed(float walkability)
    {
        speed = Mathf.Lerp(minSpeed, maxSpeed, walkability);
    }

    void FindPath()
    {
        List<GroundBlock> openBlocks = new List<GroundBlock>();
        HashSet<GroundBlock> closedBlocks = new HashSet<GroundBlock>();
        openBlocks.Add(new GroundBlock());


    }
}
