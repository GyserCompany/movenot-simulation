using System.Collections.Generic;
using UnityEngine;

public class RaycastCube : MonoBehaviour
{
    // Параметры отображения лучей
    private const float RayDistance = 10f; // Длинна полёта лучей
    private const float RayWidth = 0.01f; // Диаметр отображаемого луча
    private const float RayEmitAngle = 15f; // Максимальный угол испускания лучей
    private const float RayDeltaAngle = 1f; // Угол, на который постепенно увеличивается угол испускания
    private const int RayCount = 20; // Максимальное количество лучей в окружности

    void Start()
    {
        // Создадим массив направлений лучей
        List<Vector3> rayDirs = new List<Vector3>();
        Vector3 direction = transform.forward;
        for (float angle = RayEmitAngle; angle > 0.001f; angle -= RayDeltaAngle)
        {
            // Расчитываем количество лучей в окружности для определённого угла
            int countAtAngle = Mathf.Max(
                1,
                Mathf.RoundToInt(
                    RayCount * (Mathf.Tan(angle * Mathf.Deg2Rad) / Mathf.Tan(RayEmitAngle * Mathf.Deg2Rad))
                )
            );
 
            // Добавляем новые круговые направления
            rayDirs.AddRange(CircleDirs(angle, countAtAngle, direction));
        }

        DrawRays(rayDirs.ToArray());
    }

    // Возвращает массив круговых направлений для определённого угла
    Vector3[] CircleDirs(float emitAngle, int rayCount, Vector3 dir)
    {
        Vector3[] dirs = new Vector3[rayCount];
        float deltaRad = 2 * Mathf.PI / rayCount;
        for (int i = 0; i < rayCount; i++)
        {
            Quaternion angle = Quaternion.Euler(
                Mathf.Cos(deltaRad * i) * emitAngle,
                Mathf.Sin(deltaRad * i) * emitAngle,
                0f
            );
            dirs[i] = angle * dir;
        }
        return dirs;
    }

    // Отрисовывает лучи по массиву направлений, переданному в функцию 
    void DrawRays(Vector3[] rayDirs)
    {
        // Перебираем все направления
        for (int i = 0; i < rayDirs.Length; i++)
        {
            // Создаём луч
            Ray ray = new Ray(transform.position, rayDirs[i]);
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
}
