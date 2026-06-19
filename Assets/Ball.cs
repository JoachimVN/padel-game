using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Ball : MonoBehaviour
{
    void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.mass = 0.057f;       // 57 grams — official padel ball weight
        rb.linearDamping = 0.5f;       // air resistance
        rb.angularDamping = 0.5f;

        // Padel ball diameter: ~6.5 cm = 0.065 m
        // With default sphere (radius 0.5), set scale to (0.065, 0.065, 0.065)
        transform.localScale = Vector3.one * 0.065f;
    }
}
