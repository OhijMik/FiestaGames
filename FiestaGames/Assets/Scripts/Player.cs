using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 12.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 90.0f;
    public float rotationSpeed = 200.0f;
    public float force = 2f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;

    [HideInInspector] public bool canMove = true;

    [SerializeField] private float cameraYOffset = 0.4f;
    private Camera playerCamera;

    [SerializeField] private GameObject playerBody;

    private Alteruna.Avatar _avatar;

    void Start()
    {
        _avatar = GetComponent<Alteruna.Avatar>();

        if (!_avatar.IsMe)
            return;

        characterController = GetComponent<CharacterController>();
        playerCamera = Camera.main;
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
        playerCamera.transform.SetParent(transform);
        // Lock cursor
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    void Update()
    {
        if (!_avatar.IsMe)
        {
            return;
        }

        movement();

        LayerMask layerMask = LayerMask.GetMask("Player");

        RaycastHit hit;
        GameObject otherPlayer;
        if (Physics.Raycast(transform.position, playerBody.transform.TransformDirection(Vector3.forward), out hit, 3, layerMask))
        {
            otherPlayer = hit.transform.gameObject;
            if (Input.GetKey(KeyCode.E))
            {
                print("pushing");
                // otherPlayer.GetComponent<CharacterController>().Move(playerBody.transform.forward * Time.deltaTime * 50);
                otherPlayer.GetComponent<Rigidbody>().AddForce(playerBody.transform.forward * force, ForceMode.Impulse);
            }
        }

    }
    void movement()
    {
        bool isRunning = false;

        // Press Left Shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);

        float movementDirectionY = moveDirection.y;
        if (canMove)
        {
            if (isRunning)
            {
                moveDirection = playerBody.transform.forward * runningSpeed * Input.GetAxis("Vertical");
            }
            else
            {
                moveDirection = playerBody.transform.forward * walkingSpeed * Input.GetAxis("Vertical");
            }
        }


        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Capture horizontal input
        float horizontalInput = Input.GetAxis("Horizontal");

        // Rotate the character
        playerBody.transform.Rotate(0, horizontalInput * rotationSpeed * Time.deltaTime, 0);

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);


        if (transform.position.y < -50)
        {
            transform.position = new Vector3(0, 1, 0);
        }
    }
}