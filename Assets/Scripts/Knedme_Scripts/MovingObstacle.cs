using UnityEngine;

// Простой скрипт, чтобы сделать препятствие движущимся
public class MovingObstacle : MonoBehaviour
{
    [Header("Параметры движения")]
    public Vector3 moveDirection = Vector3.left;
    public float speed = 2f;
    public float distance = 3f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + 
            moveDirection * Mathf.Sin(Time.time * speed) * distance;
    }
}
