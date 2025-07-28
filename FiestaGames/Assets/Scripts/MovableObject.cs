using UnityEngine;
using Mirror;


public class MovableObject : NetworkBehaviour
{
    [TargetRpc]
    public void TargetApplyPush(NetworkConnection target, Vector3 pushForce)
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
            rb.AddForce(pushForce, ForceMode.Impulse);
        }
    }

    public void ApplyPushDirectly(Vector3 pushForce)
    {
        Debug.Log("[Host] Applying push directly");
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(pushForce, ForceMode.Impulse);
        }
    }

    [TargetRpc]
    public void TargetApplyPull(NetworkConnection target, Vector3 pullPos)
    {
        // This runs only on the target client, including host
        if (!isLocalPlayer)
        {
            Debug.Log("[Client] Skipping pull: not local player");
            return;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log("[Client] Applying pull from server");
            rb.transform.position = pullPos;
        }
    }

    public void ApplyPullDirectly(Vector3 pullPos)
    {
        Debug.Log("[Host] Applying pull directly");
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.transform.position = pullPos;
        }
    }
}
