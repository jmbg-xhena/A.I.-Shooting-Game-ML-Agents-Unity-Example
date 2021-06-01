using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;

public class VsAgent : Agent
{
    public GameObject mina;
    public int score = 0;
    public float speed = 3f;
    public float rotationSpeed = 3f;
    public float initRotationSpeed;
    public float jumpforce = 3f;
    public float original_jumpforce;
    public int jumps = 0;
    public bool jumping = false;
    public Text recompensa_text;
    public Text score_text;
    public Text recarga_text;

    public Transform shootingPoint;
    public int minStepsBetweenShots = 50;
    public int damage = 100;
    public int original_bullets_count;
    public int bullets_count = 3;
    public int mun_count = 2;
    public int original_mun_count = 2;
    public bool reloading = false;
    public float reload_steps;
    public int steps_reloading;
    public bool reloading_mina = false;
    public float reload_steps_mina;
    public int steps_reloading_mina;
    public bool reloading_dash = false;
    public float reload_steps_dash;
    public int steps_reloading_dash;
    public bool dashing = false;
    public bool empty_mun = false;
    public Animator anim;

    public Projectile projectile;

    private bool ShotAvaliable = true;
    private int StepsUntilShotIsAvaliable = 0;

    private Vector3 StartingPosition;
    private Rigidbody Rb;
    private EnvironmentParameters EnvironmentParameters;

    public event Action OnEnvironmentReset;

    //public GameObject mun;
    public GameObject[] recargador;
    public GameObject oponent;
    public float multip = 1;

    public ParticleSystem jump;
    public ParticleSystem dash;
    public ParticleSystem hit;
    public ParticleSystem expl;

    private void Shoot()
    {
        if (!ShotAvaliable || reloading || empty_mun)
            return;

        var layerMask = 1 << LayerMask.NameToLayer("Agent");
        var direction = transform.forward;

        var spawnedProjectile = Instantiate(projectile,shootingPoint.position, Quaternion.Euler(0f, -90f, 0f));
        spawnedProjectile.SetDirection(direction);

        bullets_count--;

        ShotAvaliable = false;
        StepsUntilShotIsAvaliable = minStepsBetweenShots;

        if (Physics.Raycast(shootingPoint.position, direction, out var hit, 100f))
        {
            if (hit.transform.CompareTag("agent"))
            {
                AddReward(100f*multip);
                //score++;
                //score_text.text = score.ToString();
                //multip += 0.1f;
                //hit.rigidbody.gameObject.GetComponent<ShootingAgent>().die();
                //hit.rigidbody.gameObject.GetComponent<ShootingAgent>().hit.gameObject.transform.position = hit.transform.position;
                //hit.rigidbody.gameObject.GetComponent<ShootingAgent>().hit.Play();
                //EndEpisode();
                hit.transform.GetComponent<ShootingAgent>().SetReward(-10);
            }
            else
            {
                AddReward(-10f);
            }
        }
        if (bullets_count <= 0 && mun_count > 0)
        {
            mun_count--;
            reloading = true;
            //AddReward(-0.01f);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.ShotAvaliable);
        sensor.AddObservation(this.transform.rotation.y);
        sensor.AddObservation(this.bullets_count);
        sensor.AddObservation(this.mun_count);
        sensor.AddObservation(oponent.transform.localPosition);
        sensor.AddObservation(Vector3.Distance(this.transform.localPosition,oponent.transform.localPosition));
        //sensor.AddObservation(Vector3.Distance(this.transform.localPosition,this.mun.transform.localPosition));
        //sensor.AddObservation(Vector3.Distance(this.transform.localPosition, this.recargador.transform.localPosition));
    }

    private void Update()
    {
        if (empty_mun)
        {
            recarga_text.enabled = true;
        }
        else
        {
            recarga_text.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        score_text.text = score.ToString();
        if (Rb.velocity.x >= 3 || Rb.velocity.x <= -3 || Rb.velocity.z >= 3 || Rb.velocity.z <=-3) {
            AddReward(0.2f);
        }
        else
        {
            AddReward(-0.1f);
        }
         
        if (Vector3.Distance(this.transform.localPosition, this.oponent.transform.localPosition) <= 9f && Vector3.Distance(this.transform.localPosition, this.oponent.transform.localPosition) > 4f)
        { 
            AddReward(0.2f);
        }

        if (Vector3.Distance(this.transform.localPosition, this.oponent.transform.localPosition) < 4f)
        {
            AddReward(0.3f);
        }

        if (Vector3.Distance(this.transform.localPosition, this.oponent.transform.localPosition) > 9f)
        {
            AddReward(-0.1f);
        }


        if (Physics.Raycast(shootingPoint.position, transform.forward, out var hit, 8f))
        {

            if (hit.transform.CompareTag("agent"))
            {
                //Debug.DrawRay(shootingPoint.position, transform.forward * 5, Color.green,0.3f);
                AddReward(80f / MaxStep);
                rotationSpeed = 0;
                jumpforce = 0;
                if (ShotAvaliable && !reloading && !empty_mun)
                    anim.SetTrigger("shoot");
                Invoke("Shoot", 1f);
            }
            else
            {
                rotationSpeed = initRotationSpeed;
                jumpforce = original_jumpforce;
                if (Physics.Raycast(shootingPoint.position, transform.forward + (Vector3.left / 8), out var hit1, 8f))
                {
                    if (hit1.transform.CompareTag("agent"))
                    {
                        //Debug.DrawRay(shootingPoint.position, (transform.forward + (Vector3.left / 8)) * 5, Color.blue,0.3f);
                        AddReward(20f / MaxStep);
                    }
                    else
                    {
                        if (Physics.Raycast(shootingPoint.position, transform.forward + (Vector3.right / 8), out var hit2, 8f))
                        {

                            if (hit2.transform.CompareTag("agent"))
                            {
                                //Debug.DrawRay(shootingPoint.position, (transform.forward + (Vector3.right / 8)) * 5, Color.red,0.3f);
                                AddReward(20f / MaxStep);
                            }
                        }
                    }
                }
            }
        }

        AddReward(-500f / MaxStep);

        if (mun_count <= 0)
        {
            if (reloading)
            {
                reloading = false;
            }
            empty_mun = true;
            //AddReward(-0.001f);
        }

        recompensa_text.text = GetCumulativeReward().ToString();
        if (!ShotAvaliable && !empty_mun)
        {
            StepsUntilShotIsAvaliable--;

            if (StepsUntilShotIsAvaliable <= 0)
            {
                ShotAvaliable = true;
            }
        }

        if (reloading)
        {
            steps_reloading++;
        }
        if ((steps_reloading > reload_steps) && reloading)
        {
            steps_reloading = 0;
            reloading = false;
            empty_mun = false;
            bullets_count = original_bullets_count;
        }

        if (reloading_mina)
        {
            steps_reloading_mina++;
        }
        if ((steps_reloading_mina > reload_steps_mina) && reloading_mina)
        {
            steps_reloading_mina = 0;
            reloading_mina = false;
        }

        if (reloading_dash)
        {
            steps_reloading_dash++;
        }
        if ((steps_reloading_dash > reload_steps_dash) && reloading_dash)
        {
            steps_reloading_dash = 0;
            reloading_dash = false;
            dashing = false;
        }

        if (jumping)
        {
            Rb.AddForce(new Vector3(0, -9.8f, 0), ForceMode.VelocityChange);
        }

        if (transform.localPosition.y <= -10)
        {
            AddReward(-100f);
            EndEpisode();
        }
        if (transform.localPosition.y > 12)
        {
            print("muy alto");
            AddReward(-50f);
            oponent.GetComponent<ShootingAgent>().EndEpisode();
            EndEpisode();
        }

        /*if (Input.GetKeyDown(KeyCode.Space) && !jumping && jumps<1) {
            jumps++;
            //print("jump");
            jumping = true;
            Rb.AddForce(new Vector3(0, jumpforce, 0), ForceMode.VelocityChange);
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            reloading = true;
        }*/

    }
    public override void OnActionReceived(float[] vectorAction)
    {
        if (Mathf.RoundToInt(vectorAction[4]) >= 1 && !jumping && jumps < 1)
        {
            jump.transform.position = transform.position+Vector3.down*3;
            jump.Play();
            //print("jump");
            jumps++;
            jumping = true;
            Rb.AddForce(new Vector3(0, jumpforce, 0), ForceMode.VelocityChange);
            //AddReward(0.005f);
            AddReward(-0.1f);
        }

        if (Mathf.RoundToInt(vectorAction[0]) >= 1)
        {
            if (ShotAvaliable && !reloading && !empty_mun)
                anim.SetTrigger("shoot");
            Invoke("Shoot", 1f);
            //Shoot();
        }

        if (Mathf.RoundToInt(vectorAction[5]) >= 1 && !reloading_mina && !jumping)
        {
            mina.transform.localPosition = transform.localPosition;
            reloading_mina = true;
        }

        /*if (Mathf.RoundToInt(vectorAction[5]) >= 1)
        {
            reloading = true;
        }*/

        if (Mathf.RoundToInt(vectorAction[6]) >= 1 && !dashing)
        {
            dash.transform.position = transform.position;
            dash.Play();
            Rb.velocity = new Vector3(vectorAction[2] * speed, 0, vectorAction[1] * speed)*100;
            dashing = true;
            reloading_dash = true;
             //AddReward(7.5f);
        }
        else {
            Rb.velocity = new Vector3(vectorAction[2] * speed, 0, vectorAction[1] * speed);
        }
        transform.Rotate(Vector3.up, vectorAction[3] * rotationSpeed);
    }

    public override void Initialize()
    {
        reload_steps_mina = reload_steps * 4;
        reload_steps_dash = reload_steps * 2;
        score_text.text = score.ToString();
        initRotationSpeed = rotationSpeed;
        original_jumpforce = jumpforce;
        StartingPosition = transform.position;
        Rb = GetComponent<Rigidbody>();
        bullets_count = original_bullets_count;
        mun_count = original_mun_count;
        //TODO: Delete
        Rb.freezeRotation = true;
        EnvironmentParameters = Academy.Instance.EnvironmentParameters;
        //mun.transform.localPosition = new Vector3(UnityEngine.Random.Range(-6f, 6f), 1, UnityEngine.Random.Range(-6f, 6f));
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetMouseButtonDown(0) ? 1f : 0f;
        actionsOut[1] = Input.GetAxis("Horizontal");
        actionsOut[2] = -Input.GetAxis("Vertical");
        actionsOut[3] = Input.GetAxis("Rotate");
        actionsOut[4] = Input.GetKeyDown(KeyCode.Space) ? 1f : 0f;
        actionsOut[5] = Input.GetMouseButtonDown(1) ? 1f : 0f;
        actionsOut[6] = Input.GetKeyDown(KeyCode.C)? 1f : 0f;
        //actionsOut[5] = Input.GetKeyDown(KeyCode.R) ? 1f : 0f;
    }

    public override void OnEpisodeBegin()
    {
        score_text.text = score.ToString();
        mina.transform.position = Vector3.up * 200;
        OnEnvironmentReset?.Invoke();
        //Load Parameter from Curciulum
        minStepsBetweenShots = Mathf.FloorToInt(EnvironmentParameters.GetWithDefault("shootingFrequenzy", 30f));
        bullets_count = original_bullets_count;
        mun_count = original_mun_count;
        empty_mun = false;
        steps_reloading = 0;
        //transform.localPosition = new Vector3(UnityEngine.Random.Range(4f, 8f), 1.2f, UnityEngine.Random.Range(-8f, 8f));
        transform.localPosition = new Vector3(UnityEngine.Random.Range(6f,11f),6f, UnityEngine.Random.Range(-30f,5f));
        jumping = true;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        Rb.velocity = Vector3.zero;
        ShotAvaliable = true;
        //mun.transform.localPosition = new Vector3(UnityEngine.Random.Range(-6f, 6f), 1, UnityEngine.Random.Range(-6f, 6f));
        rotationSpeed = initRotationSpeed;
        for (int i = 0; i < recargador.Length; i++)
        {
            recargador[i].SetActive(true);
        } 
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("agent"))
        {
            AddReward(-50f);
            //EndEpisode();
        }
        if (other.gameObject.CompareTag("mun"))
        {
            print("municion");
            //AddReward(100f);
            if (bullets_count <= 0)
            {
                reloading = true;
            }
            empty_mun = false;
            mun_count++;
            other.gameObject.transform.localPosition = new Vector3(UnityEngine.Random.Range(-6f, 6f), 1, UnityEngine.Random.Range(-6f, 6f));
        }
        if (other.gameObject.CompareTag("wall"))
        {
            Rb.AddForce(new Vector3(0, -9.8f, 0), ForceMode.VelocityChange);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        jumping = false;
        if (collision.gameObject.CompareTag("ground"))
        {
            jumps = 0;
        }
        if (collision.gameObject.CompareTag("pared") || collision.gameObject.CompareTag("wall")) 
        {
            AddReward(-0.5f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("vacio"))
        {
            print("vacio");
            AddReward(-2f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("mina azul"))
        {
            AddReward(-50f);
            print("mina azul");
            oponent.GetComponent<ShootingAgent>().AddReward(400);
            oponent.GetComponent<ShootingAgent>().score++;
            expl.transform.position = transform.position;
            expl.Play();
            oponent.GetComponent<ShootingAgent>().EndEpisode();
            EndEpisode();
        }
        if (other.gameObject.CompareTag("ps"))
        {
            AddReward(-50f);
            print("dead hit");
            oponent.GetComponent<ShootingAgent>().AddReward(400);
            oponent.GetComponent<ShootingAgent>().score++;
            hit.transform.position = transform.position;
            hit.Play();
            oponent.GetComponent<ShootingAgent>().EndEpisode();
            EndEpisode();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        jumping = true;
        jumps = 1;
    }
}
