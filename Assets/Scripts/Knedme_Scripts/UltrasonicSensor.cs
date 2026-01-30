using UnityEngine;
using Meta.Numerics.Functions;
using System.Collections.Generic;
using System.Linq;

public class UltrasonicSensor : MonoBehaviour
{
    // Физические константы
    private readonly float AirAttenuation = 0.14f; // Нп/м
    private readonly int   SpeedOfSound   = 343; // м/c

    [Header("Параметры датчика")]
    [Tooltip("Минимальный рабочий диапазон датчика [м]")][Min(0)]
    public float minRange = 0.02f;
    [Tooltip("Максимальный рабочий диапазон датчика [м]")][Min(0.01f)]
    public float maxRange = 4;
    [Tooltip("Эффективный угол датчика (измеряется от центра в градусах)")]
    public float effectiveAngle = 15;
    [Tooltip("Рабочая частота датчика [Гц]")][Min(1)]
    public int frequency = 40_000;
    [Tooltip("Диаметр преобразователя датчика [м]")][Min(0.001f)]
    public float transducerDiameter = 0.008f;
    [Tooltip("Позиция излучателя относительно датчика")]
    public Vector3 transducerPos = new Vector3(0, -0.01273f, 0.01181f);
    [Tooltip("Отображать коллайдер или нет")]
    public bool displayCollider = true;

    // Детектор, позволяющий нам получать объекты, находящиеся внутри коллайдера
    private TriggerDetector detector;

    void Start()
    {
        // Валидация введённых параметров
        effectiveAngle = Mathf.Clamp(effectiveAngle, 0, 360);
        if (effectiveAngle == 0 || minRange >= maxRange)
        {
            Debug.LogError("Были введены недопустимые значения параметров.");
            return;
        }
        float transducerRadius = transducerDiameter/2;
        float radAngle = Mathf.Deg2Rad * effectiveAngle;

        // По формулам преобразуем параметры датчика в геометрические параметры коллайдера
        float coneTopRadius = minRange * Mathf.Tan(radAngle);
        float coneHeight = (1f/AirAttenuation) * AdditionalMath.LambertW(
            (AirAttenuation * maxRange * SpeedOfSound * (float)AdvancedMath.BesselJ(1,
            (2f/SpeedOfSound) * Mathf.PI * frequency * transducerRadius * Mathf.Sin(radAngle)))
                /
            (Mathf.Exp(-AirAttenuation*maxRange) * Mathf.PI * frequency * transducerRadius * Mathf.Sin(radAngle))
        ) - minRange;
        float coneBottomRadius = (coneHeight + minRange) * Mathf.Tan(radAngle);
        float spheroidRadiusY = maxRange - coneHeight - minRange;

        // Создаём сам коллайдер и даём ему форму по геометрическим параметрам
        GameObject sensorCollider = new GameObject("SensorCollider");
        Mesh colliderShape = CreateShape(coneTopRadius, coneHeight, coneBottomRadius, spheroidRadiusY);
        MeshCollider meshCollider = sensorCollider.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = colliderShape;

        // Берём transform у родителя и выставляем позицию относительно родителя
        sensorCollider.transform.SetParent(transform, false);
        sensorCollider.transform.localPosition = transducerPos + Vector3.forward * minRange;

        // Чтобы работало обнаружение объектов коллайдером
        meshCollider.convex = true;
        meshCollider.isTrigger = true;
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Добавляем детектор, чтобы обнаружать объекты внутри коллайдера
        detector = sensorCollider.AddComponent<TriggerDetector>();

        // Если флаг displayCollider установлен, ещё создаём MeshRenderer для визуализации коллайдера
        if (displayCollider)
        {
            MeshFilter meshFilter = sensorCollider.AddComponent<MeshFilter>();
            meshFilter.mesh = colliderShape;
            MeshRenderer meshRenderer = sensorCollider.AddComponent<MeshRenderer>();
            // Задаём материал и полупрозрачный цвет
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color32(60, 176, 67, 128);
            meshRenderer.material = mat;
        }
    }

    // Возвращает расстояние до обнаруженного объекта. В юнитах, 1 юнит считается 1 метром
    // Делает это точно так же, как и функция при коде на ардуино
    public float GetDistance()
    {
        // Сортируем полученные объекты по расстоянию до их ближайшей точки
        GameObject[] sortedObjects = detector.CollidingObjects
            .OrderBy(obj => {
                Collider col = obj.GetComponent<Collider>();
                if (col == null) return float.MaxValue;
                Vector3 closestPoint = col.ClosestPoint(transform.position);
                return (closestPoint - transform.position).sqrMagnitude;
            })
            .ToArray();

        foreach (GameObject obj in sortedObjects)
        {
            // В дальнейшем, тут можно пропустить объект, если у него плохая акустика

            // Возвращаем расстояние до ближайшей точки объекта
            Collider col = obj.GetComponent<Collider>();
            if (col == null) continue;
            Vector3 closestPoint = col.ClosestPoint(transform.position);
            return Vector3.Distance(transform.position, closestPoint);
        }

        // Если ничего не обнаружили, то просто возвращается 0
        return 0.0f;
    }

    void Update()
    {
        Debug.LogFormat("Ультразвуковой датчик показывает: {0} м.", GetDistance());
    }

    // Создаёт геометрическую форму (усечённый конус + сфероид) для коллайдера по заданым параметрам
    private Mesh CreateShape(
        float coneTopRadius, float coneHeight, float coneBottomRadius, float spheroidRadiusY)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        // Количество сегментов у конуса и сфероида
        const int radialSegments = 32;
        const int spheroidSegments = 16;

        // Генерируем усечённый конус
        for (int i = 0; i <= radialSegments; i++)
        {
            float angle = (float)i / radialSegments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);
            
            // Верхний круг
            vertices.Add(new Vector3(x * coneTopRadius, y * coneTopRadius, 0));
            // Нижний круг
            vertices.Add(new Vector3(x * coneBottomRadius, y * coneBottomRadius, coneHeight));
        }

        // Треугольники конуса
        for (int i = 0; i < radialSegments; i++)
        {
            int current = i * 2;
            int next = (i + 1) * 2;
            
            triangles.Add(current);
            triangles.Add(next);
            triangles.Add(current + 1);
            
            triangles.Add(current + 1);
            triangles.Add(next);
            triangles.Add(next + 1);
        }

        int coneVertexCount = vertices.Count;

        // Генерируем половину сфероида сверху
        for (int lat = 0; lat <= spheroidSegments / 2; lat++)
        {
            float theta = (float)lat / (spheroidSegments / 2) * Mathf.PI * 0.5f;
            
            for (int lon = 0; lon <= radialSegments; lon++)
            {
                float phi = (float)lon / radialSegments * Mathf.PI * 2f;
                
                float x = coneBottomRadius * Mathf.Sin(theta) * Mathf.Cos(phi);
                float y = coneBottomRadius * Mathf.Sin(theta) * Mathf.Sin(phi);
                float z = spheroidRadiusY * Mathf.Cos(theta);
                
                vertices.Add(new Vector3(x, y, coneHeight + z));
            }
        }
        
        // Треугольники сфероида
        for (int lat = 0; lat < spheroidSegments / 2; lat++)
        {
            for (int lon = 0; lon < radialSegments; lon++)
            {
                int current = coneVertexCount + lat * (radialSegments + 1) + lon;
                int next = current + radialSegments + 1;
                
                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(current + 1);
                
                triangles.Add(current + 1);
                triangles.Add(next);
                triangles.Add(next + 1);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();    
        return mesh;
    }
}
