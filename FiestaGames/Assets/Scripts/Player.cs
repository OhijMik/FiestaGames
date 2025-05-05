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

        if (Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(rigidBody.linearVelocity.y) <= 0.05)
        {
            rigidBody.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        }

        // LayerMask layerMask = LayerMask.GetMask("Player");
        // Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 3f, Color.red, 1f);

        // RaycastHit hit;
        // if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 3, layerMask))
        // {
        //     NetworkIdentity identity = hit.transform.GetComponent<NetworkIdentity>();
        //     print("pushing1");
        //     if (Input.GetKey(KeyCode.E))
        //     {
        //         print("pushing2");
        //         CmdPushPlayer(identity);
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
                if (Input.GetKey(KeyCode.E))
                {
                    CmdPushPlayer(identity);
                }
            }
        }
        else
        {
            // On clients: use regular raycast
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, 3f, LayerMask.GetMask("Player")))
            {
                NetworkIdentity identity = hit.transform.GetComponent<NetworkIdentity>();
                if (Input.GetKey(KeyCode.E))
                {
                    CmdPushPlayer(identity);
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
            movementVector.y = rigidBody.linearVelocity.y;

            if (isRunning)
            {
                movementVector.x *= runningSpeed;
                movementVector.z *= runningSpeed;
                rigidBody.linearVelocity = movementVector;
            }
            else
            {
                movementVector.x *= walkingSpeed;
                movementVector.z *= walkingSpeed;
                rigidBody.linearVelocity = movementVector;
            }

            rigidBody.AddForce(Vector3.down * 9.8f, ForceMode.Acceleration);

            if (transform.position.y < -50)
            {
                transform.position = new Vector3(0, 1, 0);
            }
        }


    }

    [Command]
    void CmdPushPlayer(NetworkIdentity targetNetId)
    {
        GameObject target = targetNetId.gameObject;
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            print("pushing");
            Scene pusherScene = gameObject.scene;
            SceneManager.MoveGameObjectToScene(target, pusherScene);
            // Vector3 pushDir = (transform.TransformDirection(Vector3.forward) + Vector3.up * 0.01f).normalized;
            targetRb.AddForce(transform.TransformDirection(Vector3.forward) * force, ForceMode.Impulse);
        }
    }
}