using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody rigidBody;

    public float walkingSpeed = 5f;
    public float runningSpeed = 10f;
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
        if (isLocalPlayer)
        {
            // float h = Input.GetAxis("Horizontal");
            // float v = Input.GetAxis("Vertical");

            // Vector3 playerMovement = new Vector3(h * 0.25f, v * 0.25f, 0);

            // transform.position = transform.position + playerMovement;

            if (Input.GetKeyDown(KeyCode.Space) && rigidBody.linearVelocity.y == 0)
            {
                rigidBody.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
            }

            LayerMask layerMask = LayerMask.GetMask("Player");

            RaycastHit hit;
            GameObject otherPlayer;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 3, layerMask))
            {
                otherPlayer = hit.transform.gameObject;
                if (Input.GetKey(KeyCode.E))
                {
                    print("pushing");
                    otherPlayer.GetComponent<Rigidbody>().AddForce(transform.forward * force, ForceMode.Impulse);
                }
            }

            inputX = Input.GetAxis("Horizontal");
            inputY = Input.GetAxis("Vertical");
        }

        // LayerMask layerMask = LayerMask.GetMask("Player");

        // RaycastHit hit;
        // GameObject otherPlayer;
        // if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 3, layerMask))
        // {
        //     otherPlayer = hit.transform.gameObject;
        //     if (Input.GetKeyDown(KeyCode.E))
        //     {
        //         print("pushing");
        //         // otherPlayer.GetComponent<RigidbodySynchronizable>().AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        //         // otherPlayer.GetComponent<RigidbodySynchronizable>().AddForce(Vector3.forward * force, ForceMode.Impulse);
        //         Alteruna.Avatar otherAvatar = otherPlayer.GetComponent<Alteruna.Avatar>();
        //         User user = otherAvatar.Multiplayer.GetUser((ushort)otherAvatar.Owner);

        //         if (user != null)
        //         {
        //             ProcedureParameters parameters = new ProcedureParameters();
        //             parameters.Set("f", force);
        //             parameters.Set("dirX", transform.forward.x);
        //             parameters.Set("dirY", transform.forward.y);
        //             parameters.Set("dirZ", transform.forward.z);

        //             Multiplayer.Instance.InvokeRemoteProcedure("RpcPush", user.Index, parameters);
        //         }
        //     }
        // }
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
}