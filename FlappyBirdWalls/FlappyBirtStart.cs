using MelonLoader;
using UnityEngine;

namespace FlappyBirdWalls
{
    [RegisterTypeInIl2Cpp]
    public class FlappyBirdStart : MonoBehaviour
    {
        private FlappyBird gameToStart;

        public FlappyBirdStart()
        {
            gameToStart = this.gameObject.transform.parent.GetComponent<FlappyBird>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!Main.touchPlay || ((other.gameObject.name != "Bone_HandAlpha_L") && (other.gameObject.name != "Bone_HandAlpha_R")))
            {
                return;
            }
            if (!gameToStart.gameStarted)
            {
                gameToStart.StartGame();
            }
            else
            {
                gameToStart.Jump();
            }
        }
    }
}
