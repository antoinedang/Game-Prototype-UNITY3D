using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class SquadAI : MonoBehaviourPun
{

	public int size;
	public GameObject soldier;
	public int frameInterval = 10;
    public float maxDistanceFromDestination = 3f;
    public float stoppingDistance;
    public float trueStoppingDistance = 2f;
    public float maxWanderDistance = 15f;
    public float maxSelectedWanderDistance = 5f;
    public bool AI_FindCheckpoints = true;
    public bool AI_ExploreGunshots = true;
    public bool AI_Wander = true;
    public int checkpointCheckInterval = 120;
    public int wanderInterval = 60;
    public int aggression = 2; //0 = retreat, 1 = defend, 2 = attack
    Checkpoint currentCheckpoint;
    Vector3 average;
    Soldier leader;
    Vector3 newDestination;
    bool started = false;
    Soldier[] soldiers;



    [Header("DEBUG: don't change")]
    public int orderOfArrival;
    public bool inCheckpoint;
    public Vector3 lastDestination;
    public SpawnOnMouse spawner;
    public Checkpoint[] checkpoints;

    // Start is called before the first frame update
    void Awake()
    {
        AI_FindCheckpoints = (Random.value > 0.5f);
        aggression = Random.Range(0,2);
    	Soldier[] tempArray = new Soldier[size];
    	for (int i = 0; i < size; i++)
    	{
    		GameObject newSoldier = PhotonNetwork.Instantiate("Soldier", transform.position, Quaternion.identity);
            newSoldier.transform.parent = transform;
    		tempArray[i] = newSoldier.GetComponent<Soldier>();
            if (i == 0)
            {
                leader = newSoldier.GetComponent<Soldier>();
                newSoldier.GetComponent<Soldier>().leader = true;
            }
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
            lastDestination = destination + randomOffset;
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
            if (AI_FindCheckpoints && !leader.selected)
            {
                if ((!inCheckpoint || (Time.frameCount+orderOfArrival)%checkpointCheckInterval == 0) && aggression != 0) Checkpoint(aggression);
                else if ((Time.frameCount+orderOfArrival)%wanderInterval == 0) Wander();
                else {
                    for (int i = 0; i < soldiers.Length; i++)
                    {
                        if (soldiers[i].navAgent.remainingDistance <= trueStoppingDistance) soldiers[i].navAgent.isStopped = true;
                    }
                }
            } else if (AI_Wander && (Time.frameCount+orderOfArrival)%wanderInterval == 0) {
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
                lastDestination = enemy;
                soldiers[i].navAgent.isStopped = false;
            }
        }
    }

    public void LostEnemy(Soldier soldier)
    {
        Transform enemy = null;
        for (int i =  0; i < soldiers.Length; i++)
        {
            if (soldiers[i].transform.GetChild(0).GetComponent<DetectEnemy>().enemies.Count != 0)
            {
                enemy = soldiers[i].target;
                break;
            }
        }

        if (enemy != null && aggression != 0 && !leader.retreating)
        {
            lastDestination = enemy.position;
            soldier.destination = enemy.position;
            soldier.navAgent.isStopped = false;
        } else {
            if (enemy == null) for (int i = 0; i < soldiers.Length; i++) { soldiers[i].retreating = false; }
            if (leader.selected) return;
            if (AI_FindCheckpoints) Checkpoint(aggression);
            else Wander();
        }
    }

    void Checkpoint(int aggro)
    {
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
            if (Vector3.Distance(closest.transform.position, leader.transform.position) > Vector3.Distance(checkpoints[i].transform.position, leader.transform.position)) closest = checkpoints[i];
        }

        if (closest == currentCheckpoint) return;

        if (aggro == 2 && closest == null)
        {
            Checkpoint(1);
            return;
        } else if (aggro == 1 && closest == null)
        {
            Wander();
            return;
        }

        for (int i =  0; i < soldiers.Length; i++)
        {
            lastDestination = closest.transform.position;
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
        float radius = leader.selected?maxSelectedWanderDistance:maxWanderDistance;
        Vector3 origin = lastDestination;
        if (currentCheckpoint != null)
        {
            radius = currentCheckpoint.radius+trueStoppingDistance;
            origin = currentCheckpoint.transform.position;
        }

        for (int i = 0; i < soldiers.Length; i++)
        {
            Vector3 randomPosition = Random.insideUnitSphere * radius;
            randomPosition += origin;
            lastDestination = randomPosition;
            soldiers[i].destination = randomPosition;// + randomOffset;
            soldiers[i].navAgent.isStopped = false;
        }
    }

    public void AlertGunshot(Vector3 shooter)
    {
        spawner.Gunshot(shooter);
    }

    public void HighlightSoldiers(Bounds selectionBounds, Camera cam)
    {
        bool selected = false;
        for (int i = 0; i < soldiers.Length; i++)
        {
            if (selectionBounds.Contains(cam.WorldToViewportPoint(soldiers[i].transform.position)))
            {
                selected = true;
                break;
            }
        }

        if (!selected && !leader.highlighted) return;

        for (int i = 0; i < soldiers.Length; i++)
        {
            if (selected) soldiers[i].Highlight();
            else soldiers[i].Deselect();
        }
    }

    public void SelectSoldiers(bool direct = false)
    {
        if (leader.highlighted || direct)
        {
            spawner.soldiersSelected = true;
            for (int i = 0; i < soldiers.Length; i++)
            {
                soldiers[i].Select();
                soldiers[i].navAgent.isStopped = true;
            } 
        }
    }

    public void DeselectSoldiers()
    {
        for (int i =  0; i < soldiers.Length; i++)
        {
            soldiers[i].Deselect();
            soldiers[i].navAgent.isStopped = false;
        } 
    }

    public void MoveSelectedSoldiers(Vector3 newDestination)
    {
        if (!leader.selected) return;
        for (int i = 0; i < soldiers.Length; i++)
        {
            lastDestination = newDestination;
            soldiers[i].destination = Vector3.zero;
            soldiers[i].navAgent.SetDestination(newDestination);
            soldiers[i].navAgent.isStopped = false;
            if (leader.attacking) soldiers[i].retreating = true;
        } 
    }

    public void Gunshot(Vector3 shooter)
    {
        if (aggression == 0 || !AI_ExploreGunshots || leader.attacking || leader.selected)
        {
            return;
        } else if (aggression == 1)
        {
            if (inCheckpoint)
            {
                if (currentCheckpoint.control == 0) return;
            }

            Checkpoint farthest = null;
            for (int i = 0; i < checkpoints.Length; i++)
            {  
                if (checkpoints[i].control == -1 || checkpoints[i].control == 0) continue;

                if (farthest == null)
                {
                    farthest = checkpoints[i];
                    continue;
                }
                if (Vector3.Distance(farthest.transform.position, spawner.transform.position) < Vector3.Distance(checkpoints[i].transform.position, spawner.transform.position)) farthest = checkpoints[i];
            }

            if (farthest != null)
            {
                if (Vector3.Distance(shooter, spawner.transform.position) > Vector3.Distance(farthest.transform.position, spawner.transform.position) + farthest.radius) return;
            }
        }

        for (int i =  0; i < soldiers.Length; i++)
        {
            lastDestination = shooter;
            soldiers[i].destination = shooter;// + randomOffset;
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
            lastDestination = newDestination;
            soldiers[i].destination = newDestination;// + randomOffset;
            soldiers[i].navAgent.isStopped = false;
        }
    }

    public void Despawn(Soldier deadSoldier)
    {
        size --;
        if (size == 0) Destroy(gameObject);
        Soldier[] tempArray = new Soldier[size];
        for (int i = 0; i < soldiers.Length; i++)
        {
            if (soldiers[i] == null) return;
            if (soldiers[i] != deadSoldier) tempArray[i] = soldiers[i];
        }

        soldiers = tempArray;
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
                //wander around when they're in a checkpointà

        //if too far into enemy territory (= if there is no checkpoint)


}