using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 startpoint;
    [SerializeField] private Vector3 endpoint;
    [SerializeField] private int speed = 1;
    [SerializeField] private bool activated = false;

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

        bool isMoving = true;

        if (Vector3.Distance(transform.position, endpoint) < 0.001f)
        {
            isMoving = false;
        }

        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, endpoint, step);
        }
    }

    public void activate()
    {
        activated = true;
    }
}
