using Unity.VisualScripting;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 endPoint;
    [SerializeField] private int speed = 1;
    [SerializeField] private bool activated = true;
    [SerializeField] private float moveDelay = 0;
    [SerializeField] private bool stopAtStart = false;
    [SerializeField] private bool stopAtEnd = false;

    private Vector3 startPoint;
    private bool isForward = true;
    private float currMoveDelay;

    void Start()
    {
        startPoint = transform.position;
        currMoveDelay = moveDelay;
    }

    // Update is called once per frame
    void Update()
    {
        var step = speed * Time.deltaTime;

        if (stopAtStart)
        {
            if (Vector3.Distance(transform.position, startPoint) < 0.001f && !isForward)
            {
                currMoveDelay -= Time.deltaTime;
                activated = false;
            }
            else if (Vector3.Distance(transform.position, endPoint) < 0.001f && isForward)
            {
                isForward = !isForward;
            }
            else
            {
                currMoveDelay = moveDelay;
            }
        }
        else if (stopAtEnd)
        {
            if (Vector3.Distance(transform.position, startPoint) < 0.001f && !isForward)
            {
                isForward = !isForward;
            }
            else if (Vector3.Distance(transform.position, endPoint) < 0.001f && isForward)
            {
                currMoveDelay -= Time.deltaTime;
                activated = false;
            }
            else
            {
                currMoveDelay = moveDelay;
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, startPoint) < 0.001f && !isForward)
            {
                currMoveDelay -= Time.deltaTime;
                activated = false;
            }
            else if (Vector3.Distance(transform.position, endPoint) < 0.001f && isForward)
            {
                currMoveDelay -= Time.deltaTime;
                activated = false;
            }
            else
            {
                currMoveDelay = moveDelay;
            }
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
            transform.position = Vector3.MoveTowards(transform.position, endPoint, step);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startPoint, step);
        }
    }
}
