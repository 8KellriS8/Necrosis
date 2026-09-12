using UnityEngine;

namespace CorridorShooter
{
    public class SecretDoor : MonoBehaviour
    {
        public GameObject[] wallObjects;
        bool playerInRange = false;
        bool opened = false;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) playerInRange = true;
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) playerInRange = false;
        }

        void Update()
        {
            if (!opened && playerInRange && Input.GetKeyDown(KeyCode.E))
            {
                opened = true;
                foreach (var w in wallObjects)
                {
                    if (w != null) Destroy(w);
                }
                if (PlayerStats.Instance != null) PlayerStats.Instance.secretsFound++;
            }
        }
    }
}
