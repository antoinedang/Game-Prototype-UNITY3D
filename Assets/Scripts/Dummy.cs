using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour
{
    //on hit spawn explosion effect
    
    public GameObject explosionEffect;
    bool firstTime = true;
    GameObject newEffect = null;

    // Update is called once per frame
    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.layer == 9 && newEffect == null) //if its a bullet
        {
            newEffect = GameObject.Instantiate(explosionEffect);
        } 
        
        if (coll.gameObject.layer == 9) {
            newEffect.transform.position = transform.position;
            newEffect.GetComponent<ParticleSystem>().Clear();
            newEffect.GetComponent<ParticleSystem>().Play();
        }
    }
}
