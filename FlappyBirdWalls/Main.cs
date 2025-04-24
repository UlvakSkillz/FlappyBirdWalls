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
        public const string ModVersion = "1.0.2";
    }

    public class Main : MelonMod
    {
        public static GameObject ddolCanvas;
        private Mod FlappyBirdWalls = new Mod();
        public static bool touchPlay = true;
        public static bool kickPlay = true;
        private static Il2CppSystem.Collections.Generic.List<PooledMonoBehaviour> pooledStructuresWall;
        private static Il2CppSystem.Collections.Generic.List<PooledMonoBehaviour> pooledStructuresCube;
        private static Il2CppSystem.Collections.Generic.List<PooledMonoBehaviour> pooledStructuresPillar;
        private static Il2CppSystem.Collections.Generic.List<PooledMonoBehaviour> pooledStructuresBall;
        private static Il2CppSystem.Collections.Generic.List<PooledMonoBehaviour> pooledStructuresDisc;
        private static Il2CppSystem.Collections.Generic.List<PooledMonoBehaviour> pooledStructuresBoulder;
        private static Il2CppSystem.Collections.Generic.List<PooledMonoBehaviour> pooledStructuresSmallRock;
        private static Il2CppSystem.Collections.Generic.List<PooledMonoBehaviour> pooledStructuresTetherBall;

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
                    GameObject structureSelected = SelectStructure(PlayerManager.instance.localPlayer.Controller.gameObject.transform.GetChild(1).GetChild(0).GetChild(0));
                    if ((structureSelected == null) || (structureSelected.name != "Wall")) { return; }
                    FlappyBird flappyBird = structureSelected.transform.GetChild(0).GetComponent<FlappyBird>();
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

        public static GameObject SelectStructure(Transform playerPos)
        {
            List<GameObject> structuresWithinZOM = new List<GameObject>(); //ZOM = Zone of Modification
            List<GameObject> lookingAtStructureGOs = new List<GameObject>();
            List<float> lookingAtStructureDistances = new List<float>();
            Il2CppSystem.Collections.Generic.List<PooledMonoBehaviour> pooledStructures = new Il2CppSystem.Collections.Generic.List<PooledMonoBehaviour>();
            //Add each Turned On Structure to a Single List
            for (int i = 0; i < pooledStructuresWall.Count; i++) { if (pooledStructuresWall[i].transform.gameObject.active) { pooledStructures.Add(pooledStructuresWall[i]); } }
            for (int i = 0; i < pooledStructuresCube.Count; i++) { if (pooledStructuresCube[i].transform.gameObject.active) { pooledStructures.Add(pooledStructuresCube[i]); } }
            for (int i = 0; i < pooledStructuresPillar.Count; i++) if (pooledStructuresPillar[i].transform.gameObject.active) { { pooledStructures.Add(pooledStructuresPillar[i]); } }
            for (int i = 0; i < pooledStructuresBall.Count; i++) if (pooledStructuresBall[i].transform.gameObject.active) { { pooledStructures.Add(pooledStructuresBall[i]); } }
            for (int i = 0; i < pooledStructuresDisc.Count; i++) if (pooledStructuresDisc[i].transform.gameObject.active) { { pooledStructures.Add(pooledStructuresDisc[i]); } }
            for (int i = 0; i < pooledStructuresBoulder.Count; i++) if (pooledStructuresBoulder[i].transform.gameObject.active) { { pooledStructures.Add(pooledStructuresBoulder[i]); } }
            for (int i = 0; i < pooledStructuresSmallRock.Count; i++) if (pooledStructuresSmallRock[i].transform.gameObject.active) { { pooledStructures.Add(pooledStructuresSmallRock[i]); } }
            for (int i = 0; i < pooledStructuresTetherBall.Count; i++) if (pooledStructuresTetherBall[i].transform.gameObject.active) { { pooledStructures.Add(pooledStructuresTetherBall[i]); } }
            //for each structure
            for (int i = 0; i < pooledStructures.Count; i++)
            {
                GameObject structure = pooledStructures[i].transform.gameObject;
                float distence = Vector2.Distance(structure.transform.position, playerPos.position);
                float heightDifference = Vector2.Distance(new Vector2(0, structure.transform.position.y), new Vector2(0, playerPos.position.y));
                //if within 2.5 distance on Y axis, and x/z Distance is within 2.5 (Position Check)
                if ((Vector2.Distance(new Vector2(structure.transform.position.x, structure.transform.position.z), new Vector2(playerPos.transform.position.x, playerPos.transform.position.z)) < 2.5f) && (heightDifference < 2.5f))
                {
                    Vector2 forwardVector = new Vector2(playerPos.forward.normalized.x, playerPos.forward.normalized.z);
                    Vector2 directionToObject = new Vector2((structure.transform.position - playerPos.position).normalized.x, (structure.transform.position - playerPos.position).normalized.z);
                    float dotProduct = Vector2.Dot(forwardVector, directionToObject);
                    float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg; // Convert radians to degrees
                    //if within the 180 degree FOV (Rotation Check)
                    if (angle <= 90f) //90f = 180 fov
                    {
                        structuresWithinZOM.Add(structure);
                        //if within 60 degree FOV (Looking At Check)
                        if (angle <= 30f) //30f = 60 fov
                        {
                            lookingAtStructureGOs.Add(structure);
                            lookingAtStructureDistances.Add(distence);
                        }
                    }
                }
            }
            //if at least 1 Structure in Range
            if (structuresWithinZOM.Count > 0)
            {
                //if looking at at least 1 structure
                if (lookingAtStructureGOs.Count > 0)
                {
                    int smallestDistanceSpot = -1;
                    float smallestDistance = 2.6f;
                    for (int i = 0; i < lookingAtStructureGOs.Count; i++)
                    {
                        //if it's closer than the closest on x/z Axis, store as new closest
                        if (lookingAtStructureDistances[i] < smallestDistance)
                        {
                            smallestDistanceSpot = i;
                            smallestDistance = lookingAtStructureDistances[i];
                        }
                    }
                    //return the Closest of the Looked at Structures
                    return lookingAtStructureGOs[smallestDistanceSpot];
                }
                //if not looking at a Structure, but at least 1 within side of View
                else
                {
                    int closest = -1;
                    float closestDistance = 2.6f;
                    for (int i = 0; i < structuresWithinZOM.Count; i++)
                    {
                        //if it's closer than the closest on x/z Axis, store as new closest
                        float distance = Vector2.Distance(new Vector2(structuresWithinZOM[i].transform.position.x, structuresWithinZOM[i].transform.position.z), new Vector2(playerPos.transform.position.x, playerPos.transform.position.z));
                        if (distance < closestDistance)
                        {
                            closest = i;
                            closestDistance = distance;
                        }
                    }
                    //return the closest of the structures within ZOM
                    return structuresWithinZOM[closest];
                }
            }
            //return null if No Structure in Range
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

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Loader")
            {
                //store pools
                pooledStructuresWall = PoolManager.instance.availablePools[PoolManager.instance.GetPoolIndex("Wall")].PooledObjects;
                pooledStructuresCube = PoolManager.instance.availablePools[PoolManager.instance.GetPoolIndex("RockCube")].PooledObjects;
                pooledStructuresPillar = PoolManager.instance.availablePools[PoolManager.instance.GetPoolIndex("Pillar")].PooledObjects;
                pooledStructuresBall = PoolManager.instance.availablePools[PoolManager.instance.GetPoolIndex("Ball")].PooledObjects;
                pooledStructuresDisc = PoolManager.instance.availablePools[PoolManager.instance.GetPoolIndex("Disc")].PooledObjects;
                pooledStructuresBoulder = PoolManager.instance.availablePools[PoolManager.instance.GetPoolIndex("LargeRock")].PooledObjects;
                pooledStructuresSmallRock = PoolManager.instance.availablePools[PoolManager.instance.GetPoolIndex("SmallRock")].PooledObjects;
                pooledStructuresTetherBall = PoolManager.instance.availablePools[PoolManager.instance.GetPoolIndex("BoulderBall")].PooledObjects;
            }
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
