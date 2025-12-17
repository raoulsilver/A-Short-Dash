using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    Vector3 shakeOffset;

    public IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0f;

        // Internally upscale everything so caller values stay small
        float strength = magnitude * 3.5f;
        float frequency = 6f; // low frequency = slow + smooth

        // Random starting offsets so it doesn’t feel repetitive
        float seedX = Random.Range(0f, 1000f);
        float seedY = Random.Range(0f, 1000f);

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Heavy impact curve: fast hit, slow settle
            float envelope = 1f - Mathf.Pow(1f - t, 3f);

            float x = (Mathf.PerlinNoise(seedX + Time.time * frequency, 0f) * 2f - 1f)
                      * strength * (1f - envelope);
            float y = (Mathf.PerlinNoise(0f, seedY + Time.time * frequency) * 2f - 1f)
                      * strength * (1f - envelope);

            shakeOffset = new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
    }

    void LateUpdate()
    {
        transform.localPosition += shakeOffset;
    }
}