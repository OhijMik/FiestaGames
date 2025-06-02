using UnityEngine;
using Mirror;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody rigidBody;

    public float walkingSpeed = 7f;
    public float runningSpeed = 12f;
    public float jumpForce = 10.0f;
    public float rotationSpeed = 200.0f;
    public float force = 20f;

    private float inputX;
    private float inputY;

    public float pushCooldown = 3;
    private float pushCurrCooldown = 0;

    private float maxPlayerDist = 5;
    private float playerTpCooldown = 3;
    private float playerTpCurrCooldown = 3;

    [SerializeField] Vector3 spawnPoint = new Vector3(0, 1, 0);

    GameObject[] players;


    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        players = GameObject.FindGameObjectsWithTag("Player");

        if (pushCurrCooldown > 0)
        {
            pushCurrCooldown -= Time.deltaTime;
        }
        else
        {
            pushCurrCooldown = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(rigidBody.linearVelocity.y) <= 0.05)
        {
            rigidBody.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        }

        if (NetworkServer.active)
        {
            // Get the PhysicsScene of this GameObject's scene
            PhysicsScene currentPhysicsScene = gameObject.scene.GetPhysicsScene();

            // Build ray manually
            Vector3 origin = transform.position;
            Vector3 direction = transform.TransformDirection(Vector3.forward);

            Ray ray = new Ray(origin, direction);
            RaycastHit hit;

            // Use the custom physics scene to perform the raycast
            if (currentPhysicsScene.Raycast(ray.origin, ray.direction, out hit, 3f, LayerMask.GetMask("Player")))
            {
                NetworkIdentity identity = hit.transform.GetComponent<NetworkIdentity>();
                if (Input.GetKey(KeyCode.E) && pushCurrCooldown == 0)
                {
                    // direction = transform.forward + Vector3.up * 0.05f;
                    CmdRequestPush(transform.position, transform.forward);
                    pushCurrCooldown = pushCooldown;
                }
            }
        }
        else
        {
            // On clients: use regular raycast
            Vector3 direction = transform.TransformDirection(Vector3.forward);
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 3f, LayerMask.GetMask("Player")))
            {
                if (Input.GetKey(KeyCode.E) && pushCurrCooldown == 0)
                {
                    // direction = transform.forward + Vector3.up * 0.05f;
                    CmdRequestPush(transform.position, transform.forward);
                    pushCurrCooldown = pushCooldown;
                }
            }
        }

        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer == true)
        {
            players = GameObject.FindGameObjectsWithTag("Player");

            // Rotate the character
            transform.Rotate(0, inputX * rotationSpeed * Time.deltaTime, 0);

            bool isRunning = Input.GetKey(KeyCode.LeftShift);

            Vector3 movementVector = transform.forward * inputY;

            if (isRunning)
            {
                movementVector.x *= runningSpeed * Time.deltaTime;
                movementVector.z *= runningSpeed * Time.deltaTime;
            }
            else
            {
                movementVector.x *= walkingSpeed * Time.deltaTime;
                movementVector.z *= walkingSpeed * Time.deltaTime;
            }
            rigidBody.MovePosition(rigidBody.position + movementVector);

            GameObject nearestPlayer = FindNearestPlayer();
            if (nearestPlayer != null && Vector3.Distance(transform.position + movementVector, nearestPlayer.transform.position) > maxPlayerDist)
            {
                playerTpCurrCooldown -= Time.deltaTime;
            }
            else
            {
                playerTpCurrCooldown = playerTpCooldown;
            }

            if (transform.position.y < -50 || playerTpCurrCooldown <= 0)
            {
                foreach (GameObject player in players)
                {
                    player.transform.position = spawnPoint;
                }
            }
        }
    }

    private GameObject FindNearestPlayer()
    {
        GameObject nearestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (GameObject player in players)
        {
            if (player != gameObject)
            {
                float distance = Vector3.Distance(player.transform.position, transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPlayer = player;
                }
            }
        }
        return nearestPlayer;
    }

    [Command]
    void CmdRequestPush(Vector3 origin, Vector3 direction)
    {
        PhysicsSim sim = FindObjectOfType<PhysicsSim>();
        if (sim == null)
        {
            Debug.LogError("No PhysicsSim found on server!");
            return;
        }

        PhysicsScene serverScene = sim.GetScene();

        if (!serverScene.IsValid())
        {
            Debug.LogError("Invalid server physics scene");
            return;
        }

        if (serverScene.Raycast(origin, direction, out RaycastHit hit, 3f, LayerMask.GetMask("Player")))
        {
            NetworkIdentity netId = hit.collider.GetComponent<NetworkIdentity>();
            if (netId != null)
            {
                GameObject target = netId.gameObject;
                PlayerMovement targetMovement = target.GetComponent<PlayerMovement>();

                // Get the correct connection
                NetworkConnectionToClient conn = netId.connectionToClient;

                if (conn != null)
                {
                    targetMovement.TargetApplyPush(conn, direction.normalized * force);
                }
                else
                {
                    Debug.Log("[Server] No connectionToClient — target is probably the host. Call directly.");
                    if (targetMovement.isLocalPlayer)
                    {
                        targetMovement.ApplyPushDirectly(direction.normalized * force);
                    }
                }
            }
        }
    }

    [TargetRpc]
    public void TargetApplyPush(NetworkConnection target, Vector3 force)
    {
        // This runs only on the target client, including host
        if (!isLocalPlayer)
        {
            Debug.Log("[Client] Skipping push: not local player");
            return;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log("[Client] Applying push from server");
            rb.AddForce(force, ForceMode.Impulse);
        }
    }

    public void ApplyPushDirectly(Vector3 force)
    {
        Debug.Log("[Host] Applying push directly");
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }

}