using UnityEngine;

public class AcousticObject : MonoBehaviour
{
    [Header("Акустические свойства")]
    [SerializeField, Range(0f, 1f), Tooltip("Акустический коэффициент (0 = не поглощает звук, 1 = полностью поглощает звук)")]
    private float coefficient = 0.0f;
    public float Coefficient => coefficient;
}
