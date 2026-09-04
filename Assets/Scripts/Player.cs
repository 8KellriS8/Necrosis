using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour

{
    public float speed = 5f;
    private Vector2 moveInput;
    private float speedMult = 1.0f;

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        Vector2 input = Vector2.zero;
        if (keyboard.wKey.isPressed) input.y += 1;
        if (keyboard.sKey.isPressed) input.y -= 1;
        if (keyboard.aKey.isPressed) input.x -= 1;
        if (keyboard.dKey.isPressed) input.x += 1;
        if (keyboard.leftShiftKey.isPressed)
        {
            speedMult = 2.0f;
        }
        else
        {
            speedMult = 0.75f;
        }
        input = input.normalized;
        
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        transform.Translate(move * speed * Time.deltaTime * speedMult, Space.World);
    }
}
