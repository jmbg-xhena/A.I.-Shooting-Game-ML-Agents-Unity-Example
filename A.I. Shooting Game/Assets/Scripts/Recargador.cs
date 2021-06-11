using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recargador : MonoBehaviour
{
    MeshRenderer mesh;
    Collider col;
    bool activando=false;
    // Start is called before the first frame update

    private void Start()
    {
        mesh = gameObject.GetComponent<MeshRenderer>();
        col = gameObject.GetComponent<SphereCollider>();
    }
     
    private void Update()
    {
        if (!mesh.enabled && !activando)
        {
            activando = true;
            Invoke("activate", 20f);
        }
    }

    public void activate() {
        mesh.enabled = true;
        col.enabled = true;
        activando = false;
        CancelInvoke("activate");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("agent"))
        {
            ShootingAgent script = other.gameObject.GetComponent<ShootingAgent>();
            if (script.mun_count < 1)
            {
                script.mun_count = script.original_mun_count;
                script.empty_mun = false;
                script.reloading = true;
                script.AddReward(100f);
                mesh.enabled = false;
                col.enabled = false;
                print("recarga");
            }
            else
            {
                script.AddReward(-30f);
            }
        }
        if (other.transform.CompareTag("enemy"))
        {
            VsAgent script = other.gameObject.GetComponent<VsAgent>();
            if (script.mun_count < 1)
            {
                script.mun_count = script.original_mun_count;
                script.empty_mun = false;
                script.reloading = true;
                script.AddReward(100f);
                mesh.enabled = false;
                col.enabled = false;
                print("recarga");
            }
            else {
                script.AddReward(-30f);
            }
        }
    }
}
