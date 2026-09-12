using UnityEngine;

namespace CorridorShooter
{
    public enum PickupType { Health, Ammo, Armor }

    public class PickupItem : MonoBehaviour
    {
        public PickupType type;
        public float amount;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (PlayerStats.Instance == null) return;

            switch (type)
            {
                case PickupType.Health: PlayerStats.Instance.AddHealth(amount); break;
                case PickupType.Ammo: PlayerStats.Instance.AddAmmo((int)amount); break;
                case PickupType.Armor: PlayerStats.Instance.AddArmor(amount); break;
            }
            Destroy(gameObject);
        }
    }
}
