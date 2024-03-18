using GameNetcodeStuff;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace LethalCompany.Mods
{
    public class PathDrawer : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private Key toggleKey;
        private Vector3 mainEntrancePosition;
        private Vector3 previousNearestNodePosition;
        private GameObject playerObject;
        private const float SimplifyTolerance = 0.01f;
        private bool isLineDrawn = false;
        private Coroutine drawLineCoroutine;
        private NavMeshManager navMeshManager;
        private AINodeManager aiNodeManager;

        private void Start()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = PluginConfig.LineColor.Value;
            lineRenderer.endColor = PluginConfig.LineColor.Value;
            lineRenderer.startWidth = PluginConfig.LineWidth.Value;
            lineRenderer.endWidth = PluginConfig.LineWidth.Value;
            lineRenderer.enabled = false;
            toggleKey = PluginConfig.ToggleKey.Value;
            navMeshManager = new NavMeshManager();
            aiNodeManager = new AINodeManager();
        }

        private void Update()
        {
            var localPlayerController = StartOfRound.Instance?.localPlayerController;
            bool isPlayerOutside = IsPlayerOutside(localPlayerController);

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
                        Debug.Log("Main Entrance Position: " + mainEntrancePosition);
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
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Player not found!");
                }
            }
        }

        private bool IsPlayerOutside(PlayerControllerB localPlayerController)
        {
            return localPlayerController != null && !localPlayerController.isInsideFactory;
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

            Vector3[] currentPositions = new Vector3[pathCorners.Length + 1];
            int currentSegmentCount = 0;

            while (elapsedTime < animationDuration)
            {
                float t = elapsedTime / animationDuration;
                float easedT = EaseInOutCubic(t);
                int newSegmentCount = Mathf.RoundToInt(easedT * (pathCorners.Length - 1)) + 1;
                newSegmentCount = Mathf.Clamp(newSegmentCount, 2, pathCorners.Length + 1);

                if (newSegmentCount != currentSegmentCount)
                {
                    currentSegmentCount = newSegmentCount;
                    currentPositions[0] = playerPosition;
                    Array.Copy(pathCorners, 0, currentPositions, 1, currentSegmentCount - 1);
                    lineRenderer.positionCount = currentSegmentCount;
                    lineRenderer.SetPositions(currentPositions);
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

    public class NavMeshManager
    {
        private NavMeshAgent navMeshAgent;

        public GameObject FindPlayer()
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
        private GameObject[] allAINodes;
        private bool isPlayerOutside;

        public void UpdateAINodesReference(bool isPlayerOutside)
        {
            if (this.isPlayerOutside != isPlayerOutside)
            {
                this.isPlayerOutside = isPlayerOutside;
                if (isPlayerOutside)
                {
                    allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                    Debug.Log("Outside AI Nodes found: " + allAINodes.Length);
                }
                else
                {
                    allAINodes = GameObject.FindGameObjectsWithTag("AINode");
                    Debug.Log("Inside AI Nodes found: " + allAINodes.Length);
                }
            }
        }

        public Vector3 FindNearestAINodePosition(Vector3 position)
        {
            if (allAINodes == null || allAINodes.Length == 0)
            {
                // If no AI nodes are found, return the playerObject's position as a fallback
                return position;
            }

            GameObject nearestNode = allAINodes.Aggregate((minNode, node) =>
            {
                float minDistance = Vector3.Distance(position, minNode.transform.position);
                float currentDistance = Vector3.Distance(position, node.transform.position);
                return currentDistance < minDistance ? node : minNode;
            });

            return nearestNode.transform.position;
        }
    }
}