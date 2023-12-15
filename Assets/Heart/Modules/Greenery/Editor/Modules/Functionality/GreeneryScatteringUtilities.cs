using System.Collections.Generic;
using Pancake.Greenery;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    public static class GreeneryScatteringUtilities
    {
        public static int AddSpawnPoint(
            GreeneryManager greeneryManager,
            List<GreeneryItem> selectedItems,
            RaycastHit spawnHit,
            Color surfaceColor,
            float spawnSlopeThreshold,
            float sizeFactor,
            Gradient colorGradient)
        {
            if (Vector3.Dot(spawnHit.normal, new Vector3(0, 1, 0)) >= spawnSlopeThreshold)
            {
                GreeneryItem greeneryItem = selectedItems[UnityEngine.Random.Range(0, selectedItems.Count)];
                return greeneryManager.AddSpawnPoint(greeneryItem,
                new SpawnData(spawnHit.point,
                    spawnHit.normal,
                    surfaceColor,
                    sizeFactor,
                    colorGradient.Evaluate(UnityEngine.Random.value)));
            }

            return -1;
        }

        public static int AddSpawnPoint(
            GreeneryManager greeneryManager,
            List<GreeneryItem> selectedItems,
            RaycastHit spawnHit,
            Color surfaceColor,
            float spawnSlopeThreshold,
            float sizeFactor,
            Gradient colorGradient,
            out GreeneryItem selectedItem)
        {
            selectedItem = selectedItems[UnityEngine.Random.Range(0, selectedItems.Count)];
            if (Vector3.Dot(spawnHit.normal, new Vector3(0, 1, 0)) >= spawnSlopeThreshold)
            {
                return greeneryManager.AddSpawnPoint(selectedItem,
                new SpawnData(spawnHit.point,
                    spawnHit.normal,
                    surfaceColor,
                    sizeFactor,
                    colorGradient.Evaluate(UnityEngine.Random.value)));
            }

            return -1;
        }

        public static int AddSpawnPoint(
            GreeneryManager greeneryManager,
            List<GreeneryItem> selectedItems,
            Vector3 position,
            Vector3 normal,
            Color surfaceColor,
            float sizeFactor,
            Gradient colorGradient)
        {
            GreeneryItem greeneryItem = selectedItems[UnityEngine.Random.Range(0, selectedItems.Count)];
            return greeneryManager.AddSpawnPoint(greeneryItem,
            new SpawnData(position,
                normal,
                surfaceColor,
                sizeFactor,
                colorGradient.Evaluate(UnityEngine.Random.value)));
        }
    }
}