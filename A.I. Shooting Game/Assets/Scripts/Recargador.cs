using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recargador : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("agent"))
        {
            ShootingAgent script = other.gameObject.GetComponent<ShootingAgent>();
            if (script.mun_count < 1)
            {
                //print("recarga");
                script.mun_count = script.original_mun_count;
                script.empty_mun = false;
                script.reloading = true;
                script.AddReward(25f);
            }
        }
    }
}
