using UnityEngine;
using Mirror;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine.SceneManagement;

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

        if (pushCooldown > 0)
        {
            pushCooldown -= Time.deltaTime;
        }
        else
        {
            pushCooldown = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(rigidBody.linearVelocity.y) <= 0.05)
        {
            rigidBody.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        }

        // LayerMask layerMask = LayerMask.GetMask("Player");
        // Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 3f, Color.red, 1f);

        // RaycastHit hit;
        // Vector3 direction = transform.TransformDirection(Vector3.forward);
        // if (Physics.Raycast(transform.position, direction, out hit, 3))
        // {
        //     print("detected");
        //     NetworkIdentity identity = hit.transform.GetComponent<NetworkIdentity>();
        //     if (Input.GetKey(KeyCode.E) && pushCooldown == 0)
        //     {
        //         direction.y = 0.25f;
        //         CmdPushPlayer(identity, direction.normalized);
        //         pushCooldown = 3;
        //     }
        // }

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
                if (Input.GetKey(KeyCode.E) && pushCooldown == 0)
                {
                    print("server pushing");
                    // direction = transform.forward + Vector3.up * 0.05f;
                    CmdPushPlayer(identity, direction);
                    pushCooldown = 3;
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
                if (Input.GetKey(KeyCode.E) && pushCooldown == 0)
                {
                    print("client pushing");
                    // direction = transform.forward + Vector3.up * 0.05f;
                    CmdPushPlayer(identity, direction);
                    pushCooldown = 3;
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

            // if (pushedDelayCooldown == 0)
            // {
            //     // if (isRunning)
            //     // {
            //     //     movementVector.x *= runningSpeed;
            //     //     movementVector.z *= runningSpeed;
            //     //     rigidBody.linearVelocity = movementVector;
            //     // }
            //     // else
            //     // {
            //     //     movementVector.x *= walkingSpeed;
            //     //     movementVector.z *= walkingSpeed;
            //     //     rigidBody.linearVelocity = movementVector;
            //     // }

            //     movementVector.x *= walkingSpeed * Time.deltaTime;
            //     movementVector.z *= walkingSpeed * Time.deltaTime;
            //     rigidBody.MovePosition(rigidBody.position + movementVector);
            // }
            movementVector.x *= walkingSpeed * Time.deltaTime;
            movementVector.z *= walkingSpeed * Time.deltaTime;
            movementVector.y = rigidBody.linearVelocity.y * Time.deltaTime;
            rigidBody.MovePosition(rigidBody.position + movementVector);

            if (transform.position.y < -50)
            {
                transform.position = new Vector3(0, 1, 0);
            }
        }


    }

    [Command(requiresAuthority = false)]
    void CmdPushPlayer(NetworkIdentity targetNetId, Vector3 dir)
    {
        GameObject target = targetNetId.gameObject;
        Rigidbody targetRb = target.GetComponent<Rigidbody>();

        if (targetRb != null)
        {
            Debug.Log($"[Server] Target: {target.name}, Scene: {target.scene.name}");
            Debug.Log($"[Server] This Player: {name}, Scene: {gameObject.scene.name}");
            Debug.Log($"[Server] Direction: {dir}");

            // Ensure same scene
            SceneManager.MoveGameObjectToScene(target, gameObject.scene);

            // targetRb.linearVelocity = dir * force;
            targetRb.AddForce(dir.normalized * force, ForceMode.Impulse);
        }
    }
    // [Command(requiresAuthority = false)]
    // void CmdPushPlayer(NetworkIdentity targetNetId, Vector3 dir)
    // {
    //     TargetPushPlayer(targetNetId.connectionToClient, dir);
    // }

    // [TargetRpc]
    // void TargetPushPlayer(NetworkConnection target, Vector3 dir)
    // {
    //     print("pushing");
    //     GetComponent<Rigidbody>().AddForce(dir * force, ForceMode.Impulse);
    //     print(GetComponent<Rigidbody>().linearVelocity);
    // }

}