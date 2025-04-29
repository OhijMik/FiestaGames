using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 90.0f;
    public float rotationSpeed = 200.0f;

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
            return;

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


        // Player and Camera rotation
        // if (canMove && playerCamera != null)
        // {
        //     rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        //     rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        //     playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        //     transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        // }
    }
}