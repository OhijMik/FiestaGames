using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    private Rigidbody rigidBody;

    public float walkingSpeed = 7f;
    public float runningSpeed = 10f;
    public float jumpForce = 10.0f;
    public float soloJumpForce = 15.0f;
    public float rotationSpeed = 200.0f;
    public float force = 20f;

    private float inputX;
    private float inputY;

    public float pushCooldown = 3;
    private float pushCurrCooldown = 0;
    private float playerPushRange = 2;

    private float playerPullRange = 2;

    private float maxPlayerDist = 15;
    public float playerTpCooldown = 5;
    private float playerTpCurrCooldown = 5;

    GameObject[] players;

    bool jump = false;


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

        if (Input.GetKeyDown(KeyCode.B))
        {
            jump = true;
        }

        float currJumpForce = jumpForce;

        if (players.Length == 1)
        {
            currJumpForce = soloJumpForce;
        }

        if (Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(rigidBody.linearVelocity.y) <= 0.05)
        {
            rigidBody.AddForce(currJumpForce * Vector3.up, ForceMode.Impulse);
        }
        if (jump && Mathf.Abs(rigidBody.linearVelocity.y) <= 0.01)
        {
            rigidBody.AddForce(currJumpForce * Vector3.up, ForceMode.Impulse);
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
            if (currentPhysicsScene.Raycast(ray.origin, ray.direction, out hit, playerPushRange, LayerMask.GetMask("Player")))
            {
                if (Input.GetKey(KeyCode.Mouse0) && pushCurrCooldown == 0)
                {
                    CmdRequestPush(transform.position, transform.forward);
                    pushCurrCooldown = pushCooldown;
                }
            }

            if (currentPhysicsScene.Raycast(ray.origin, ray.direction, out hit, playerPullRange, LayerMask.GetMask("Player")))
            {
                if (Input.GetKey(KeyCode.Mouse1))
                {
                    CmdRequestPull(transform.position, transform.forward);
                }
            }
        }
        else
        {
            // On clients: use regular raycast
            Vector3 direction = transform.TransformDirection(Vector3.forward);
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, playerPushRange, LayerMask.GetMask("Player")))
            {
                if (Input.GetKey(KeyCode.Mouse0) && pushCurrCooldown == 0)
                {
                    CmdRequestPush(transform.position, transform.forward);
                    pushCurrCooldown = pushCooldown;
                }
            }

            if (Physics.Raycast(transform.position, direction, out hit, playerPullRange, LayerMask.GetMask("Player")))
            {
                if (Input.GetKey(KeyCode.Mouse1))
                {
                    CmdRequestPull(transform.position, transform.forward);
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

            GameObject furthestPlayer = FindFurthestPlayer();
            if (furthestPlayer != null && Vector3.Distance(transform.position + movementVector, furthestPlayer.transform.position) > maxPlayerDist)
            {
                playerTpCurrCooldown -= Time.deltaTime;
            }
            else
            {
                playerTpCurrCooldown = playerTpCooldown;
            }

            Vector3 spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;

            if (playerTpCurrCooldown <= 0)
            {
                foreach (GameObject player in players)
                {
                    player.transform.position = spawnPoint;
                }
            }
            foreach (GameObject player in players)
            {
                if (player.transform.position.y < -30)
                {
                    foreach (GameObject p in players)
                    {
                        p.transform.position = spawnPoint;
                    }
                    break;
                }
            }
        }
    }

    private GameObject FindFurthestPlayer()
    {
        GameObject furthestPlayer = null;
        float maxDistance = float.MinValue;

        foreach (GameObject player in players)
        {
            if (player != gameObject)
            {
                float distance = Vector3.Distance(player.transform.position, transform.position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    furthestPlayer = player;
                }
            }
        }
        return furthestPlayer;
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

        if (serverScene.Raycast(origin, direction, out RaycastHit hit, playerPushRange, LayerMask.GetMask("Player")))
        {
            NetworkIdentity netId = hit.collider.GetComponent<NetworkIdentity>();
            if (netId != null)
            {
                GameObject target = netId.gameObject;
                Player targetMovement = target.GetComponent<Player>();

                // Get the correct connection
                NetworkConnectionToClient conn = netId.connectionToClient;

                if (conn != null)
                {
                    targetMovement.TargetApplyPush(conn, (transform.forward + Vector3.up * 0.5f).normalized * force);
                }
                else
                {
                    Debug.Log("[Server] No connectionToClient — target is probably the host. Call directly.");
                    if (targetMovement.isLocalPlayer)
                    {
                        targetMovement.ApplyPushDirectly((transform.forward + Vector3.up * 0.5f).normalized * force);
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


    [Command]
    void CmdRequestPull(Vector3 origin, Vector3 direction)
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

        if (serverScene.Raycast(origin, direction, out RaycastHit hit, playerPullRange, LayerMask.GetMask("Player")))
        {
            NetworkIdentity netId = hit.collider.GetComponent<NetworkIdentity>();
            if (netId != null)
            {
                GameObject target = netId.gameObject;
                Player targetMovement = target.GetComponent<Player>();

                // Get the correct connection
                NetworkConnectionToClient conn = netId.connectionToClient;

                if (conn != null)
                {
                    targetMovement.TargetApplyPull(conn, transform.forward * force);
                }
                else
                {
                    Debug.Log("[Server] No connectionToClient — target is probably the host. Call directly.");
                    if (targetMovement.isLocalPlayer)
                    {
                        targetMovement.ApplyPullDirectly(transform.forward * force);
                    }
                }
            }
        }
    }

    [TargetRpc]
    public void TargetApplyPull(NetworkConnection target, Vector3 force)
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

    public void ApplyPullDirectly(Vector3 force)
    {
        Debug.Log("[Host] Applying push directly");
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }

    public float getPlayerTpCurrCooldown()
    {
        return playerTpCurrCooldown;
    }

}