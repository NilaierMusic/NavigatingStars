using GameNetcodeStuff;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using System.Reflection;

namespace LethalCompany.Mods
{
    public class PathDrawer : MonoBehaviour
    {
        private SoundManager? soundManager;
        private LineRenderer? lineRenderer;
        private Key toggleKey;
        private Vector3 mainEntrancePosition;
        private Vector3 previousNearestNodePosition;
        private GameObject? playerObject;
        private const float SimplifyTolerance = 0.01f;
        private bool isLineDrawn = false;
        private Coroutine? drawLineCoroutine;
        private NavMeshManager? navMeshManager;
        private AINodeManager? aiNodeManager;

        private void Start()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"))
            {
                color = PluginConfig.LineColor.Value
            };
            lineRenderer.startColor = PluginConfig.LineColor.Value;
            lineRenderer.endColor = PluginConfig.LineColor.Value;
            lineRenderer.startWidth = PluginConfig.LineWidth.Value;
            lineRenderer.endWidth = PluginConfig.LineWidth.Value;
            lineRenderer.enabled = false;
            toggleKey = PluginConfig.ToggleKey.Value;
            navMeshManager = new NavMeshManager();
            aiNodeManager = new AINodeManager();
            soundManager = FindObjectOfType<SoundManager>();
            if (soundManager == null)
            {
                GameObject soundManagerObj = new GameObject("SoundManager");
                soundManager = soundManagerObj.AddComponent<SoundManager>();
            }
            // Configure sound settings based on configuration values
            if (soundManager != null)
            {
                soundManager.EnableSoundEffects = PluginConfig.EnableSoundEffects.Value;
                soundManager.SoundEffectVolume = PluginConfig.SoundEffectVolume.Value;
                soundManager.Enable3DSound = PluginConfig.Enable3DSound.Value;
            }
        }

        private void Update()
        {
            var localPlayerController = StartOfRound.Instance?.localPlayerController;
            bool isPlayerOutside = localPlayerController != null && !localPlayerController.isInsideFactory;

            // Update the AI nodes reference based on the player's location
            aiNodeManager.UpdateAINodesReference(isPlayerOutside);

            if (isLineDrawn)
            {
                float distanceToEndNodeSqr = Vector3.Distance(playerObject.transform.position, mainEntrancePosition);
                if (distanceToEndNodeSqr <= PluginConfig.AutoClearDistance.Value * PluginConfig.AutoClearDistance.Value)
                {
                    ClearLine();
                    navMeshManager.EnableNavMeshAgent();
                    isLineDrawn = false;
                }
                else if (PluginConfig.DynamicLineRedraw.Value)
                {
                    UpdateLineSegments();
                }
            }

            if (Keyboard.current[toggleKey].wasPressedThisFrame)
            {
                Debug.Log("Toggle key pressed!");
                // Move the player finding logic to the Update method
                playerObject = navMeshManager.FindPlayer();

                if (playerObject == null)
                {
                    return; // Exit the Update method if the player is not found
                }

                if (playerObject != null)
                {
                    if (isLineDrawn)
                    {
                        ClearLine();
                        navMeshManager.EnableNavMeshAgent();
                        isLineDrawn = false;
                    }
                    else
                    {
                        isPlayerOutside = IsPlayerOutside();
                        Vector3 playerPosition = playerObject.transform.position;
                        mainEntrancePosition = RoundManager.Instance.GetNavMeshPosition(RoundManager.FindMainEntrancePosition(true, isPlayerOutside), default(NavMeshHit), 5f, -1);
                        Debug.Log($"Main Entrance Position: {mainEntrancePosition}");
                        NavMeshPath path = new NavMeshPath();
                        bool pathFound = NavMesh.CalculatePath(playerPosition, mainEntrancePosition, NavMesh.AllAreas, path);
                        if (pathFound)
                        {
                            if (drawLineCoroutine != null)
                            {
                                StopCoroutine(drawLineCoroutine);
                            }
                            drawLineCoroutine = StartCoroutine(DrawLineAnimation(path));
                            isLineDrawn = true;
                            lineRenderer.enabled = true;
                            navMeshManager.DisableNavMeshAgent();
                            Debug.Log("Line renderer enabled.");
                        }
                        else
                        {
                            Debug.LogWarning("No valid path found!");

                            // Display a tip when no valid path is found
                            HUDManager hudManager = FindObjectOfType<HUDManager>();
                            if (hudManager != null)
                            {
                                hudManager.DisplayTip("Path Not Found", "No valid path to the main entrance was found. Please try again or contact support.", true);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Player not found!");
                }
            }
        }

        private IEnumerator DrawLineAnimation(NavMeshPath path)
        {
            // Check if the player object is null before using it
            if (playerObject == null)
            {
                Debug.LogWarning("Player not found!");
                yield break; // Exit the coroutine if the player is not found
            }

            Vector3 playerPosition = playerObject.transform.position;
            Vector3[] pathCorners = path.corners;
            float segmentDuration = 0.1f;
            float animationDuration = (pathCorners.Length - 1) * segmentDuration;
            float elapsedTime = 0f;
            lineRenderer.positionCount = pathCorners.Length + 1;
            lineRenderer.SetPosition(0, playerPosition);
            lineRenderer.SetPositions(pathCorners);
            lineRenderer.enabled = true;

            // Draw the first segment immediately
            Vector3[] currentPositions = new Vector3[2];
            currentPositions[0] = playerPosition;
            currentPositions[1] = pathCorners[0];
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(currentPositions);

            int currentSegmentCount = 1;

            while (elapsedTime < animationDuration)
            {
                float t = elapsedTime / animationDuration;
                float easedT = EaseInOutCubic(t);
                int newSegmentCount = Mathf.RoundToInt(easedT * (pathCorners.Length - 1)) + 1;
                newSegmentCount = Mathf.Clamp(newSegmentCount, 2, pathCorners.Length + 1);

                if (newSegmentCount != currentSegmentCount)
                {
                    currentSegmentCount = newSegmentCount;
                    currentPositions = new Vector3[currentSegmentCount + 1];
                    currentPositions[0] = playerPosition;
                    Array.Copy(pathCorners, 0, currentPositions, 1, currentSegmentCount);
                    lineRenderer.positionCount = currentSegmentCount + 1;
                    lineRenderer.SetPositions(currentPositions);

                    // Play the sound effect at the current segment position starting from the second segment
                    if (currentSegmentCount > 1 && PluginConfig.EnableSoundEffects.Value)
                    {
                        Vector3 segmentPosition = currentPositions[currentSegmentCount];
                        soundManager.PlaySound(segmentPosition);
                    }
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (!PluginConfig.DynamicLineRedraw.Value)
            {
                mainEntrancePosition = pathCorners[pathCorners.Length - 1];
            }
            lineRenderer.Simplify(SimplifyTolerance);
        }

        private float EaseInOutCubic(float t)
        {
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        private bool IsPlayerOutside()
        {
            var localPlayer = StartOfRound.Instance?.localPlayerController;
            return localPlayer != null && !localPlayer.isInsideFactory;
        }

        private void UpdateLineSegments()
        {
            if (isLineDrawn)
            {
                // Check if the player object is null before using it
                if (playerObject == null)
                {
                    Debug.LogWarning("Player not found!");
                    return;
                }

                Vector3 playerPosition = playerObject.transform.position;
                Vector3 nearestNodePosition = aiNodeManager.FindNearestAINodePosition(playerPosition);

                if (nearestNodePosition != previousNearestNodePosition)
                {
                    Debug.Log("Nearest AI Node Position: " + nearestNodePosition);
                    previousNearestNodePosition = nearestNodePosition;
                }
                NavMeshPath path = new NavMeshPath();
                bool pathFound = NavMesh.CalculatePath(nearestNodePosition, mainEntrancePosition, NavMesh.AllAreas, path);
                if (pathFound)
                {
                    Vector3[] pathCorners = new Vector3[lineRenderer.positionCount];

                    // Add error handling for path finding
                    if (pathCorners == null || pathCorners.Length == 0)
                    {
                        Debug.LogError("Invalid path corners found!");
                        return;
                    }

                    int pathCornerCount = path.GetCornersNonAlloc(pathCorners);
                    lineRenderer.positionCount = pathCornerCount + 1;
                    lineRenderer.SetPosition(0, playerPosition);
                    for (int i = 0; i < pathCornerCount; i++)
                    {
                        lineRenderer.SetPosition(i + 1, pathCorners[i]);
                    }
                    lineRenderer.Simplify(SimplifyTolerance);
                }
                else
                {
                    Debug.LogWarning("No valid path found!");
                }
            }
        }

        public void ClearLine()
        {
            if (drawLineCoroutine != null)
            {
                StopCoroutine(drawLineCoroutine);
                drawLineCoroutine = null;
            }
            lineRenderer.positionCount = 0;
            mainEntrancePosition = Vector3.zero;
            previousNearestNodePosition = Vector3.zero;
            lineRenderer.enabled = false;
        }
    }

    public class SoundManager : MonoBehaviour
    {
        public static SoundManager? Instance { get; private set; }

        private AudioSource? audioSource;
        private AudioClip? lineDrawingSound;

        public bool EnableSoundEffects { get; set; }
        public float SoundEffectVolume { get; set; }
        public bool Enable3DSound { get; set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            audioSource = gameObject.AddComponent<AudioSource>();
            LoadAudioResources();
        }

        private void LoadAudioResources()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("NavigatingStars.Resources.line_drawing_sound.wav"))
            {
                if (stream != null)
                {
                    int samples = (int)(stream.Length / 2);
                    float[] samplesData = new float[samples];

                    byte[] buffer = new byte[2];
                    for (int i = 0; i < samples; i++)
                    {
                        stream.Read(buffer, 0, 2);
                        short sample = BitConverter.ToInt16(buffer, 0);
                        samplesData[i] = sample / 32768.0f;
                    }

                    lineDrawingSound = AudioClip.Create("LineDrawingSound", samples, 1, 22050, false);
                    lineDrawingSound.SetData(samplesData, 0);
                }
                else
                {
                    Debug.LogError("Failed to load line drawing sound resource.");
                }
            }
        }

        private AudioClip CreateAudioClip(byte[] audioData, string name)
        {
            // Calculate the number of samples and channels from the audio data
            int channels = 1; // Assuming mono audio
            int freq = 22050; // Assuming 22050 Hz sample rate
            int samples = audioData.Length / 2; // 16-bit audio, 2 bytes per sample

            // Create the AudioClip with the calculated parameters
            AudioClip clip = AudioClip.Create(name, samples, channels, freq, false);

            // Convert byte[] to float[]
            float[] samplesData = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(audioData, i * 2);
                samplesData[i] = sample / 32768.0f;
            }

            // Set the audio data in the AudioClip
            clip.SetData(samplesData, 0);

            return clip;
        }

        public void PlaySound(Vector3 position)
        {
            if (EnableSoundEffects && lineDrawingSound != null)
            {
                audioSource.Stop();
                audioSource.clip = lineDrawingSound;
                audioSource.spatialBlend = Enable3DSound ? 1f : 0f;
                audioSource.volume = SoundEffectVolume;
                audioSource.transform.position = position;
                audioSource.Play();
            }
        }
    }

    public class NavMeshManager
    {
        private NavMeshAgent? navMeshAgent;

        public GameObject? FindPlayer()
        {
            var localPlayer = StartOfRound.Instance?.localPlayerController;
            if (localPlayer != null)
            {
                GameObject playerObject = localPlayer.gameObject;
                navMeshAgent = playerObject.GetComponentInChildren<NavMeshAgent>();
                if (navMeshAgent == null)
                {
                    CreateNavMeshAgent(playerObject);
                }
                Debug.Log("Player found: " + playerObject.name);
                return playerObject;
            }
            else
            {
                Debug.LogWarning("Player not found!");
                return null;
            }
        }

        private void CreateNavMeshAgent(GameObject playerObject)
        {
            GameObject navMeshAgentObject = new GameObject("NavMeshAgent");
            navMeshAgentObject.transform.SetParent(playerObject.transform);
            navMeshAgent = navMeshAgentObject.AddComponent<NavMeshAgent>();
        }

        public void EnableNavMeshAgent()
        {
            navMeshAgent.enabled = true;
        }

        public void DisableNavMeshAgent()
        {
            navMeshAgent.enabled = false;
        }
    }

    public class AINodeManager
    {
        private GameObject[] insideAINodes;
        private GameObject[] outsideAINodes;
        private bool isPlayerOutside;

        public AINodeManager()
        {
            // Load both inside and outside AI nodes at the start
            insideAINodes = GameObject.FindGameObjectsWithTag("AINode");
            outsideAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
            Debug.Log("Inside AI Nodes found: " + insideAINodes.Length);
            Debug.Log("Outside AI Nodes found: " + outsideAINodes.Length);
        }

        public void UpdateAINodesReference(bool isPlayerOutside)
        {
            this.isPlayerOutside = isPlayerOutside;
        }

        public Vector3 FindNearestAINodePosition(Vector3 position)
        {
            GameObject[] aiNodes = isPlayerOutside ? outsideAINodes : insideAINodes;

            if (aiNodes == null || aiNodes.Length == 0)
            {
                // If no AI nodes are found, return the playerObject's position as a fallback
                return position;
            }

            GameObject nearestNode = aiNodes.OrderBy(node => Vector3.Distance(position, node.transform.position)).First();

            return nearestNode.transform.position;
        }
    }
}