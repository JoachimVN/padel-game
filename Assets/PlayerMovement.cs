using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float speed = 7f;

    Rigidbody rb;
    float halfSign;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        halfSign = Mathf.Sign(transform.position.z);
    }

    void FixedUpdate()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float h = 0f;
        float v = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  h -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v -= 1f;

        Vector3 move = new Vector3(h, 0f, v).normalized;
        rb.linearVelocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);

        Vector3 pos = rb.position;
        pos.x = Mathf.Clamp(pos.x, -4.5f, 4.5f);
        pos.z = halfSign > 0
            ? Mathf.Clamp(pos.z, 0.5f, 9.5f)
            : Mathf.Clamp(pos.z, -9.5f, -0.5f);
        rb.MovePosition(pos);
    }
}
