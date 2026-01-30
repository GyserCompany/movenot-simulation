using UnityEngine;

// Имплементация некоторых нужных математических функций
public static class AdditionalMath
{
    public static float LambertW(float x)
    {
        if (x < -Mathf.Exp(-1))
            Debug.LogErrorFormat("Функция LambertW не определена для {0}.", x);

        int amountOfIterations = Mathf.Max(4, (int)Mathf.Ceil(Mathf.Log10(x) / 3)) + 1000000;
        float w = 3 * Mathf.Log(x + 1) / 4;
        for (int i = 0; i < amountOfIterations; i++)
            w = w - (w * Mathf.Exp(w) - x) / (Mathf.Exp(w) * (w + 1) - (w + 2) * (w * Mathf.Exp(w) - x) / (2 * w + 2));

        return w;
    }
}
