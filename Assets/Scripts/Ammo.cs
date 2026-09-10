using UnityEngine;

public class Ammo : MonoBehaviour
{
    public GameObject playerObject;
    public Player playerScript;
    public int type;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerObject = GameObject.FindWithTag("Player");
        playerScript = playerObject.GetComponent<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerScript.ChangeAmmoAmount(type, 30);
        Debug.Log(playerScript.ammo1);
        gameObject.SetActive(false);
    }
}
