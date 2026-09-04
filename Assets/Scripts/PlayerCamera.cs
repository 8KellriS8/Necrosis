using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public float sensitivity = 2f;
    private float xRotation = 0f;
    private Vector2 mouseDelta;

    void Update()
    {
        mouseDelta = Mouse.current.delta.ReadValue() * sensitivity * 0.1f;

        // Поворачиваем ВСЕГО игрока по горизонтали
        transform.parent.Rotate(Vector3.up * mouseDelta.x);

        // Поворачиваем ТОЛЬКО камеру по вертикали (с ограничением)
        xRotation -= mouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
}
