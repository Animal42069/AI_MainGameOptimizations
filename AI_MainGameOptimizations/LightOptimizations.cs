using System;
using HarmonyLib;
using UnityEngine;

namespace AI_MainGameOptimizations
{
    public static class LightOptimizations
    {
        private static Light _mainLight;
        private static float[] _shadowCullDistance = new float[32];

        public static void InitializeLighting(Light mainLight, float shadowDistance)
        {
            _mainLight = mainLight;

            for (int layer = 0; layer < _shadowCullDistance.Length; layer++)
                _shadowCullDistance[layer] = shadowDistance;

            UpdateShadowClipPlane(_shadowCullDistance);
        }

        private static void UpdateShadowClipPlane(float[] distances)
        {
            if (_mainLight == null)
                return;

            _shadowCullDistance = distances;
            _mainLight.layerShadowCullDistances = _shadowCullDistance;
        }

        public static void UpdateShadowClipPlane(int layer, float distance)
        {
            if (_mainLight == null || layer >= _shadowCullDistance.Length)
                return;

            _shadowCullDistance[layer] = distance;
            _mainLight.layerShadowCullDistances = _shadowCullDistance;
        }

        public static void AdjustCharacterLighting(GameLayers.LayerStrategy layerStrategy)
        {
            var lights = GameObject.FindObjectsOfType(typeof(Light)) as Light[];

            if (layerStrategy == GameLayers.LayerStrategy.MultiLayer)
            {
                foreach (Light light in lights)
                {
                    if (light == null)
                        continue;

                    if (light.name.Contains("FaceLight"))
                    {
                        light.cullingMask = (int)GameLayers.LayerMask.CharaLayer;
                    }
                    else if (light.name.Contains("Cam Light"))
                    {
                        light.cullingMask = (int)GameLayers.LayerMask.CharaLayer;
                        light.type = LightType.Point;
                        light.intensity = 1.5f;
                        light.range = 20;
                    }
                    else if (light.name.Contains("Back Light"))
                    {
                        light.cullingMask = (int)GameLayers.LayerMask.CharaLayer;
                        light.type = LightType.Spot;
                        light.intensity = 1.5f;
                        light.range = 20;
                        light.spotAngle = 90;
                        light.transform.localEulerAngles = Vector3.zero;
                    }
                    else if (light.name.Contains("Directional Light Dummy"))
                    {
                        light.cullingMask = (int)GameLayers.LayerMask.None;
                    }
                }
            }
            else
            {
                foreach (Light light in lights)
                {
                    if (light == null)
                        continue;

                    if (light.name.Contains("FaceLight"))
                    {
                        light.cullingMask = -956304089;
                    }
                    else if (light.name.Contains("Cam Light"))
                    {
                        light.cullingMask = -529271514;
                        light.type = LightType.Directional;
                        light.intensity = 0.55f;
                        light.range = 29.95f;
                    }
                    else if (light.name.Contains("Back Light"))
                    {
                        light.cullingMask = -462162649;
                        light.type = LightType.Directional;
                        light.intensity = 0.5f;
                        light.range = 10;
                        light.spotAngle = 30;
                        light.transform.localEulerAngles = new Vector3(355.7f, 180f, 0);
                    }
                    else if (light.name.Contains("Directional Light Dummy"))
                    {
                        light.cullingMask = 72053030;
                    }
                }
            }
        }
    }
}
