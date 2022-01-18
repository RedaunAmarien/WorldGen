using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitCitizen : MonoBehaviour
{
    GridManager manager;
    NavMeshAgent navigation;
    public Material[] playerColors;
    [Range(0,7)]
    public int owner;
    public string unitName;
    public enum Citizenship
    {
        Villager, Infantry, Cavalry, Hero, Special
    }
    public enum Jobs
    {
        Idle, Farm, Hunt, Build, GatherWood, GatherWild, MineGold, MineStone, Trade
    }

    public Citizenship myStatus;
    public Jobs myJob;
    public bool working;
    public int health = 10, maxHealth = 10, moveSpeed, buildSpeed, attackM, attackP, armorM, armorP, lOS;
    public UnitBuildingHousing myHome;
    public UnitBuildingBase myBase;
    public GameObject myWork;

    void Start() 
    {
        navigation = gameObject.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (working)
        {
            switch (myJob)
            {
                case Jobs.Trade:
                    navigation.SetDestination(myWork.transform.position);
                break;

                case Jobs.Build:

                break;
                
                default:
                    Debug.Log("Job " + myJob.ToString() + " not found.");
                break;
            }

        }
        else
        {

        }
    }

    public void OnBorn(int owner, string name)
    {
        manager = GameObject.Find("GameManager").GetComponent<GridManager>();
        unitName = name;
        StartCoroutine(ChangeOwner(owner));
    }

    public IEnumerator ChangeOwner (int newPlayer)
    {
        yield return new WaitForSeconds(1);
        gameObject.GetComponentInChildren<Renderer>().material = playerColors[newPlayer];
        gameObject.transform.SetParent(GameObject.Find("Player Units (" + newPlayer + ")").transform);
        myBase = manager.bases[newPlayer].GetComponent<UnitBuildingBase>();
        owner = newPlayer;
        myJob = Jobs.Idle;
        SetHomeless();
    }

    public void SetHomeless ()
    {
        myBase.GetComponent<UnitBuildingBase>().AddHomeless(this);
    }
}
