using UnityEngine;

public class ArrowMovement : MonoBehaviour
{
    [SerializeField] private float amplitude = 1.0f;
    [SerializeField] private float speed = 2.0f;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        float newY = initialPosition.y + amplitude * Mathf.Sin(speed * Time.time);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}