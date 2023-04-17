using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.TMPro;

public class Citizen : MonoBehaviour
{
    [Header("Tasks")]
    public List<CitizenTask> tasks = new();

    [Header("Traits")]
    public string unitName;
    readonly int nameLengthMin = 3;
    readonly int nameLengthMax = 7;
    public float heightMultiplier = 1;

    public enum CitizenStatus
    {
        Vagabond, Tradesman, Hunter, Warrior, Priest, Chieftain
    }
    public enum Job
    {
        Idle, Farm, Hunt, Build, GatherWood, GatherWild, MineGold, MineStone, Trade
    }
    public enum Race
    {
        Orc, HalfOrc, Human, Minotaur, Centaur, Leonitaur, Kobold
    }

    public CitizenStatus currentStatus;
    public Job currentJob;
    public Race race;
    public bool isWorking;
    public int currentHealth = 16;
    public int maxHealth = 16;
    public int moveSpeed;
    public int buildSpeed;
    public int attackPoints;
    public int defensePoints;
    public int sightRange;

    public enum HungerLevel
    {
        Full, Fine, Peckish, Hungry, Starving
    }
    public enum LibidoLevel
    {
        Glowing, Satisfied, Interested, Aroused, Horny
    }
    public enum SleepLevel
    {
        WellRested, Awake, Drowsy, Tired, Exhausted
    }
    [Header("Needs")]
    public HungerLevel hungerLevel;
    public LibidoLevel libidoLevel;
    public SleepLevel sleepLevel;
    [Range(0,1)]
    public float hunger;
    [Range(0, 1)]
    public float libido;
    [Range(0, 1)]
    public float sleep;
    [Range(0, 1)]
    public float social;
    [Tooltip("Seconds it takes to fill the bar from 0")]
    [SerializeField] private float hungerDecay;
    [Tooltip("Seconds it takes to fill the bar from 0")]
    [SerializeField] private float libidoDecay;
    [Tooltip("Seconds it takes to fill the bar from 0")]
    [SerializeField] private float sleepDecay;
    [Tooltip("Seconds it takes to fill the bar from 0")]
    [SerializeField] private float socialDecay;


    [Header("Internal")]
    private Player player;
    [SerializeField] private bool isInitialized;
    [SerializeField] private TMPro.TextMeshPro nametag;
    [SerializeField] private Transform nametagParent;
    [SerializeField] private GameObject mainCamera;

    void Start()
    {
        if (isInitialized)
        {
            return;
        }

        unitName = Generate.Name(Random.Range(nameLengthMin, nameLengthMax + 1)) + " " + Generate.Name(Random.Range(nameLengthMin, nameLengthMax + 1));
        nametag.text = unitName;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        nametagParent = nametag.transform.parent;
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        isInitialized = true;

        transform.localScale = heightMultiplier * Vector3.one;
    }

    void Update()
    {

        if (isWorking)
        {
            switch (currentJob)
            {
                case Job.Trade:

                    break;

                case Job.Build:

                    break;

                default:
                    Debug.LogFormat("Job {0} not found.", currentJob.ToString());
                    break;
            }

        }
        else
        {

        }
    }

    private void FixedUpdate()
    {
        //Update Needs
        hunger += 1 / hungerDecay / 60;
        libido += 1 / libidoDecay / 60;
        sleep += 1 / sleepDecay / 60;
        social += 1 / socialDecay / 60;

        if (hunger > 1)
            hunger = 1;

        if (hunger > 4 / 5f)
            hungerLevel = HungerLevel.Starving;
        else if (hunger > 3 / 5f)
            hungerLevel = HungerLevel.Hungry;
        else if (hunger > 2 / 5f)
            hungerLevel = HungerLevel.Peckish;
        else if (hunger > 1 / 5f)
            hungerLevel = HungerLevel.Fine;
        else
            hungerLevel = HungerLevel.Full;

        if (libido > 1)
            libido = 1;

        if (libido > 4 / 5f)
            libidoLevel = LibidoLevel.Horny;
        else if (libido > 3 / 5f)
            libidoLevel = LibidoLevel.Aroused;
        else if (libido > 2 / 5f)
            libidoLevel = LibidoLevel.Interested;
        else if (libido > 1 / 5f)  
            libidoLevel = LibidoLevel.Satisfied;
        else
            libidoLevel = LibidoLevel.Glowing;

        if (sleep > 1)
            sleep = 1;

        if (sleep > 4 / 5f)
            sleepLevel = SleepLevel.Exhausted;
        else if (sleep > 3 / 5f)
            sleepLevel = SleepLevel.Tired;
        else if (sleep > 2 / 5f)
            sleepLevel = SleepLevel.Drowsy;
        else if (sleep > 1 / 5f)
            sleepLevel = SleepLevel.Awake;
        else
            sleepLevel = SleepLevel.WellRested;
    }

    private void LateUpdate()
    {
        nametagParent.LookAt(mainCamera.transform.position);
    }
}
