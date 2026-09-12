using UnityEngine;

namespace CorridorShooter
{
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance;

        public float health = 100f;
        public float armor = 0f;
        public int ammo = 50;
        public int secretsFound = 0;
        public int secretsTotal = 2;

        void Awake()
        {
            Instance = this;
        }

        public void TakeDamage(float dmg)
        {
            if (armor > 0)
            {
                float absorbed = Mathf.Min(armor, dmg * 0.5f);
                armor -= absorbed;
                dmg -= absorbed;
            }
            health -= dmg;
            if (health <= 0)
            {
                health = 0;
                if (GameManager.Instance != null) GameManager.Instance.EndGame(false);
            }
        }

        public void AddHealth(float v) { health = Mathf.Min(100, health + v); }
        public void AddArmor(float v) { armor = Mathf.Min(100, armor + v); }
        public void AddAmmo(int v) { ammo += v; }
    }
}
