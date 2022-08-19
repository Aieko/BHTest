using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace BHTest
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager instance;

        private NetworkRoomManagerExt NetworkRoomManager;
        [SyncVar]
        private bool gameOver = false;

        private static Rect Menu11;

        private static int winnerId = 0;

        private int secondsToRestart = 5;
        // Start is called before the first frame update
        void Start()
        {
            if (instance == null)
            { // Ёкземпл€р менеджера был найден
                instance = this; // «адаем ссылку на экземпл€р объекта
            }
            else if (instance == this)
            { // Ёкземпл€р объекта уже существует на сцене
                Destroy(gameObject); // ”дал€ем объект
            }

            NetworkRoomManager = FindObjectOfType<NetworkRoomManagerExt>();

            int windowWidth = 200;
            int windowHeight = 200;
            int x = (Screen.width - windowWidth) / 2;
            int y = (Screen.height - windowWidth) / 2;
            Menu11 = new Rect(x, y, windowWidth, windowHeight);

        }

        public void CheckWinner(uint score, int id)
        {
            if (score == 3)
            {
                gameOver = true;
                winnerId = id;
                RpcRestart();
            }
        }

        [ClientRpc]
        private void RpcRestart()
        {
            StartCoroutine(RestartCoroutine());
        }

        private IEnumerator RestartCoroutine()
        {
            do
            {
                secondsToRestart--;

                yield return new WaitForSeconds(1);
            } while (secondsToRestart > 0);

            NetworkRoomManager.ServerChangeScene(NetworkRoomManager.RoomScene);

            gameOver = false;
            
        }

        private void OnGUI()
        {
            if (gameOver)
            {
                GUI.TextArea(Menu11, $"GAME OVER! Winner is player {winnerId}\n\r New game will start in {secondsToRestart}");
            }
        }  
    }
}
