using System;
using UnityEngine;

namespace AI_MainGameOptimizations
{
    public static class MainLightOptimizations
    {
        private static Light _mainLight;
        private static float[] _shadowCullDistance = new float[32];

        public static void InitializeLighting(Light mainLight, float shadowDistance)
        {
            Console.WriteLine("InitializeLighting");

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
    }
}
