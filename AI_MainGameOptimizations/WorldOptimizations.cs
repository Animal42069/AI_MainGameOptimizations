using System;
using UnityEngine;

namespace AI_MainGameOptimizations
{
    class WorldOptimizations
    {
        private static Terrain terrain;
        private static bool isHousingIsland;

        public static void InitializeWorldOptimizations(float detailDistance, bool castShadows, GameLayers.LayerStrategy layerStrategy, bool drawTrees, LightShadows spotLightShadows, LightShadows pointLightShadows, bool enableCityPointLights, float citySpotLightIntensity, AnimatorCullingMode cullingMode)
        {
            terrain = GetTerrain();

            if (terrain == null)
                return;

            SetTerrainDetailDistance(detailDistance);
            SetTerrainShadows(castShadows);
            EnableTreeRendering(drawTrees);

            AdjustTerrainLayer(layerStrategy);
            AdjustCullingMasks(layerStrategy);

            SetCityLightShadows(spotLightShadows, pointLightShadows);
            EnableCityPointLights(enableCityPointLights);
            SetCitySpotLightIntensity(citySpotLightIntensity);

            UpdateAnimatorCulling(cullingMode);
        }

        public static void DestroyOptimizers()
        {
            MoveTerrainToLayer((int)GameLayers.Layer.MapLayer);
            EnableTreeRendering(true);

            isHousingIsland = false;
            terrain = null;
        }

        private static Terrain GetTerrain()
        {
            isHousingIsland = false;
            var map = GameObject.Find("map00_Beach/map00_terrain_data/map00_terrain");
            if (map == null)
            {
                map = GameObject.Find("map_01_data/terrin_island_medium");
                isHousingIsland = true;
            }

            if (map == null)
                return null;

            return map.GetComponent<Terrain>();
        }

        public static void SetTerrainDetailDistance(float detailDistance)
        {
            if (terrain == null)
                return;

            terrain.basemapDistance = detailDistance;
        }

        public static void SetTerrainShadows(bool castShadows)
        {
            if (terrain == null)
                return;

            terrain.castShadows = castShadows;
        }

        public static void EnableTreeRendering(bool enable)
        {
            if (terrain == null)
                return;

            terrain.drawTreesAndFoliage = enable;
        }

        public static void AdjustTerrainLayer(GameLayers.LayerStrategy layerStrategy)
        {
            if (isHousingIsland)
                return;

            if (layerStrategy == GameLayers.LayerStrategy.MultiLayer)
                MoveTerrainToLayer((int)GameLayers.Layer.LargeObjectLayer);
            else if (layerStrategy == GameLayers.LayerStrategy.SingleLayer)
                MoveTerrainToLayer((int)GameLayers.Layer.DefaultLayer);
            else
                MoveTerrainToLayer((int)GameLayers.Layer.MapLayer);
        }

        public static void AdjustCullingMasks(GameLayers.LayerStrategy layerStrategy)
        {
            if (layerStrategy == GameLayers.LayerStrategy.MultiLayer)
            {
                AddLayerMaskToCullingMasks(GameLayers.LayerMask.MediumObjectLayer | 
                                           GameLayers.LayerMask.SmallObjectLayer | 
                                           GameLayers.LayerMask.LargeObjectLayer);
            }
            else if (layerStrategy == GameLayers.LayerStrategy.SingleLayer)
            {
                AddLayerMaskToCullingMasks(GameLayers.LayerMask.DefaultLayer);
            }
        }

        public static void MoveTerrainToLayer(int moveToLayer)
        {
            if (terrain == null || isHousingIsland)
                return;

            terrain.gameObject.layer = moveToLayer;
        }

        public static void SetCityLightShadows(LightShadows spotLightShadows, LightShadows pointLightShadows)
        {
            if (isHousingIsland)
                return;

            var lights = GetCityLights();
            if (lights.IsNullOrEmpty())
                return;

            foreach (var light in lights)
            {
                if (light.type == LightType.Spot)
                    light.shadows = spotLightShadows;

                if (light.type == LightType.Point)
                    light.shadows = pointLightShadows;
            }
        }

        public static void MoveLightsToTerrainLayer(bool toTerrainLayer)
        {
            if (isHousingIsland)
                return;

            if (toTerrainLayer)
                MoveCityLightsToLayer(GameLayers.Layer.LargeObjectLayer);
            else
                MoveCityLightsToLayer(GameLayers.Layer.MapLayer);
        }

        public static void MoveCityLightsToLayer(GameLayers.Layer moveToLayer)
        {
            if (isHousingIsland)
                return;

            var lights = GetCityLights();
            if (lights.IsNullOrEmpty())
                return;

            foreach (var light in lights)
                light.gameObject.layer = (int)moveToLayer;
        }

        public static void AddLayerMaskToCullingMasks(GameLayers.LayerMask newLayerMask)
        {
            Camera[] cameras = Resources.FindObjectsOfTypeAll<Camera>();
            if (cameras == null)
                return;

            foreach (var camera in cameras)
            {
                if ((camera.cullingMask & (int)GameLayers.LayerMask.MapLayer) == (int)GameLayers.LayerMask.MapLayer)
                    camera.cullingMask |= (int)newLayerMask;
            }

            Light[] lights = Resources.FindObjectsOfTypeAll<Light>();
            if (lights.IsNullOrEmpty())
                return;

            foreach (var light in lights)
            {
                if ((light.cullingMask & (int)GameLayers.LayerMask.MapLayer) == (int)GameLayers.LayerMask.MapLayer)
                    light.cullingMask |= (int)newLayerMask;
            }
        }

        public static void RemoveLayerMaskFromCullingMasks(GameLayers.LayerMask layerMask)
        {
            Camera[] cameras = Resources.FindObjectsOfTypeAll<Camera>();
            if (cameras == null)
                return;

            foreach (var camera in cameras)
            {
                if ((camera.cullingMask & (int)GameLayers.LayerMask.MapLayer) == (int)GameLayers.LayerMask.MapLayer)
                    camera.cullingMask &= ~(int)layerMask;
            }

            Light[] lights = Resources.FindObjectsOfTypeAll<Light>();
            if (lights.IsNullOrEmpty())
                return;

            foreach (var light in lights)
            {
                if ((light.cullingMask & (int)GameLayers.LayerMask.MapLayer) == (int)GameLayers.LayerMask.MapLayer)
                    light.cullingMask &= ~(int)layerMask;
            }
        }

        public static void EnableCityPointLights(bool bEnable)
        {
            if (isHousingIsland)
                return;

            var lights = GetCityLights();
            if (lights.IsNullOrEmpty())
                return;

            foreach (var light in lights)
            {
                if (light.type == LightType.Point)
                    light.gameObject.SetActive(bEnable);
            }
        }

        public static void SetCitySpotLightIntensity(float intensity)
        {
            if (isHousingIsland)
                return;

            var lights = GetCityLights();
            if (lights.IsNullOrEmpty())
                return;

            foreach (var light in lights)
            {
                if (light.type == LightType.Spot)
                    light.intensity = intensity;
            }
        }

        private static Light[] GetCityLights()
        {
            var lightData = GameObject.Find("map00_Beach/map00_effect_data/light_data/p_ai_mi_citylight00_gp");

            if (lightData == null)
                return null;

            return lightData.GetComponentsInChildren<Light>(true);
        }

        public static void UpdateAnimatorCulling(AnimatorCullingMode cullingMode)
        {
            var worldData = GameObject.Find("map00_Beach");

            if (worldData == null)
                return;

            var worldAnimators = worldData.GetComponentsInChildren<Animator>(true);

            foreach (var animator in worldAnimators)
                animator.cullingMode = cullingMode;
        }
    }
}
