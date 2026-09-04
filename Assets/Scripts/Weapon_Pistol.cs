using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    public RawImage[] weaponImages; // [0] - Idle, [1] - Aim, [2] - Shoot
    public float shootDuration = 0.1f;
    
    private enum WeaponState { Idle, Aim, Shoot }
    private bool isShooting = false;
    private float timer = 0f;

    void Start()
    {
        ShowState(WeaponState.Aim);
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        Mouse mouse = Mouse.current;
        if (mouse == null) return;
        // Shift зажат = Idle, иначе Aim
        if (keyboard.leftShiftKey.isPressed)
        {
            if (!isShooting) ShowState(WeaponState.Idle);
        }
        else
        {
            if (!isShooting) ShowState(WeaponState.Aim);
        }

        // Выстрел
        if (mouse.leftButton.wasPressedThisFrame && !isShooting)
        {
            Shoot();
        }

        // Таймер
        if (isShooting)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isShooting = false;
                // Возврат
                if (keyboard.leftShiftKey.isPressed)
                    ShowState(WeaponState.Idle);
                else
                    ShowState(WeaponState.Aim);
            }
        }
    }

    void Shoot()
    {
        isShooting = true;
        timer = shootDuration;
        ShowState(WeaponState.Shoot);
        
        // Raycast выстрел
        RaycastHit hit;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Physics.Raycast(ray, out hit, 100f);
    }

    void ShowState(WeaponState state)
    {
        // Скрываем всё
        foreach (var img in weaponImages)
        {
            if (img != null) img.gameObject.SetActive(false);
        }
        
        // Показываем нужное
        if (weaponImages.Length > (int)state && weaponImages[(int)state] != null)
        {
            weaponImages[(int)state].gameObject.SetActive(true);
        }
    }
}