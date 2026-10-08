using Jotunn.Managers;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Processing.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Processing
{
    internal partial class ItemProcessor : DataProcessor<ItemFile>
    {

        private static void LogHierarchy(Transform transform, string indent = "")
        {
            Plugin.LogWarning($"{indent}{transform.name}");
            foreach (var component in transform.GetComponents<Component>())
            {
                Plugin.LogWarning(
                    $"{indent}  [{component.GetType().Name}]"
                );
            }
            foreach (Transform child in transform)
            {
                LogHierarchy(child, indent + "  ");
            }
        }

        private bool ApplyVisualFrom(GameObject item, GameObject visualSource, string model)
        {
            if (!item || !visualSource)
            {
                return false;
            }

            //LogHierarchy(item.transform);
            //LogHierarchy(visualSource.transform);

            var visualName = $"{item.name}_CustomVisual";
            var oldVisuals = GetVisualObjects(item);

            // remember their original active state
            originalVisualStates[item.name] = oldVisuals.ToDictionary(
                visual => visual,
                visual => visual.activeSelf
            );

            var visualRoot = new GameObject(visualName);
            visualRoot.transform.SetParent(item.transform, false);

            // remember what OTAB added
            customVisuals[item.name] = visualRoot;

            foreach (Transform sourceChild in visualSource.transform)
            {
                if (ContainsForbiddenComponents(sourceChild))
                {
                    Plugin.LogDebug($"{model}: Skipping visual child '{sourceChild.name}' because it contains gameplay/network components");
                    continue;
                }

                //Plugin.LogMessage(sourceChild.gameObject.name);
                var clonedChild = UnityEngine.Object.Instantiate(sourceChild.gameObject, visualRoot.transform, false);
                clonedChild.name = sourceChild.name;
                clonedChild.transform.localPosition = sourceChild.localPosition;
                clonedChild.transform.localRotation = sourceChild.localRotation;
                clonedChild.transform.localScale = sourceChild.localScale;
            }

            // disable old visuals
            foreach (var oldVisual in oldVisuals)
            {
                oldVisual.SetActive(false);
            }

            EnsureVisualCollider(item, visualRoot, oldVisuals);

            // get+set new icon for new model
            var itemDrop = item.GetComponent<ItemDrop>();
            var shared = itemDrop.m_itemData.m_shared;
            var icon = RenderManager.Instance.Render(new RenderManager.RenderRequest(item)
            {
                Rotation = RenderManager.IsometricRotation,
                UseCache = false,
                Width = 64,
                Height = 64,
            });
            customIcons.Add(icon);
            shared.m_icons = new[] { icon };

            return true;
        }

        private static List<GameObject> GetVisualObjects(GameObject prefab)
        {
            var result = new List<GameObject>();

            foreach (Transform child in prefab.transform)
            {
                if (
                    child.GetComponentInChildren<Renderer>(true) ||
                    child.GetComponentInChildren<Light>(true) ||
                    child.GetComponentInChildren<ParticleSystem>(true)
                )
                {
                    result.Add(child.gameObject);
                }
            }

            return result;
        }

        private static bool ContainsForbiddenComponents(Transform root)
        {
            return
                root.name == "wetsplsh" ||
                root.GetComponentInChildren<ZNetView>(true) ||
                root.GetComponentInChildren<ItemDrop>(true) ||
                root.GetComponentInChildren<EggGrow>(true) ||
                root.GetComponentInChildren<EggHatch>(true) ||
                root.GetComponentInChildren<Destructible>(true) ||
                root.GetComponentInChildren<DropOnDestroyed>(true);
        }

        private static void EnsureVisualCollider(GameObject item, GameObject visualRoot, List<GameObject> oldVisuals)
        {
            // There is still an active collider -> nothing to do
            if (item.GetComponentsInChildren<Collider>().Any(collider => collider.gameObject.activeInHierarchy))
            {
                return;
            }

            // Find the original collider in the now disabled visual
            BoxCollider originalCollider = oldVisuals
                .SelectMany(visual => visual.GetComponentsInChildren<BoxCollider>(true))
                .FirstOrDefault();

            if (originalCollider == null)
            {
                Plugin.LogWarning($"{item.name}: No collider available for the custom visual");
                return;
            }

            // Find renderers belonging to the new visual.
            // Particle systems must not affect the collider size.
            Renderer[] renderers = visualRoot
                .GetComponentsInChildren<Renderer>(true)
                .Where(renderer => !(renderer is ParticleSystemRenderer))
                .ToArray();

            if (renderers.Length == 0)
            {
                Plugin.LogWarning($"{item.name}: No renderer available to calculate collider bounds");
                return;
            }

            // Calculate world-space bounds of the complete new visual
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            // makes egg not hitable
            visualRoot.layer = originalCollider.gameObject.layer;
            visualRoot.tag = originalCollider.gameObject.tag;

            // Create replacement collider on the active visual root
            BoxCollider collider = visualRoot.AddComponent<BoxCollider>();
            collider.isTrigger = originalCollider.isTrigger;
            collider.sharedMaterial = originalCollider.sharedMaterial;

            // Convert world-space bounds into visualRoot local space
            Vector3 localCenter = visualRoot.transform.InverseTransformPoint(bounds.center);
            Vector3 localMin = visualRoot.transform.InverseTransformPoint(bounds.min);
            Vector3 localMax = visualRoot.transform.InverseTransformPoint(bounds.max);

            collider.center = localCenter;
            collider.size = new Vector3(
                Mathf.Abs(localMax.x - localMin.x),
                Mathf.Abs(localMax.y - localMin.y),
                Mathf.Abs(localMax.z - localMin.z)
            );

            Plugin.LogDebug($"{item.name}: Created BoxCollider for custom visual (center={collider.center}, size={collider.size})");
        }

    }
}