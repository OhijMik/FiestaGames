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

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();


        // characterController = GetComponent<CharacterController>();
        // playerCamera = Camera.main;
        // playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
        // playerCamera.transform.SetParent(transform);
        // Lock cursor
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

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
                NetworkIdentity identity = hit.transform.GetComponent<NetworkIdentity>();
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
            // Rotate the character
            transform.Rotate(0, inputX * rotationSpeed * Time.deltaTime, 0);

            bool isRunning = Input.GetKey(KeyCode.LeftShift);

            Vector3 movementVector = transform.forward * inputY;

            if (isRunning)
            {
                movementVector.x *= runningSpeed * Time.deltaTime;
                movementVector.z *= runningSpeed * Time.deltaTime;
                rigidBody.MovePosition(rigidBody.position + movementVector);
            }
            else
            {
                movementVector.x *= walkingSpeed * Time.deltaTime;
                movementVector.z *= walkingSpeed * Time.deltaTime;
                rigidBody.MovePosition(rigidBody.position + movementVector);
            }

            if (transform.position.y < -50)
            {
                transform.position = new Vector3(0, 1, 0);
            }
        }


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



    [Command(requiresAuthority = false)]
    void CmdPushPlayer(NetworkIdentity targetNetId, Vector3 dir)
    {
        GameObject target = targetNetId.gameObject;
        NetworkIdentity netId = target.GetComponent<NetworkIdentity>();

        Debug.Log($"[Server] Command received to push {target.name}");

        if (netId.connectionToClient != null)
        {
            TargetApplyPush(netId.connectionToClient, dir.normalized * force);
        }
        else
        {
            Debug.LogWarning("[Server] No connectionToClient found on target!");
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