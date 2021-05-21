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
    public int score = 0;
    public float speed = 3f;
    public float rotationSpeed = 3f;
    public float initRotationSpeed;
    public float jumpforce = 3f;
    public int jumps = 0;
    public bool jumping = false;
    public Text recompensa_text;
    public Text score_text;

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
    public bool empty_mun = false;

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

    private void Shoot()
    {
        if (!ShotAvaliable || reloading || empty_mun)
            return;

        var layerMask = 1 << LayerMask.NameToLayer("Enemy");
        var direction = transform.forward;

        var spawnedProjectile = Instantiate(projectile, shootingPoint.position, Quaternion.Euler(0f, -90f, 0f));
        spawnedProjectile.SetDirection(direction);

        bullets_count--;

        ShotAvaliable = false;
        StepsUntilShotIsAvaliable = minStepsBetweenShots;

        if (Physics.Raycast(shootingPoint.position, direction, out var hit, 100f))
        {
            if (hit.transform.CompareTag("enemy")) {
                hit.transform.GetComponent<Enemy>().GetShot(damage, this);
                AddReward(1f);
            }
            else
            {
                AddReward(-2f); 
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
        //sensor.AddObservation(Vector3.Distance(this.transform.localPosition,this.mun.transform.localPosition));
        sensor.AddObservation(Vector3.Distance(this.transform.localPosition,this.recargador.transform.localPosition));
    }

    private void FixedUpdate()
    {
        /*if (Vector3.Distance(this.transform.localPosition, this.mun.transform.localPosition) < 2f) {
            AddReward(0.001f);
        }*/

        if (Physics.Raycast(shootingPoint.position, transform.forward, out var hit, 50f))
        {

            if (hit.transform.CompareTag("enemy"))
            {
                //Debug.DrawRay(shootingPoint.position, transform.forward * 5, Color.green,0.3f);
                AddReward(30f / MaxStep);
                rotationSpeed = 0;
            }
            else {
                rotationSpeed = initRotationSpeed;
                if (Physics.Raycast(shootingPoint.position, transform.forward + (Vector3.left / 8), out var hit1, 50f))
                {
                    if (hit1.transform.CompareTag("enemy"))
                    {
                        //Debug.DrawRay(shootingPoint.position, (transform.forward + (Vector3.left / 8)) * 5, Color.blue,0.3f);
                        AddReward(10f / MaxStep);
                    }
                    else
                    {
                        if (Physics.Raycast(shootingPoint.position, transform.forward + (Vector3.right / 8), out var hit2, 50f))
                        {

                            if (hit2.transform.CompareTag("enemy"))
                            {
                                //Debug.DrawRay(shootingPoint.position, (transform.forward + (Vector3.right / 8)) * 5, Color.red,0.3f);
                                AddReward(10f / MaxStep);
                            }
                        }
                    }
                }
            }
        }

        AddReward(-1f / MaxStep);

        if (mun_count <= 0)
        {
            if (reloading) {
                reloading = false;
            }
            empty_mun = true;
            //AddReward(-0.001f);
        }

        recompensa_text.text = GetCumulativeReward().ToString();
        score_text.text = score.ToString();
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

        if (jumping)
        {
            Rb.AddForce(new Vector3(0, -9.8f, 0), ForceMode.VelocityChange);
        }

        if (transform.localPosition.y <= -3)
        {
            enemyManager.SetEnemiesActive();
            AddReward(-1f);
            EndEpisode();
        }
        if (transform.localPosition.y > 5)
        { 
            print("muy alto");
            enemyManager.SetEnemiesActive();
            AddReward(-3f);
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
        if (Mathf.RoundToInt(vectorAction[4]) >= 1 && !jumping && jumps<1)
        {
            //print("jump");
            jumps++;
            jumping = true;
            Rb.AddForce(new Vector3(0, jumpforce, 0), ForceMode.VelocityChange);
            //AddReward(0.005f);
        }

        if (Mathf.RoundToInt(vectorAction[0]) >= 1)
        {
            Shoot();
        }

        /*if (Mathf.RoundToInt(vectorAction[5]) >= 1)
        {
            reloading = true;
        }*/


        Rb.velocity = new Vector3(vectorAction[2] * speed, 0, vectorAction[1] * speed);
        transform.Rotate(Vector3.up, vectorAction[3] * rotationSpeed);
    }

    public override void Initialize()
    {
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
        actionsOut[0] = Input.GetKeyDown(KeyCode.P) ? 1f : 0f;
        actionsOut[1] = Input.GetAxis("Horizontal");
        actionsOut[2] = -Input.GetAxis("Vertical");
        actionsOut[3] = Input.GetAxis("Rotate");
        actionsOut[4] = Input.GetKeyDown(KeyCode.Space) ? 1f : 0f;
        //actionsOut[5] = Input.GetKeyDown(KeyCode.R) ? 1f : 0f;
    }

    public override void OnEpisodeBegin()
    {
        OnEnvironmentReset?.Invoke();
        score = 0;
        //Load Parameter from Curciulum
        minStepsBetweenShots = Mathf.FloorToInt(EnvironmentParameters.GetWithDefault("shootingFrequenzy", 30f));
        bullets_count = original_bullets_count;
        mun_count = original_mun_count;
        empty_mun = false;
        steps_reloading = 0;
        transform.position = StartingPosition;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        Rb.velocity = Vector3.zero;
        ShotAvaliable = true;
        //mun.transform.localPosition = new Vector3(UnityEngine.Random.Range(-6f, 6f), 1, UnityEngine.Random.Range(-6f, 6f));
        rotationSpeed = initRotationSpeed; 
    }

    public void RegisterKill()
    {
        score++;
        AddReward(80.0f / EnvironmentParameters.GetWithDefault("amountZombies", 5f));
        print("kill");
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("enemy"))
        {
            enemyManager.SetEnemiesActive();
            AddReward(-80f);
            EndEpisode();
        }
        if (other.gameObject.CompareTag("mun")) {
            print("municion");
            AddReward(100f);
            if (bullets_count <= 0) {
                reloading = true;
            }
            empty_mun = false; 
            mun_count++;
            other.gameObject.transform.localPosition = new Vector3(UnityEngine.Random.Range(-6f,6f), 1, UnityEngine.Random.Range(-6f, 6f));
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        jumping = false;
        if (collision.gameObject.CompareTag("ground"))
        {
            jumps = 0;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        jumping = true;
        jumps = 1;
    }
}
