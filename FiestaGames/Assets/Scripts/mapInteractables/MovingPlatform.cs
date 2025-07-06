using Unity.VisualScripting;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 endpoint;
    [SerializeField] private int speed = 1;
    [SerializeField] private bool activated = true;
    [SerializeField] private float moveDelay = 0;

    private Vector3 startpoint;
    private bool isForward = true;
    private float currMoveDelay;

    void Start()
    {
        startpoint = transform.position;
        currMoveDelay = moveDelay;
    }

    // Update is called once per frame
    void Update()
    {
        var step = speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, startpoint) < 0.001f && !isForward)
        {
            currMoveDelay -= Time.deltaTime;
            activated = false;
        }
        else if (Vector3.Distance(transform.position, endpoint) < 0.001f && isForward)
        {
            currMoveDelay -= Time.deltaTime;
            activated = false;
        }
        else
        {
            currMoveDelay = moveDelay;
            activated = true;
        }

        if (currMoveDelay < 0)
        {
            isForward = !isForward;
            activated = true;
        }

        if (!activated)
        {
            return;
        }

        if (isForward)
        {
            transform.position = Vector3.MoveTowards(transform.position, endpoint, step);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startpoint, step);
        }
    }
}
