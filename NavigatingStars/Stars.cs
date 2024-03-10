using GameNetcodeStuff;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace LethalCompany.Mods
{
    public class PathDrawer : MonoBehaviour
    {
        // Private variables to store references and state
        private LineRenderer lineRenderer;
        private GameObject[] allAINodes;
        private Key toggleKey;
        private Vector3 mainEntrancePosition;
        private Vector3 previousNearestNodePosition;
        private GameObject player;
        private bool isLineDrawn = false;
        private Coroutine drawLineCoroutine;

        private void Start()
        {
            // Initialize the line renderer component
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = PluginConfig.LineColor.Value;
            lineRenderer.endColor = PluginConfig.LineColor.Value;
            lineRenderer.startWidth = PluginConfig.LineWidth.Value;
            lineRenderer.endWidth = PluginConfig.LineWidth.Value;
            lineRenderer.enabled = false;

            // Get the toggle key from the plugin configuration
            toggleKey = PluginConfig.ToggleKey.Value;

            // Find the player object
            FindPlayer();
        }

        private void FindPlayer()
        {
            // Find the local player controller
            PlayerControllerB localPlayer = StartOfRound.Instance?.localPlayerController;
            if (localPlayer != null)
            {
                // Store the player object and create a NavMeshAgent component
                player = localPlayer.gameObject;
                CreateNavMeshAgent();
                Debug.Log("Player found: " + player.name);
            }
            else
            {
                Debug.LogWarning("Player not found!");
            }
        }

        private void CreateNavMeshAgent()
        {
            // Create a new GameObject and add a NavMeshAgent component to it
            GameObject navMeshAgentObject = new GameObject("NavMeshAgent");
            navMeshAgentObject.transform.SetParent(player.transform);
            navMeshAgentObject.AddComponent<NavMeshAgent>();
        }

        private void Update()
        {
            // Check if the line is drawn
            if (isLineDrawn)
            {
                // Calculate the distance between the player and the end node
                float distanceToEndNode = Vector3.Distance(player.transform.position, mainEntrancePosition);
                if (distanceToEndNode <= PluginConfig.AutoClearDistance.Value)
                {
                    // Clear the line and enable the NavMeshAgent if the player is close to the end node
                    ClearLine();
                    NavMeshAgent navMeshAgent = player.GetComponentInChildren<NavMeshAgent>();
                    if (navMeshAgent != null)
                    {
                        navMeshAgent.enabled = true;
                    }
                    isLineDrawn = false;
                }
                else if (PluginConfig.DynamicLineRedraw.Value)
                {
                    // Update the line segments dynamically if enabled in the plugin configuration
                    UpdateLineSegments();
                }
            }

            // Check if the toggle key is pressed
            if (Keyboard.current[toggleKey].wasPressedThisFrame)
            {
                Debug.Log("Toggle key pressed!");

                // Find the local player controller
                PlayerControllerB localPlayer = StartOfRound.Instance?.localPlayerController;
                if (localPlayer != null)
                {
                    // Store the player object
                    player = localPlayer.gameObject;

                    // Get the NavMeshAgent component or create a new one if it doesn't exist
                    NavMeshAgent navMeshAgent = player.GetComponentInChildren<NavMeshAgent>();
                    if (navMeshAgent == null)
                    {
                        CreateNavMeshAgent();
                        navMeshAgent = player.GetComponentInChildren<NavMeshAgent>();
                    }

                    // Toggle the line drawing
                    if (isLineDrawn)
                    {
                        // Clear the line and enable the NavMeshAgent if the line is already drawn
                        ClearLine();
                        navMeshAgent.enabled = true;
                        isLineDrawn = false;
                    }
                    else
                    {
                        // Update the AI nodes reference based on the player's location
                        UpdateAINodesReference();

                        // Find the nearest AI node position to the player
                        Vector3 playerPosition = player.transform.position;
                        Vector3 nearestNodePosition = FindNearestAINodePosition(playerPosition);

                        // Get the main entrance position using the RoundManager
                        mainEntrancePosition = RoundManager.Instance.GetNavMeshPosition(RoundManager.FindMainEntrancePosition(true, IsPlayerOutside()), default(NavMeshHit), 5f, -1);
                        Debug.Log("Main Entrance Position: " + mainEntrancePosition);

                        // Calculate the path from the nearest node to the main entrance
                        NavMeshPath path = new NavMeshPath();
                        bool pathFound = NavMesh.CalculatePath(nearestNodePosition, mainEntrancePosition, NavMesh.AllAreas, path);

                        if (pathFound)
                        {
                            // Stop any previous line drawing coroutine
                            if (drawLineCoroutine != null)
                            {
                                StopCoroutine(drawLineCoroutine);
                            }

                            // Start the line drawing animation coroutine
                            drawLineCoroutine = StartCoroutine(DrawLineAnimation(path));
                            isLineDrawn = true;
                            lineRenderer.enabled = true;
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

        private IEnumerator DrawLineAnimation(NavMeshPath path)
        {
            // Add the player's position as the starting point of the line
            Vector3 playerPosition = player.transform.position;
            Vector3[] pathCorners = new Vector3[path.corners.Length + 1];
            pathCorners[0] = playerPosition;
            path.corners.CopyTo(pathCorners, 1);

            // Calculate the animation duration based on the number of segments
            float segmentDuration = 0.1f; // Duration for each segment
            float animationDuration = (pathCorners.Length - 1) * segmentDuration;

            float elapsedTime = 0f;

            // Set the initial line segment from the player to the starting AI node
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, playerPosition);
            lineRenderer.SetPosition(1, pathCorners[1]);
            lineRenderer.enabled = true;

            // Draw the lines between the AI nodes
            while (elapsedTime < animationDuration)
            {
                float t = elapsedTime / animationDuration;
                float easedT = EaseInOutCubic(t); // Apply an easing function to the progress

                int currentSegmentCount = Mathf.RoundToInt(easedT * (pathCorners.Length - 1)) + 1;
                currentSegmentCount = Mathf.Clamp(currentSegmentCount, 2, pathCorners.Length);

                lineRenderer.positionCount = currentSegmentCount;

                for (int i = 2; i < currentSegmentCount; i++)
                {
                    lineRenderer.SetPosition(i, pathCorners[i]);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Set the final positions to be the corners of the path, including the player's position
            lineRenderer.positionCount = pathCorners.Length;
            for (int i = 0; i < pathCorners.Length; i++)
            {
                lineRenderer.SetPosition(i, pathCorners[i]);
            }

            // Store the last position as the end node position only if dynamic line redraw is disabled
            if (!PluginConfig.DynamicLineRedraw.Value)
            {
                mainEntrancePosition = pathCorners[pathCorners.Length - 1];
            }

            // Simplify the line to reduce the number of vertices and create a smoother appearance
            lineRenderer.Simplify(0.1f); // Adjust the tolerance value as needed
        }

        private float EaseInOutCubic(float t)
        {
            // This method implements an easing function using a cubic curve.
            // It takes a value 't' between 0 and 1 and returns the eased value.
            // The easing function is used to create smooth transitions.
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        private bool IsPlayerOutside()
        {
            // This method checks if the player is outside the factory.
            // It retrieves the local player controller from the StartOfRound instance.
            // If the player controller is found and the player is not inside the factory,
            // it returns true. Otherwise, it returns false.
            PlayerControllerB localPlayer = StartOfRound.Instance?.localPlayerController;
            if (localPlayer != null)
            {
                return !localPlayer.isInsideFactory;
            }
            return false;
        }

        private void UpdateAINodesReference()
        {
            // This method updates the reference to the AI nodes based on the player's location.
            // If the player is outside, it finds all game objects with the tag "OutsideAINode".
            // If the player is inside, it finds all game objects with the tag "AINode".
            // The found AI nodes are stored in the 'allAINodes' array.
            // It also logs the number of AI nodes found.
            if (IsPlayerOutside())
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

        private Vector3 FindNearestAINodePosition(Vector3 position)
        {
            // This method finds the nearest AI node position to the given position.
            // It iterates through all the AI nodes and calculates the distance between
            // the given position and each node position (ignoring the y-coordinate).
            // It keeps track of the nearest node position and distance.
            // Finally, it returns the position of the nearest AI node.
            Vector3 nearestNodePosition = Vector3.zero;
            float nearestDistance = float.MaxValue;

            foreach (GameObject node in allAINodes)
            {
                Vector3 nodePosition = node.transform.position;
                float distance = Vector3.Distance(new Vector3(position.x, 0f, position.z), new Vector3(nodePosition.x, 0f, nodePosition.z));
                if (distance < nearestDistance)
                {
                    nearestNodePosition = nodePosition;
                    nearestDistance = distance;
                }
            }

            return nearestNodePosition;
        }

        private void UpdateLineSegments()
        {
            // This method updates the line segments based on the player's position and the main entrance position.
            // It is called when the line is drawn ('isLineDrawn' is true).
            // It finds the nearest AI node position to the player's position.
            // If the nearest node position has changed, it logs the new position.
            // It then calculates a path from the nearest node position to the main entrance position using NavMesh.
            // If a path is found, it adds the player's position as the starting point and updates the line renderer positions.
            // Finally, it simplifies the line to reduce the number of vertices and create a smoother appearance.
            if (isLineDrawn)
            {
                Vector3 playerPosition = player.transform.position;
                Vector3 nearestNodePosition = FindNearestAINodePosition(playerPosition);

                if (nearestNodePosition != previousNearestNodePosition)
                {
                    Debug.Log("Nearest AI Node Position: " + nearestNodePosition);
                    previousNearestNodePosition = nearestNodePosition;
                }

                NavMeshPath path = new NavMeshPath();
                bool pathFound = NavMesh.CalculatePath(nearestNodePosition, mainEntrancePosition, NavMesh.AllAreas, path);

                if (pathFound)
                {
                    // Add the player's position as the starting point of the line
                    Vector3[] pathCorners = new Vector3[path.corners.Length + 1];
                    pathCorners[0] = playerPosition;
                    path.corners.CopyTo(pathCorners, 1);

                    // Update the line renderer positions
                    lineRenderer.positionCount = pathCorners.Length;
                    for (int i = 0; i < pathCorners.Length; i++)
                    {
                        lineRenderer.SetPosition(i, pathCorners[i]);
                    }

                    // Simplify the line to reduce the number of vertices and create a smoother appearance
                    lineRenderer.Simplify(0.1f); // Adjust the tolerance value as needed
                }
            }
        }

        public void ClearLine()
        {
            // This method clears the line renderer and resets related variables.
            // It stops the 'drawLineCoroutine' if it is running.
            // If dynamic line redrawing is disabled in the plugin configuration,
            // it resets the 'mainEntrancePosition' to zero.
            // It sets the line renderer's position count to zero, disables the line renderer,
            // and resets the 'mainEntrancePosition' and 'previousNearestNodePosition' to zero.
            if (drawLineCoroutine != null)
            {
                StopCoroutine(drawLineCoroutine);
                drawLineCoroutine = null;
            }
            if (!PluginConfig.DynamicLineRedraw.Value)
            {
                mainEntrancePosition = Vector3.zero;
            }
            lineRenderer.positionCount = 0;
            mainEntrancePosition = Vector3.zero;
            previousNearestNodePosition = Vector3.zero;
            lineRenderer.enabled = false;
        }

        private void OnDestroy()
        {
            // This method is called when the script instance is being destroyed.
            // It cleans up the line renderer component by destroying it.
            Destroy(lineRenderer);
        }
    }
}