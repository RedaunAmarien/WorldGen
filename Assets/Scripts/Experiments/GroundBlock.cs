using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.AI.Navigation;

public class GroundBlock : MonoBehaviour
{
    [SerializeField] int walkScore;
    [SerializeField] int falloffSpeed;
    [SerializeField] int maxWalkScore;
    [SerializeField] Gradient colors;
    int falloffIndex;
    public float walkability;
    public int gCost;
    public int hCost;
    Renderer rend;

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public float tileScore
    {
        get { return walkability * fCost; }
    }

    private void Start()
    {
        rend = GetComponentInChildren<Renderer>();
        rend.material.color = colors.Evaluate(0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "UnitVillager")
        {
            walkScore++;
            if (walkScore < 0) walkScore = 0;
            if (walkScore > maxWalkScore) walkScore = maxWalkScore;
            walkability = (float)walkScore / maxWalkScore;
            other.gameObject.GetComponent<PathFinder>().SetSpeed(walkability);
            rend.material.color = colors.Evaluate(walkability);
        }
    }

    private void Update()
    {
        falloffIndex++;
        if (falloffIndex >= falloffSpeed)
        {
            falloffIndex = 0;
            walkScore--;
            if (walkScore < 0) walkScore = 0;
            if (walkScore > maxWalkScore) walkScore = maxWalkScore;
            walkability = (float)walkScore / maxWalkScore;
            rend.material.color = colors.Evaluate(walkability);
        }
    }
}
