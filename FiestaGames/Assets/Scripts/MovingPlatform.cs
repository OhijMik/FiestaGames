using Unity.VisualScripting;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 startpoint;
    [SerializeField] private Vector3 endpoint;
    [SerializeField] private int speed = 1;
    [SerializeField] private bool activated = true;
    private bool isForward = true;

    void Start()
    {
        startpoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!activated)
        {
            return;
        }

        var step = speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, startpoint) < 0.001f)
        {
            isForward = true;
        }
        else if (Vector3.Distance(transform.position, endpoint) < 0.001f)
        {
            isForward = false;
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
