using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class ShootingAgent : Agent
{
    public int score = 0;
    public float speed = 3f;
    public float rotationSpeed = 3f;
    public float jumpforce=3f;
    public bool jumping = false;
    
    public Transform shootingPoint;
    public int minStepsBetweenShots = 50;
    public int damage = 100;
    public int original_bullets_count;
    public int bullets_count=5;
    public bool reloading=false;
    public float reload_steps;
    public int steps_reloading;

    public Projectile projectile;
    public EnemyManager enemyManager;
    
    private bool ShotAvaliable = true;
    private int StepsUntilShotIsAvaliable = 0;
    
    private Vector3 StartingPosition;
    private Rigidbody Rb;
    private EnvironmentParameters EnvironmentParameters;

    public event Action OnEnvironmentReset;
    
    private void Shoot()
    {
        if (!ShotAvaliable&&reloading)
            return;
        
        var layerMask = 1 << LayerMask.NameToLayer("Enemy");
        var direction = transform.forward;

        var spawnedProjectile = Instantiate(projectile, shootingPoint.position, Quaternion.Euler(0f, -90f, 0f));
        spawnedProjectile.SetDirection(direction);
        
        Debug.DrawRay(transform.position, direction, Color.blue, 1f);
        
        if (Physics.Raycast(shootingPoint.position, direction, out var hit, 200f, layerMask))
        {
            hit.transform.GetComponent<Enemy>().GetShot(damage, this);
        }
        else
        {
            AddReward(-0.033f);
        }

        if (transform.localPosition.y <= -3) {
            enemyManager.SetEnemiesActive();
            AddReward(-1f);
            EndEpisode();
        }

        bullets_count--;

        if (bullets_count <= 0) {
            reloading = true;
        }

        ShotAvaliable = false;
        StepsUntilShotIsAvaliable = minStepsBetweenShots;
        steps_reloading = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(ShotAvaliable);
        sensor.AddObservation(transform.rotation.y);
        sensor.AddObservation(bullets_count);
    }

    private void FixedUpdate()
    {
        if (!ShotAvaliable)
        {
            StepsUntilShotIsAvaliable--;

            if (StepsUntilShotIsAvaliable <= 0) {
                ShotAvaliable = true;
            }

            steps_reloading++;
            if (steps_reloading == reload_steps) {
                steps_reloading = 0;
                reloading = false;
                bullets_count = original_bullets_count;
            }

        }
        
        AddReward(-1f/MaxStep);
    }
    public override void OnActionReceived(float[] vectorAction)
    {
        if (Mathf.RoundToInt(vectorAction[0]) >= 1)
        {
            Shoot();
        }

        if (jumping) {
            vectorAction[4] = 0;
            Rb.AddForce(new Vector3(0, -3.8f, 0), ForceMode.VelocityChange);
        }

        Rb.velocity = new Vector3(vectorAction[2] * speed, 0, vectorAction[1] * speed);
        transform.Rotate(Vector3.up, vectorAction[3] * rotationSpeed);


        if (vectorAction[4] == 1)
        {
            print("jump");
            jumping = true;
            Rb.AddForce(new Vector3(0, jumpforce, 0),ForceMode.VelocityChange);
        }
    }
    
    public override void Initialize()
    {
        StartingPosition = transform.position;
        Rb = GetComponent<Rigidbody>();
        bullets_count = original_bullets_count;
        //TODO: Delete
        Rb.freezeRotation = true;
        EnvironmentParameters = Academy.Instance.EnvironmentParameters;
    }
    
    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetMouseButtonDown(0) ? 1f : 0f;
        actionsOut[1] = Input.GetAxis("Horizontal");
        actionsOut[2] = -Input.GetAxis("Vertical");
        actionsOut[3] = Input.GetAxis("Rotate");
        actionsOut[4] = Input.GetKeyDown(KeyCode.Space) ? 1f : 0f;
    }

    public override void OnEpisodeBegin()
    {
        OnEnvironmentReset?.Invoke();

        //Load Parameter from Curciulum
        minStepsBetweenShots = Mathf.FloorToInt(EnvironmentParameters.GetWithDefault("shootingFrequenzy", 30f));
        bullets_count = original_bullets_count;
        transform.position = StartingPosition;
        Rb.velocity = Vector3.zero;
        ShotAvaliable = true;
    }

    public void RegisterKill()
    {
        score++;
        AddReward(1.0f / EnvironmentParameters.GetWithDefault("amountZombies", 4f));
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("enemy"))
        {
            enemyManager.SetEnemiesActive();
            AddReward(-1f);
            EndEpisode();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        jumping = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        jumping = true;
    }
}
