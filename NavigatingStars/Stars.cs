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
        private GameObject player;
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
            player = navMeshManager.FindPlayer();
        }

        private void Update()
        {
            var localPlayerController = StartOfRound.Instance?.localPlayerController;
            bool isPlayerOutside = IsPlayerOutside(localPlayerController);

            if (isLineDrawn)
            {
                float distanceToEndNodeSqr = Vector3.Distance(player.transform.position, mainEntrancePosition);
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
                if (player != null)
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
                        Vector3 playerPosition = player.transform.position;
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
            Vector3 playerPosition = player.transform.position;
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
                Vector3 playerPosition = player.transform.position;
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
                    int pathCornerCount = path.GetCornersNonAlloc(pathCorners);
                    lineRenderer.positionCount = pathCornerCount + 1;
                    lineRenderer.SetPosition(0, playerPosition);
                    for (int i = 0; i < pathCornerCount; i++)
                    {
                        lineRenderer.SetPosition(i + 1, pathCorners[i]);
                    }
                    lineRenderer.Simplify(SimplifyTolerance);
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
                GameObject player = localPlayer.gameObject;
                navMeshAgent = player.GetComponentInChildren<NavMeshAgent>();
                if (navMeshAgent == null)
                {
                    CreateNavMeshAgent(player);
                }
                Debug.Log("Player found: " + player.name);
                return player;
            }
            else
            {
                Debug.LogWarning("Player not found!");
                return null;
            }
        }

        private void CreateNavMeshAgent(GameObject player)
        {
            GameObject navMeshAgentObject = new GameObject("NavMeshAgent");
            navMeshAgentObject.transform.SetParent(player.transform);
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
                // If no AI nodes are found, return the player's position as a fallback
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