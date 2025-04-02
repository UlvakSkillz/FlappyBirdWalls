using HarmonyLib;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.MoveSystem;
using Il2CppRUMBLE.Players.Subsystems;
using Il2CppRUMBLE.Pools;
using Il2CppRUMBLE.Poses;
using MelonLoader;
using RumbleModUI;
using UnityEngine;

namespace FlappyBirdWalls
{
    class BuildInfo
    {
        public const string ModName = "FlappyBirdWalls";
        public const string Author = "UlvakSkillz";
        public const string ModVersion = "1.0.1";
    }

    public class Main : MelonMod
    {
        public static GameObject ddolCanvas;
        private Mod FlappyBirdWalls = new Mod();
        public static bool touchPlay = true;
        public static bool kickPlay = true;

        [HarmonyPatch(typeof(Structure), "Start")]
        public static class StructureSpawn
        {
            private static void Postfix(ref Structure __instance)
            {
                try
                {
                    if (__instance.processableComponent.gameObject.name != "Wall") { return; }
                }
                catch (Exception e)
                {
                    MelonLogger.Msg(e);
                    return;
                }
                __instance.processableComponent.gameObject.transform.GetChild(0).gameObject.AddComponent<FlappyBird>();
            }
        }

        [HarmonyPatch(typeof(PlayerPoseSystem), "OnPoseSetCompleted", new Type[] { typeof(PoseSet) })]
        private static class PosePatch
        {
            private static void Postfix(PoseSet set)
            {
                if (!kickPlay) { return; }
                if (set.name == "PoseSetKick")
                {
                    GameObject wallSelected = SelectStructure();
                    if (wallSelected == null) { return; }
                    FlappyBird flappyBird = wallSelected.transform.GetChild(0).GetComponent<FlappyBird>();
                    if (!flappyBird.gameStarted)
                    {
                        flappyBird.StartGame();
                    }
                    else
                    {
                        flappyBird.Jump();
                    }
                }
            }
        }

        public static GameObject SelectStructure()
        {
            List<GameObject> walls = new List<GameObject>();
            List<GameObject> closestWallsGOs = new List<GameObject>();
            List<float> closestWallsDistance = new List<float>();
            Il2CppSystem.Collections.Generic.List<PooledMonoBehaviour> pooledObjects = PoolManager.instance.availablePools[PoolManager.instance.GetPoolIndex("Wall")].PooledObjects;
            Transform playerPos = PlayerManager.instance.localPlayer.Controller.gameObject.transform.GetChild(1).GetChild(0).GetChild(0); //headset
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                GameObject wall = pooledObjects[i].networkGameObject.cachedRigidBody.gameObject;
                Vector3 difference = wall.transform.position - playerPos.position;
                float distence = Vector3.Distance(wall.transform.position, playerPos.position);
                difference = new Vector3(Math.Abs(difference.x), Math.Abs(difference.y), Math.Abs(difference.z));
                if ((wall.active) && (difference.x < 2.5f) && (difference.y < 2.5f) && (difference.z < 2.5f))
                {
                    Vector3 forwardVector = playerPos.forward.normalized;
                    Vector3 directionToObject = (wall.transform.position - playerPos.position).normalized;
                    float dotProduct = Vector3.Dot(forwardVector, directionToObject);
                    float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg; // Convert radians to degrees
                    float halfFOV = 180f; //degree FOV

                    if (angle <= halfFOV)
                    {
                        walls.Add(wall);
                        if (angle <= 30f)
                        {
                            closestWallsGOs.Add(wall);
                            closestWallsDistance.Add(distence);
                        }
                    }
                }
            }
            if (walls.Count > 0)
            {
                if (closestWallsGOs.Count > 0)
                {
                    int smallestDistanceSpot = -1;
                    float smallestDistance = 10;
                    for (int i = 0; i < closestWallsGOs.Count; i++)
                    {
                        if (closestWallsDistance[i] < smallestDistance)
                        {
                            smallestDistanceSpot = i;
                            smallestDistance = closestWallsDistance[i];
                        }
                    }
                    return closestWallsGOs[smallestDistanceSpot];
                }
                else
                {
                    int closest = -1;
                    float closestDistance = 10;
                    for (int i = 0; i < closestWallsGOs.Count; i++)
                    {
                        float distance = Vector3.Distance(walls[i].transform.position, playerPos.transform.position);
                        if (distance < closestDistance)
                        {
                            closest = i;
                            closestDistance = distance;
                        }
                    }
                    return walls[closest];
                }
            }
            else { return null; }
        }

        public override void OnLateInitializeMelon()
        {
            ddolCanvas = LoadAssetBundle("FlappyBirdWalls.flappybird", "Canvas");
            ddolCanvas.name = "Flappy Bird Canvas";
            ddolCanvas.SetActive(false);
            GameObject.DontDestroyOnLoad(ddolCanvas);
            FlappyBirdWalls.ModName = BuildInfo.ModName;
            FlappyBirdWalls.ModVersion = BuildInfo.ModVersion;
            FlappyBirdWalls.SetFolder("FlappyBirdWalls");
            FlappyBirdWalls.AddToList("Touch Play", true, 0, "Toggles Touching the Wall to Play", new Tags { });
            FlappyBirdWalls.AddToList("Kick Play", true, 0, "Toggles Kicking the Wall to Play", new Tags { });
            FlappyBirdWalls.GetFromFile();
            UI.instance.UI_Initialized += UIInit;
            FlappyBirdWalls.ModSaved += Save;
        }

        private void UIInit()
        {
            UI.instance.AddMod(FlappyBirdWalls);
        }

        private void Save()
        {
            touchPlay = (bool)FlappyBirdWalls.Settings[0].SavedValue;
            kickPlay = (bool)FlappyBirdWalls.Settings[1].SavedValue;
        }

        public GameObject LoadAssetBundle(string bundleName, string objectName)
        {
            using (Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream(bundleName))
            {
                byte[] bundleBytes = new byte[bundleStream.Length];
                bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
                Il2CppAssetBundle bundle = Il2CppAssetBundleManager.LoadFromMemory(bundleBytes);
                return UnityEngine.Object.Instantiate(bundle.LoadAsset<GameObject>(objectName));
            }
        }
    }
}
