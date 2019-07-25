using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class Soldier : MonoBehaviourPun
{

	public float randomness = 0.25f;
    public float turningSpeed = 180f;
    public float shotsPerSecond = 5;
    public int updateInterval = 10;
    public GameObject bullet;
    public float bulletSpeed = 100f;
    public float marginOfError = 15f;
    public Color highlightedColor;
    public Color selectedColor;
    public Color normalColor;
    public Behaviour[] componentsToEnable;
    public Behaviour[] componentsToDisable;
    Renderer render;
    float lastShotTime = 0f;
    float defaultTurnSpeed;


    //gunshots
    //enemyies/soldiers dying
    //soldier count
    //checkpoint status/color
    //

    [Header("DEBUG: don't change")]
    public SquadAI squad;
    public Transform target;
    public Vector3 destination;
    public bool attacking;
    public NavMeshAgent navAgent;
    public bool highlighted = false;
    public bool selected = false;
    public bool retreating;
    public bool leader = false;
    PhotonView photonView;


    // Start is called before the first frame update
    void Awake()
    {
        render = GetComponent<Renderer>();
        photonView = GetComponent<PhotonView>();
        transform.localScale += Random.Range(-1f,1f)*randomness*transform.localScale;


        if (!photonView.IsMine)
        {
            for (int i = 0; i < componentsToEnable.Length; i++)
            {
                componentsToEnable[i].enabled = true;
            }

            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }

        target = null;
        navAgent = GetComponent<NavMeshAgent>();
        defaultTurnSpeed = navAgent.angularSpeed;
        navAgent.acceleration += Random.Range(-1f,1f)*randomness*navAgent.acceleration;
        navAgent.angularSpeed += Random.Range(-1f,1f)*randomness*navAgent.angularSpeed;
        transform.GetChild(0).GetComponent<SphereCollider>().radius /= transform.localScale.x;
    }

    public void Attack(Transform enemy)
    {
        navAgent.isStopped = true;
        squad.DetectedEnemy(enemy.position);
    	target = enemy;
        destination = enemy.position;
        attacking = true;
        navAgent.angularSpeed = turningSpeed;
    }

    public void StopAttacking()
    {
        target = null;
        lastShotTime = 0f;
        squad.LostEnemy(this);
        attacking = false;
        navAgent.angularSpeed = defaultTurnSpeed;
        //continue to destination
    }

    void Update()
    {
        if (Time.frameCount%updateInterval != 0 || !photonView.IsMine) return;

        if (target != null)
        {
            if (!retreating)
            {
                navAgent.isStopped = true;
                Quaternion targetDir = Quaternion.LookRotation(target.position - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetDir, turningSpeed * Time.deltaTime * updateInterval);

                float dotProduct = Vector3.Dot(targetDir.eulerAngles, transform.rotation.eulerAngles);

                if (lastShotTime == 0f && dotProduct >= 1f-(marginOfError/360f)) lastShotTime = Time.time;

                if (dotProduct >= 1f-(marginOfError/360f) && Time.time >= lastShotTime+(1f/shotsPerSecond))
                {
                    lastShotTime = Time.time;
                    Shoot();
                }
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
        if (leader)
        {
            squad.AlertGunshot(transform.position);
            photonView.RPC("EnemyGunshot", RpcTarget.All, transform.position);
        }
        GameObject newBullet = PhotonNetwork.Instantiate("Bullet", transform.position, Quaternion.identity);
        newBullet.GetComponent<Rigidbody>().AddForce((target.position-transform.position).normalized*bulletSpeed);
    }

    void OnMouseUp()
    {
        if (photonView.IsMine) squad.SelectSoldiers(true);
    }

    [PunRPC]
    public void Despawn()
    {
        squad.Despawn(this);
    }

    public void Highlight()
    {
        highlighted = true;
        selected = false;
        retreating = false;
        render.material.color = highlightedColor;
    }

    public void Select()
    {
        selected = true;
        render.material.color = selectedColor;
    }

    public void Deselect()
    {
        highlighted = false;
        selected = false;
        retreating = false;
        render.material.color = normalColor;
    }
}
