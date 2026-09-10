using UnityEngine;

public class Atack : MonoBehaviour
{
    public GameObject playerObject;
    public Player playerScript;
    public int power;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerObject = GameObject.FindWithTag("Player");
        playerScript = playerObject.GetComponent<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerScript.GetHit(power);
    }
}
