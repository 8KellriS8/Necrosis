using UnityEngine;

namespace CorridorShooter
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        public float moveSpeed = 6f;
        public float mouseSensitivity = 2f;
        public float shotDamageMin = 15f;
        public float shotDamageMax = 30f;
        public float shotRange = 60f;
        public float fireRate = 0.32f;

        CharacterController cc;
        Camera cam;
        float pitch = 0f;
        float fireTimer = 0f;
        float verticalVelocity = 0f;

        void Start()
        {
            cc = GetComponent<CharacterController>();
            cam = GetComponentInChildren<Camera>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver()) return;

            // поворот мышью
            float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
            transform.Rotate(Vector3.up * mx);
            pitch = Mathf.Clamp(pitch - my, -80f, 80f);
            if (cam != null) cam.transform.localRotation = Quaternion.Euler(pitch, 0, 0);

            // движение
            float h = Input.GetAxisRaw("Horizontal"); // A/D
            float v = Input.GetAxisRaw("Vertical");   // W/S
            Vector3 move = (transform.right * h + transform.forward * v);
            if (move.magnitude > 1f) move.Normalize();
            move *= moveSpeed;

            if (cc.isGrounded) verticalVelocity = -0.5f;
            else verticalVelocity -= 9.8f * Time.deltaTime;
            move.y = verticalVelocity;

            cc.Move(move * Time.deltaTime);

            // стрельба
            fireTimer -= Time.deltaTime;
            bool wantsFire = Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1");
            if (wantsFire && fireTimer <= 0f && PlayerStats.Instance != null && PlayerStats.Instance.ammo > 0)
            {
                Shoot();
                fireTimer = fireRate;
                PlayerStats.Instance.ammo--;
            }
        }

        void Shoot()
        {
            if (cam == null) return;
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, shotRange))
            {
                EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(Random.Range(shotDamageMin, shotDamageMax));
                }
            }
        }
    }
}
