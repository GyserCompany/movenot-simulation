    using UnityEngine;

    public class ServoController : MonoBehaviour
    {
        [Header("Параметры сервомотора")]
        [Tooltip("Скорость поворота [°/сек]")][Min(0.001f)]
        public float rotationSpeed = 45f;
    
        [SerializeField] private Transform horn; // Трансформ головки сервомотора
        private float targetAngle = 0f; // Угол, на который необходимо повернуться

        void Start()
        {
            // Находим головку если она не выставлена в инспекторе
            if (horn == null)
                horn = transform.Find("Horn");
        }

        // Выставляет необходимый угол поворота головки серво (в градусах)
        public void SetAngle(float angle)
        {
            if (angle < 0)
                angle = 0;
            else if (angle > 180)
                angle = 180;

            targetAngle = angle;
        }

        void Update()
        {
            // Поворачиваем головку если необходимо
            float currentAngle = horn.localEulerAngles.y;
            currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            horn.localEulerAngles = new Vector3(0, currentAngle, 0);

            // Бесконечная анимация поворота
            if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle)) < 0.1f)
                targetAngle = targetAngle == 90f ? 0f : 90f;
        }
    }
