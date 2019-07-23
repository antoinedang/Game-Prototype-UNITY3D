using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : MonoBehaviour
{
    [HideInInspector]
	public NavMeshAgent navAgent;

	public float randomness = 0.25f;

    public float turningSpeed = 180f;

    public float shotsPerSecond = 5;

    [HideInInspector]
    public Vector3 destination;

    float lastShotTime = 0f;

    public int updateInterval = 10;

    public GameObject bullet;

    public float bulletSpeed = 100f;

    public float marginOfError = 15f;

    [HideInInspector]
    public SquadAI squad;

	[HideInInspector]
	public Transform target;

    [HideInInspector]
    public bool attacking;

    [HideInInspector]
    public bool retreat;

    // Start is called before the first frame update
    void Awake()
    {
        target = null;
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.acceleration += Random.Range(-1f,1f)*randomness*navAgent.acceleration;
        navAgent.angularSpeed += Random.Range(-1f,1f)*randomness*navAgent.angularSpeed;

        transform.localScale += Random.Range(-1f,1f)*randomness*transform.localScale;

        transform.GetChild(0).GetComponent<SphereCollider>().radius /= transform.localScale.x;
    }

    public void Attack(Transform enemy)
    {
        squad.DetectedEnemy(enemy.position);
    	target = enemy;
        attacking = true;
    }

    public void StopAttacking()
    {
        target = null;
        lastShotTime = 0f;
        squad.LostEnemy(this);
        attacking = false;
        //continue to destination
    }

    void Update()
    {
        if (Time.frameCount%updateInterval != 0) return;

        if (target != null)
        {
            navAgent.isStopped = true;
            Vector3 targetDir = target.position - transform.position;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, turningSpeed * Time.deltaTime * updateInterval, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);

            if (lastShotTime == 0f && Vector3.Dot(targetDir, newDir) >= 1f-(marginOfError/180f)) lastShotTime = Time.time;

            if (Vector3.Dot(targetDir, newDir) >= 1f-(marginOfError/180f) && Time.time >= lastShotTime+(1f/shotsPerSecond))
            {
                lastShotTime = Time.time;
                Shoot();
            }
        }

        if (target == null && destination != Vector3.zero)
        {
            if (navAgent.hasPath && navAgent.destination == destination) navAgent.isStopped = false;
            else navAgent.destination = destination;
            destination = Vector3.zero;
        }
    }

    //go to where they hear the bullets coming from

    //go towards the checkpoint

    void Shoot()
    {
        GameObject newBullet = Instantiate(bullet, transform.position, Quaternion.identity);
        newBullet.GetComponent<Rigidbody>().AddForce((target.position-transform.position).normalized*bulletSpeed);
    }

}
