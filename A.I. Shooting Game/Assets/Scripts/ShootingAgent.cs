using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;


public class ShootingAgent : Agent
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
    //public Text recompensa_text;
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
    public EnemyManager enemyManager;

    private bool ShotAvaliable = true;
    private int StepsUntilShotIsAvaliable = 0;

    private Vector3 StartingPosition;
    private Rigidbody Rb;
    private EnvironmentParameters EnvironmentParameters;

    public event Action OnEnvironmentReset;

    //public GameObject mun;
    public GameObject recargador;
    public GameObject oponent;
    public float multip = 1;
    public ParticleSystem jump;
    public ParticleSystem dash;
    public ParticleSystem hit;
    public ParticleSystem expl;
    private Vector3 move;
    private float rotation;
    public Projectile spawnedProjectile;
    public bool mirando;
    public float init_height;


    private void Shoot()
    {
        if (!ShotAvaliable || reloading || empty_mun)
            return;

        var layerMask = 1 << LayerMask.NameToLayer("Enemy");
        var direction = transform.forward;

        spawnedProjectile = Instantiate(projectile, shootingPoint.position, Quaternion.Euler(0f, -90f, 0f));
        spawnedProjectile.SetDirection(direction);

        bullets_count--;

        ShotAvaliable = false;
        StepsUntilShotIsAvaliable = minStepsBetweenShots;

        if (Physics.Raycast(shootingPoint.position, direction, out var hit, 100f))
        {
            if (hit.transform.CompareTag("enemy")) {
                //hit.transform.GetComponent<Enemy>().GetShot(damage, this);
                hit.transform.GetComponent<VsAgent>().SetReward(-10);
                //score++;
                //score_text.text = score.ToString();
                //multip += 0.1f;
                //hit.rigidbody.gameObject.GetComponent<VsAgent>().hit.gameObject.transform.position = hit.transform.position;
                //hit.rigidbody.gameObject.GetComponent<VsAgent>().hit.Play();
                //hit.transform.GetComponent<VsAgent>().EndEpisode(); 
                //EndEpisode();
                AddReward(10f);
                hit.transform.GetComponent<VsAgent>().AddReward(-20);
                //print("die");
            }
            else 
            {
                AddReward(-20f);
            }
        }
        if (bullets_count <= 0 && mun_count > 0)
        {
            mun_count--;
            reloading = true;
            //AddReward(-0.001f);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.ShotAvaliable);
        sensor.AddObservation(this.transform.rotation.y);
        sensor.AddObservation(oponent.transform.rotation.y);
        sensor.AddObservation(this.bullets_count);
        sensor.AddObservation(this.mun_count);
        sensor.AddObservation(this.empty_mun);
        sensor.AddObservation(this.mirando);
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(oponent.transform.localPosition);
        sensor.AddObservation(oponent.GetComponent<VsAgent>().mina.transform.localPosition);
        /*if (oponent.GetComponent<ShootingAgent>().spawnedProjectile) {
            sensor.AddObservation(oponent.GetComponent<ShootingAgent>().spawnedProjectile.transform.localPosition);
        }*/
        sensor.AddObservation(Vector3.Distance(this.transform.localPosition, oponent.transform.localPosition));
        //sensor.AddObservation(Vector3.Distance(this.transform.localPosition,this.mun.transform.localPosition));
        //sensor.AddObservation(Vector3.Distance(this.transform.localPosition, this.recargador.transform.localPosition));
    }

    private void Update()
    {
        //print((int)Vector3.Distance(this.transform.position, this.oponent.transform.position));

        if (this.transform.localPosition.y > init_height - 1f && this.transform.localPosition.y < init_height + 1f)
        {
            AddReward(1f);
        }
        if (Vector3.Distance(this.transform.localPosition, this.oponent.transform.localPosition) <= 14f && Vector3.Distance(this.transform.localPosition, this.oponent.transform.localPosition) > 6f)
        {
            AddReward(0.6f);
            //print("muy cerca");
        }

        if (Vector3.Distance(this.transform.localPosition, this.oponent.transform.localPosition) < 6f)
        {
            AddReward(1.2f);
            //print("cerca");
        }

        if (Vector3.Distance(this.transform.localPosition, this.oponent.transform.localPosition) > 14f)
        {
            AddReward(-1.2f);
            //print("lejos");
        }
        if (empty_mun)
        {
            recarga_text.enabled = true;
        }
        else
        {
            recarga_text.enabled = false;
        }

        move.z = Input.GetAxis("Horizontal");
        move.x = -Input.GetAxis("Vertical");
        rotation = Input.GetAxis("Rotate");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump.transform.position = transform.position + Vector3.down * 3;
            jump.Play();
            //print("jump");
            jumps++;
            jumping = true;
            Rb.AddForce(new Vector3(0, jumpforce, 0), ForceMode.VelocityChange);
            //AddReward(0.0005f);
            AddReward(-0.01f);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (ShotAvaliable && !reloading && !empty_mun)
                anim.SetTrigger("shoot");
            Invoke("Shoot", 1f);
        }
        if (Input.GetMouseButtonDown(1) && !reloading_mina && !jumping)
        {
            reloading_mina = true;
            mina.transform.localPosition = transform.localPosition;
        }
        if (!dashing)
        {
            Rb.velocity = new Vector3(move.x * speed, 0, move.z * speed);
        }
        else {
            dashing=false;
        }

        if (Input.GetKeyDown(KeyCode.C) && !reloading_dash)
        {
            dash.transform.position = transform.position;
            dash.Play();
            Rb.velocity = new Vector3(move.x *30* speed, 0, move.z *30 * speed);
            dashing = true;
            reloading_dash = true;
            //AddReward(0.1f);
        }
        transform.Rotate(Vector3.up, rotation/5 * rotationSpeed);
    }

    private void FixedUpdate()
    {
        score_text.text = score.ToString();
        if (Rb.velocity.x >= 3 || Rb.velocity.x <= -3 || Rb.velocity.z >= 3 || Rb.velocity.z <= -3)
        {
            AddReward(0.02f);
        }
        else
        {
            AddReward(-0.03f);
        }

        if (Physics.Raycast(shootingPoint.position, transform.forward, out var hit, 8f))
        {

            if (hit.transform.CompareTag("enemy"))
            {
                //Debug.DrawRay(shootingPoint.position, transform.forward * 5, Color.green,0.3f);
                AddReward(8f / MaxStep);
                mirando = true;
                if (hit.transform.GetComponent<VsAgent>()) {
                    hit.transform.GetComponent<VsAgent>().SetReward(4 / MaxStep);
                }

                /*rotationSpeed = 0;
                jumpforce = 0;
                if (ShotAvaliable && !reloading && !empty_mun)
                    anim.SetTrigger("shoot");
                Invoke("Shoot", 1f);*/
            }
            else {
                jumpforce = original_jumpforce;
                rotationSpeed = initRotationSpeed;
                if (Physics.Raycast(shootingPoint.position, transform.forward + (Vector3.left / 8), out var hit1, 8f))
                {
                    if (hit1.transform.CompareTag("enemy"))
                    {
                        //Debug.DrawRay(shootingPoint.position, (transform.forward + (Vector3.left / 8)) * 5, Color.blue,0.3f);
                        AddReward(2f / MaxStep);
                        mirando = true;
                        if (hit.transform.GetComponent<VsAgent>())
                        {
                            hit.transform.GetComponent<VsAgent>().SetReward(1 / MaxStep);
                        }
                    }
                    else
                    {
                        if (Physics.Raycast(shootingPoint.position, transform.forward + (Vector3.right / 8), out var hit2, 8f))
                        {

                            if (hit2.transform.CompareTag("enemy"))
                            {
                                //Debug.DrawRay(shootingPoint.position, (transform.forward + (Vector3.right / 8)) * 5, Color.red,0.3f);
                                AddReward(2f / MaxStep);
                                mirando = true;
                                if (hit.transform.GetComponent<VsAgent>())
                                {
                                    hit.transform.GetComponent<VsAgent>().SetReward(1 / MaxStep);
                                }
                                else {
                                    mirando = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        AddReward(-5f / MaxStep);

        if (mun_count <= 0)
        {
            if (reloading) {
                reloading = false;
            }
            empty_mun = true;
            //AddReward(-0.001f);
        }

        //recompensa_text.text = GetCumulativeReward().ToString();
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
        }

        if (jumping)
        {
            Rb.AddForce(new Vector3(0, -9.8f, 0), ForceMode.VelocityChange);
        }

        if (transform.localPosition.y <= -10)
        {
            //enemyManager.SetEnemiesActive();
            AddReward(-500f);
            score--;
            EndEpisode();
        }
        if (transform.localPosition.y > 20)
        { 
            print("muy alto2");
            //enemyManager.SetEnemiesActive();
            AddReward(-100f);
            oponent.GetComponent<VsAgent>().EndEpisode();
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
       /* if (Mathf.RoundToInt(vectorAction[4]) >= 1 && !jumping && jumps<1)
        {
            jump.transform.position = transform.position+Vector3.down;
            jump.Play();
            //print("jump");
            jumps++;
            jumping = true;
            Rb.AddForce(new Vector3(0, jumpforce, 0), ForceMode.VelocityChange);
            //AddReward(0.0005f);
            AddReward(-2f);
        }

        if (Mathf.RoundToInt(vectorAction[0]) >= 1)
        {
            if (ShotAvaliable && !reloading && !empty_mun)
                anim.SetTrigger("shoot");
            Invoke("Shoot", 1f);
        } 
        if (Mathf.RoundToInt(vectorAction[5]) >= 1 && !reloading_mina && !jumping)
        {
            reloading_mina = true;
            mina.transform.localPosition = transform.localPosition;
        }

        /*if (Mathf.RoundToInt(vectorAction[5]) >= 1)
        {
            reloading = true;
        }*/
        /*if (Mathf.RoundToInt(vectorAction[6]) >= 1 && !dashing)
        {
            dash.transform.position = transform.position;
            dash.Play();
            Rb.velocity = new Vector3(vectorAction[2] * speed, 0, vectorAction[1] * speed) * 30;
            dashing = true;
            reloading_dash = true;
            //AddReward(0.1f);
        }
        else
        {
            Rb.velocity = new Vector3(vectorAction[2] * speed, 0, vectorAction[1] * speed);
        }
        transform.Rotate(Vector3.up, vectorAction[3] * rotationSpeed);*/
    }

    public override void Initialize()
    {
        reload_steps_mina = reload_steps * 4;
        reload_steps_dash = reload_steps * 2;
        original_jumpforce = jumpforce;
        score_text.text = score.ToString();
        initRotationSpeed = rotationSpeed;
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
        /*actionsOut[0] = Input.GetMouseButtonDown(0) ? 1f : 0f;
        actionsOut[1] = Input.GetAxis("Horizontal");
        actionsOut[2] = -Input.GetAxis("Vertical");
        actionsOut[3] = Input.GetAxis("Rotate");
        actionsOut[4] = Input.GetKeyDown(KeyCode.Space) ? 1f : 0f;
        actionsOut[5] = Input.GetMouseButtonDown(1) ? 1f : 0f;
        actionsOut[6] = Input.GetKeyDown(KeyCode.C) ? 1f : 0f;*/
        //actionsOut[5] = Input.GetKeyDown(KeyCode.R) ? 1f : 0f;
    }

    public override void OnEpisodeBegin()
    {
        init_height = 0;
        score_text.text = score.ToString();
        mina.transform.position = Vector3.up * 200;
        OnEnvironmentReset?.Invoke();
        //Load Parameter from Curciulum
        minStepsBetweenShots = Mathf.FloorToInt(EnvironmentParameters.GetWithDefault("shootingFrequenzy", 30f)); 
        bullets_count = original_bullets_count;
        mun_count = original_mun_count;
        empty_mun = false;
        steps_reloading = 0;
        //transform.localPosition = new Vector3(UnityEngine.Random.Range(-8f, -4f), 1.2f, UnityEngine.Random.Range(-8f, 8f));
        transform.localPosition = new Vector3(UnityEngine.Random.Range(-11f,-6f), 6f, UnityEngine.Random.Range(-30f, 5f));
        jumping = true;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        Rb.velocity = Vector3.zero;
        ShotAvaliable = true;
        //mun.transform.localPosition = new Vector3(UnityEngine.Random.Range(-6f, 6f), 1, UnityEngine.Random.Range(-6f, 6f));
        rotationSpeed = initRotationSpeed;
    }

    public void RegisterKill()
    {
        score++;
        AddReward(16.0f / EnvironmentParameters.GetWithDefault("amountZombies", 8f));
        print("kill");
    }

    public void die() {
        enemyManager.SetEnemiesActive();
        AddReward(-5f);
        print("kill");
        EndEpisode(); 
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("ground")&& init_height == 0)
        {
            init_height = this.transform.localPosition.y;
        }

        if (other.gameObject.CompareTag("enemy"))
        {
            //die();
            AddReward(-5f);
        }
        if (other.gameObject.CompareTag("mun")) {
            print("municion");
            if (bullets_count <= 0) {
                reloading = true;
            }
            empty_mun = false; 
            mun_count++;
            other.gameObject.transform.localPosition = new Vector3(UnityEngine.Random.Range(-6f,6f), 1, UnityEngine.Random.Range(-6f, 6f));
        }
        if (other.gameObject.CompareTag("wall"))
        {
            Rb.AddForce(new Vector3(0, -9.8f, 0), ForceMode.VelocityChange);
        }
        if (other.gameObject.CompareTag("obs"))
        {
            AddReward(-0.5f);
        }

        if (other.gameObject.CompareTag("wall"))
        {
            AddReward(-1f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        jumping = false;
        if (collision.gameObject.CompareTag("ground"))
        {
            jumps = 0;
        }
        if (collision.gameObject.CompareTag("obs"))
        {
            AddReward(-0.001f);
        }
        if (collision.gameObject.CompareTag("pared") || collision.gameObject.CompareTag("wall")) 
        {
            AddReward(-0.2f);
        }
    }
     
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("vacio"))
        {
            print("vacio");
            AddReward(-0.005f);
            Rb.AddForce(Vector3.down * 50);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("vacio"))
        {
            AddReward(-1f);
        }
        if (other.gameObject.CompareTag("mina roja"))
        {
            AddReward(-200f);
            print("mina roja");
            oponent.GetComponent<VsAgent>().AddReward(30);
            oponent.GetComponent<VsAgent>().score++;
            expl.transform.position = transform.position;
            expl.Play();
            oponent.GetComponent<VsAgent>().EndEpisode();
            EndEpisode();
        }
        if (other.gameObject.CompareTag("pvs"))
        {
            AddReward(-400f);
            print("kill hit");
            oponent.GetComponent<VsAgent>().AddReward(80);
            oponent.GetComponent<VsAgent>().score++;
            hit.transform.position = transform.position;
            hit.Play();
            oponent.GetComponent<VsAgent>().EndEpisode();
            EndEpisode();
        }
        if (other.gameObject.CompareTag("afuera"))
        {
            AddReward(-1000f);
            print("afuera");
            EndEpisode();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        jumping = true;
        jumps = 1;
    }
}
