using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SquadAI : MonoBehaviour
{

	public int size;
	public GameObject soldier;
	Soldier[] soldiers;
	public int frameInterval = 10;
    public float maxDistanceFromDestination = 3f;
    public float stoppingDistance;
    public float trueStoppingDistance = 2f;
    public float maxWanderDistance = 15f;
    [HideInInspector]
    public Checkpoint[] checkpoints;
    Vector3 average;
    bool started = false;
    Soldier leader;
    Vector3 newDestination;
    [HideInInspector]
    public SpawnOnMouse spawner;
    public bool AI_FindCheckpoints = true;
    public bool AI_ExploreGunshots = true;
    Checkpoint currentCheckpoint;

    [HideInInspector]
    public int orderOfArrival;

    [HideInInspector]
    public bool inCheckpoint;

    public int checkpointCheckInterval = 120;

    public int wanderInterval = 60;

    public int aggression = 2; //0 = retreat, 1 = defend, 2 = attack

    // Start is called before the first frame update
    void Awake()
    {
        AI_FindCheckpoints = (Random.value > 0.5f);
        aggression = Random.Range(0,2);
    	Soldier[] tempArray = new Soldier[size];
    	for (int i = 0; i < size; i++)
    	{
    		GameObject newSoldier = Instantiate(soldier, transform);
    		tempArray[i] = newSoldier.GetComponent<Soldier>();
            if (i == 0) leader = newSoldier.GetComponent<Soldier>();
            newSoldier.GetComponent<Soldier>().squad = this;
            newSoldier.GetComponent<NavMeshAgent>().Warp(transform.position);
    	}
    	soldiers = tempArray;
    }

    public void SetDestination(Vector3 destination)
    {
    	for (int i = 0; i < soldiers.Length; i++)
    	{
            Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * maxDistanceFromDestination;
            soldiers[i].destination = destination + randomOffset;
            soldiers[i].navAgent.isStopped = true;
    	}
    }

    void StartAgents()
    {
        started = true;
        for (int i =  0; i < soldiers.Length; i++)
        {
            soldiers[i].navAgent.isStopped = false;
        }
    }

    void Update()
    {
        if (Time.frameCount%frameInterval != 0) return;

        if (leader.navAgent.remainingDistance != 0f && leader.navAgent.remainingDistance <= stoppingDistance && !leader.attacking && !leader.navAgent.pathPending)
        {
            if (AI_FindCheckpoints)
            {
                if ((!inCheckpoint || (Time.frameCount+orderOfArrival)%checkpointCheckInterval == 0) && aggression != 0) Checkpoint(aggression);
                else if ((Time.frameCount+orderOfArrival)%wanderInterval == 0) Wander();
                else {
                    for (int i = 0; i < soldiers.Length; i++)
                    {
                        if (soldiers[i].navAgent.remainingDistance <= trueStoppingDistance) soldiers[i].navAgent.isStopped = true;
                    }
                }
            } else if ((Time.frameCount+orderOfArrival)%wanderInterval == 0) {
                Wander();
            } else {
                for (int i = 0; i < soldiers.Length; i++)
                {
                    if (soldiers[i].navAgent.remainingDistance <= trueStoppingDistance) soldiers[i].navAgent.isStopped = true;
                }
            }
        }

        if (started) return;
        for (int i = 0; i < soldiers.Length; i++)
        {
            if (soldiers[i].navAgent.pathPending || soldiers[i].target != null)
            {
                return;
            }
        }
        StartAgents();
    }

    public void DetectedEnemy(Vector3 enemy)
    {
        for (int i =  0; i < soldiers.Length; i++)
        {
            if (soldiers[i].target == null)
            {
                soldiers[i].destination = enemy;
                soldiers[i].navAgent.isStopped = false;
            }
        }
    }

    public void LostEnemy(Soldier soldier)
    {
        Transform enemy = null;
        for (int i =  0; i < soldiers.Length; i++)
        {
            if (soldiers[i].attacking)
            {
                enemy = soldiers[i].target;
                break;
            }
        }

        if (enemy != null && aggression != 0)
        {
            soldier.destination = enemy.position;
            soldier.navAgent.isStopped = false;
        } else {
            if (AI_FindCheckpoints) Checkpoint(aggression);
            else Wander();
        }
    }

    void Checkpoint(int aggro)
    {
        average = AveragePosition();
        Checkpoint closest = null;
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (aggro == 1 && (checkpoints[i].control == -1 || checkpoints[i].control == 0)) continue;
            if (aggro == 2 && checkpoints[i].control == 1) continue;

            if (closest == null)
            {
                closest = checkpoints[i];
                continue;
            }
            if (Vector3.Distance(closest.transform.position, average) > Vector3.Distance(checkpoints[i].transform.position, average)) closest = checkpoints[i];
        }

        if (closest == currentCheckpoint) return;

        if (aggro == 2 && closest == null)
        {
            Checkpoint(1);
            return;
        } else if (aggro == 1 && closest == null)
        {
            FindOthers();
            return;
        }

        for (int i =  0; i < soldiers.Length; i++)
        {
            soldiers[i].destination = closest.transform.position;// + randomOffset;
            soldiers[i].navAgent.isStopped = false;
        }
    }

    public void EnterCheckpoint(Soldier soldier, Checkpoint checkpoint)
    {
        if (soldier == leader)
        {
            inCheckpoint = true;
            currentCheckpoint = checkpoint;
        }
    }

    public void ExitCheckpoint(Soldier soldier)
    {
        if (soldier == leader)
        {
            inCheckpoint = false;
            currentCheckpoint = null;
        }
    }

    void Wander()
    {
        float radius = maxWanderDistance;
        Vector3 origin = leader.transform.position;
        if (currentCheckpoint != null)
        {
            radius = currentCheckpoint.radius+trueStoppingDistance;
            origin = currentCheckpoint.transform.position;
        }

        for (int i = 0; i < soldiers.Length; i++)
        {
            Vector3 randomPosition = Random.insideUnitSphere * radius;
            randomPosition += origin;
            soldiers[i].destination = randomPosition;// + randomOffset;
            soldiers[i].navAgent.isStopped = false;
        }
    }

    void FindOthers()
    {
        GameObject[] squads = GameObject.FindGameObjectsWithTag("Squad");
        newDestination = Vector3.zero;

        //find and go to other soldiers
        for (int i = 0; i < squads.Length; i++ )
        {
            if (squads[i] != gameObject)
            {
                newDestination = squads[i].GetComponent<SquadAI>().AveragePosition();;
                break;
            }
        }

        if (newDestination == Vector3.zero) newDestination = spawner.transform.position;

        for (int i =  0; i < soldiers.Length; i++)
        {
            soldiers[i].destination = newDestination;// + randomOffset;
            soldiers[i].navAgent.isStopped = false;
        }
    }

    public Vector3 AveragePosition()
    {
        average = Vector3.zero;
        for (int i =  0; i < soldiers.Length; i++)
        {
            average += soldiers[i].transform.position;
        }

        return average/soldiers.Length;
    }


    //click on squad, click on where you want them to go
    //click on troup button, click on where you want them to go

    //meanwhile, non-selected troups go to where they're told, then follow AI
                //gunshot: attack or defend = go towards the gun shot sound (if not too far away + no too deep in enemy territory for defense)
                //wander around when they're in a checkpoint


}