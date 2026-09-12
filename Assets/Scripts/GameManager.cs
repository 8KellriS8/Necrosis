using UnityEngine;
using UnityEngine.SceneManagement;

namespace CorridorShooter
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        int enemiesAlive = 0;
        bool gameOver = false;
        string endMessage = "";

        void Awake()
        {
            Instance = this;
        }

        public void RegisterEnemy() { enemiesAlive++; }

        public void EnemyDied()
        {
            enemiesAlive--;
            if (enemiesAlive <= 0 && !gameOver) EndGame(true);
        }

        public bool IsGameOver() { return gameOver; }

        public void EndGame(bool win)
        {
            if (gameOver) return;
            gameOver = true;
            endMessage = win ? "УРОВЕНЬ ПРОЙДЕН!" : "ВЫ ПОГИБЛИ";
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            if (gameOver && Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        void OnGUI()
        {
            GUIStyle hud = new GUIStyle(GUI.skin.label) { fontSize = 18 };
            hud.normal.textColor = Color.green;

            if (!gameOver && PlayerStats.Instance != null)
            {
                string line = string.Format("HP {0}   ARM {1}   AMMO {2}   ENEMIES {3}   SECRETS {4}/{5}",
                    Mathf.CeilToInt(PlayerStats.Instance.health),
                    Mathf.CeilToInt(PlayerStats.Instance.armor),
                    PlayerStats.Instance.ammo,
                    enemiesAlive,
                    PlayerStats.Instance.secretsFound,
                    PlayerStats.Instance.secretsTotal);
                GUI.Label(new Rect(10, Screen.height - 34, 700, 30), line, hud);
                GUI.Label(new Rect(Screen.width / 2 - 5, Screen.height / 2 - 5, 20, 20), "+");
            }

            if (gameOver)
            {
                GUIStyle big = new GUIStyle(GUI.skin.label) { fontSize = 30, alignment = TextAnchor.MiddleCenter };
                big.normal.textColor = Color.green;
                GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 - 60, 440, 60), endMessage, big);

                GUIStyle small = new GUIStyle(big) { fontSize = 16 };
                GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2, 440, 30), "Нажмите R для перезапуска", small);
            }
        }
    }
}
