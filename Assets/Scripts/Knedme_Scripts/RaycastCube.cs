using System;
using System.Linq;
using UnityEngine;

public class RaycastCube : MonoBehaviour
{
    // Параметры отображения лучей
    private const float RayDistance = 10f; // Длинна полёта лучей
    private const float RayWidth = 0.01f; // Диаметр отображаемого луча
    private const float RayEmitAngle = 15f; // Угол испускания лучей
    private const int RayCount = 8; // Количество испускаемых лучей
    void Start()
    {
        // Формируем массив направлений лучей
        Quaternion[] rayDirs = new Quaternion[RayCount];
        const float deltaRad = 2 * (float) Math.PI / RayCount;
        for (int i = 0; i < RayCount; i++)
        {
            rayDirs[i] = Quaternion.Euler(
                (float) Math.Cos(deltaRad * i) * RayEmitAngle,
                (float) Math.Sin(deltaRad * i) * RayEmitAngle,
                0
            );
        }

        // Перебираем все направления
        for (int i = 0; i < rayDirs.Length; i++)
        {
            // Создаём луч
            Ray ray = new Ray(transform.position, rayDirs[i] * transform.forward);
            RaycastHit hit;

            // Создаём дочерний объект
            GameObject child = new GameObject($"Ray{i}");
            child.transform.SetParent(transform);

            // Создаём отрисовщик луча для дочернего объекта
            LineRenderer lineRenderer = child.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = RayWidth;
            lineRenderer.endWidth   = RayWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, ray.origin);

            if (Physics.Raycast(ray, out hit, RayDistance))
            {
                lineRenderer.SetPosition(1, hit.point);
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor   = Color.green;
            }
            else
            {
                lineRenderer.SetPosition(1, ray.origin + ray.direction * RayDistance);
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor   = Color.red;
            }
        }
    }

    void Update() {}
}
