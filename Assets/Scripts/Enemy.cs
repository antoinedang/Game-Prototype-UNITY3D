using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Enemy : MonoBehaviour
{

	public float health = 2f;
	Rigidbody rb;
    public Material enemyMaterial;
    Renderer render;
    PhotonView photonView;

    // Start is called before the first frame update
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        render = GetComponent<Renderer>();
        rb = gameObject.AddComponent<Rigidbody>() as Rigidbody;
        rb.angularDrag = 0;
        rb.detectCollisions = false;
        rb.isKinematic = true;
        rb.useGravity = false;
        render.material = enemyMaterial;
        gameObject.layer = 10;
        PhotonNetwork.Destroy(transform.GetChild(0).gameObject);
        PhotonNetwork.Destroy(transform.GetChild(1).gameObject);
    }

    public void Damage(float damage)
    {
    	health -= damage;

    	if (health <= 0f)
    	{
    		photonView.RPC("Despawn", RpcTarget.All);
    		PhotonNetwork.Destroy(gameObject);
    	}
    }
}
