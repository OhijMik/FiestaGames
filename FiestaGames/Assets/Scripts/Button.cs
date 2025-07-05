using Unity.VisualScripting;
using UnityEngine;

public class Button : MonoBehaviour
{
    [SerializeField] Material buttonOnMat;
    [SerializeField] Material buttonOffMat;
    MeshRenderer meshRenderer;
    private bool isOn = false;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = buttonOffMat;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isOn)
        {
            isOn = true;
            meshRenderer.material = buttonOnMat;
        }
    }
}
