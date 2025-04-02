using Il2CppTMPro;
using MelonLoader;
using System.Collections;
using UnityEngine;

namespace FlappyBirdWalls
{
    [RegisterTypeInIl2Cpp]
    public class FlappyBird : MonoBehaviour
    {
        private Bird bird = new Bird();
        private List<Vector2> pipes = new List<Vector2>() { Vector2.zero, Vector2.zero };
        private System.Random random = new System.Random();
        private const int maxPipeHeight = 200;
        private const float halfScreenHeight = 400;
        private const float halfScreenWidth = 250;
        public bool gameStarted = false;
        private GameObject canvas, birdMain, birdUp, birdFlat, birdDown, pipe1, pipe2, score1, score2;

        class Bird
        {
            public bool isAlive = false;
            public int gameScore = 0;
            public float height = 0;
            public float speedY = 0;

            public Bird()
            {
                isAlive = true;
                gameScore = 0;
                height = 0;
                speedY = 0;
            }
        }

        void Start()
        {
            GameObject triggerGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Component.Destroy(triggerGO.GetComponent<MeshRenderer>());
            triggerGO.name = "FlappyBirdTrigger";
            triggerGO.transform.parent = this.gameObject.transform;
            triggerGO.transform.localPosition = new Vector3(0f, 0f, -0.22f);
            triggerGO.transform.localRotation = Quaternion.identity;
            triggerGO.transform.localScale = new Vector3(1.1f, 1.9f, 0.01f);
            triggerGO.layer = 22;
            triggerGO.GetComponent<BoxCollider>().isTrigger = true;
            triggerGO.AddComponent<FlappyBirdStart>();
            canvas = GameObject.Instantiate(Main.ddolCanvas);
            score1 = canvas.transform.GetChild(0).GetChild(0).gameObject;
            
            score2 = canvas.transform.GetChild(0).GetChild(1).gameObject;
            pipe1 = canvas.transform.GetChild(1).GetChild(0).gameObject;
            pipe2 = canvas.transform.GetChild(1).GetChild(1).gameObject;
            birdMain = canvas.transform.GetChild(2).gameObject;
            birdUp = canvas.transform.GetChild(2).GetChild(0).gameObject;
            birdFlat = canvas.transform.GetChild(2).GetChild(1).gameObject;
            birdDown = canvas.transform.GetChild(2).GetChild(2).gameObject;
            canvas.transform.parent = this.gameObject.transform;
            canvas.transform.localPosition = new Vector3(0f, 0f, -0.225f);
            canvas.transform.localRotation = Quaternion.identity;
            canvas.transform.localScale = new Vector3(0.0023f, 0.0023f, 0.0023f);
        }

        public void StartGame()
        {
            gameStarted = true;
            bird = new Bird();
            pipes = new List<Vector2>() { new Vector2(halfScreenWidth + 25, random.Next(-maxPipeHeight, maxPipeHeight)), new Vector2((halfScreenWidth * 2) + 75, random.Next(-maxPipeHeight, maxPipeHeight)) };
            canvas.SetActive(true);
            TextMeshProUGUI score1TMP = score1.GetComponent<TextMeshProUGUI>();
            score1TMP.text = "0";
            score1TMP.alignment = TextAlignmentOptions.Center;
            score1TMP.fontSize = 64f;
            TextMeshProUGUI score2TMP = score2.GetComponent<TextMeshProUGUI>();
            score2TMP.text = "1";
            score2TMP.alignment = TextAlignmentOptions.Center;
            score2TMP.fontSize = 64f;
            MelonCoroutines.Start(StopGameIfStructureDestroyed(this.gameObject.transform.parent.gameObject));
            MelonCoroutines.Start(GameLoop());
        }

        private IEnumerator StopGameIfStructureDestroyed(GameObject structure)
        {
            while (structure.active)
            {
                yield return new WaitForFixedUpdate();
            }
            canvas.SetActive(false);
            bird.isAlive = false;
            yield break;
        }

        public void Jump()
        {
            if (bird.speedY < 5f) { bird.speedY = 0; }
            bird.speedY += 5f;
        }

        private IEnumerator GameLoop()
        {
            while (bird.isAlive && gameStarted)
            {
                CheckPipes();
                MovePipes();
                MoveBird();
                UpdatePositions();
                yield return new WaitForFixedUpdate();
            }
            gameStarted = false;
            yield break;
        }

        private void UpdatePositions()
        {
            birdMain.transform.localPosition = new Vector3(-halfScreenWidth / 2, bird.height, 0);
            pipe1.transform.localPosition = new Vector3(pipes[0].x, pipes[0].y, 0);
            pipe2.transform.localPosition = new Vector3(pipes[1].x, pipes[1].y, 0);
            score1.transform.parent = pipe1.transform;
            score1.transform.localPosition = Vector3.zero;
            score2.transform.parent = pipe2.transform;
            score2.transform.localPosition = Vector3.zero;
        }

        private void MoveBird()
        {
            bird.speedY -= 0.1f;
            bird.height += bird.speedY;
            if (Math.Abs(bird.speedY) < 1)
            {
                if (!birdFlat.active)
                {
                    birdFlat.SetActive(true);
                    birdUp.SetActive(false);
                    birdDown.SetActive(false);
                }
            }
            else if (0 < bird.speedY)
            {
                if (!birdUp.active)
                {
                    birdUp.SetActive(true);
                    birdFlat.SetActive(false);
                    birdDown.SetActive(false);
                }
            }
            else if (bird.speedY < 0)
            {
                if (!birdDown.active)
                {
                    birdDown.SetActive(true);
                    birdUp.SetActive(false);
                    birdFlat.SetActive(false);
                }
            }
            if (bird.height < -halfScreenHeight)
            {
                bird.isAlive = false;
            }
            if (bird.height > halfScreenHeight)
            {
                bird.isAlive = false;
            }
        }

        private void CheckPipes()
        {
            for (int i = 0; i < pipes.Count; i++)
            {
                if (pipes[i].x < -halfScreenWidth - 50)
                {
                    pipes[i] = new Vector2(halfScreenWidth + 50, random.Next(-maxPipeHeight, maxPipeHeight));
                    TextMeshProUGUI tmpgui = ((i == 0) ? pipe1 : pipe2).transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                    tmpgui.text = (int.Parse(tmpgui.text) + 2).ToString();
                }
            }
        }

        private void MovePipes()
        {
            for (int i = 0; i < pipes.Count; i++)
            {
                pipes[i] = new Vector2(pipes[i].x - 1f, pipes[i].y);
                if ((Math.Abs((-halfScreenWidth / 2) - pipes[i].x) < 75f) &&  (80 < Math.Abs(bird.height - pipes[i].y)))
                {
                    bird.isAlive = false;
                }
            }
        }
    }
}
