using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerBullet : MonoBehaviour
{
	public float damage = 1f;
	public float airTime = 1.5f;
    // Start is called before the first frame update
    void Start()
    {
        transform.up = GetComponent<Rigidbody>().velocity;
        Invoke("Delete", airTime);
    }

    void Delete()
    {
    	PhotonNetwork.Destroy(gameObject);
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider coll)
    {
        coll.gameObject.GetComponent<Enemy>().Damage(damage);
        Delete();
    }
}
