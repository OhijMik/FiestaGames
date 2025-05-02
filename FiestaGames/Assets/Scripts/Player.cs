using System;
using System.Collections;
using System.Collections.Generic;
using Alteruna;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("Base setup")]
    public float walkingSpeed = 5f;
    public float runningSpeed = 10f;
    public float jumpForce = 10.0f;
    public float rotationSpeed = 200.0f;
    public float force = 2f;

    [SerializeField] private float cameraYOffset = 0.4f;
    private Camera playerCamera;

    private RigidbodySynchronizable _rigid;
    private Alteruna.Avatar _avatar;

    void Awake()
    {
        _avatar = GetComponent<Alteruna.Avatar>();
        _rigid = GetComponent<RigidbodySynchronizable>();
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
        if (!_avatar.IsMe)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && _rigid.velocity.y == 0)
        {
            _rigid.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
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
                otherPlayer.GetComponent<RigidbodySynchronizable>().AddForce(transform.forward * force, ForceMode.Impulse);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!_avatar.IsMe)
        {
            return;
        }

        // Capture horizontal input
        float horizontalInput = Input.GetAxis("Horizontal");

        // Rotate the character
        transform.Rotate(0, horizontalInput * rotationSpeed * Time.deltaTime, 0);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 movementVector = transform.forward * Input.GetAxis("Vertical");
        movementVector.y = _rigid.velocity.y;

        if (isRunning)
        {
            movementVector.x *= runningSpeed;
            movementVector.z *= runningSpeed;
            _rigid.velocity = movementVector;
        }
        else
        {
            movementVector.x *= walkingSpeed;
            movementVector.z *= walkingSpeed;
            _rigid.velocity = movementVector;
        }

        _rigid.AddForce(Vector3.down * 9.8f, ForceMode.Acceleration);

        if (transform.position.y < -50)
        {
            transform.position = new Vector3(0, 1, 0);
        }
    }
}