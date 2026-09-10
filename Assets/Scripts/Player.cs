using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour

{
    public float speed = 5f;
    public float hp = 100f;
    private Vector2 moveInput;
    private float speedMult = 1.0f;
    public int ammo1 = 0;
    public int ammo2 = 0;

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
    public void GetHit(float dmg)
    {
        hp -= dmg;
        Debug.Log(hp);
    }
    public void ChangeAmmoAmount(int type, int amount)
    {
        if (type == 1)
        {
            if  (ammo1+amount>130)  ammo1 = 130;
            else ammo1 += amount;
        }
        else
        {
            if  (ammo2+amount>80)  ammo2 = 130;
            else ammo2 += amount;
        }
    }
}
