using System;
using UnityEngine;

namespace AI_MainGameOptimizations
{
    public static class CameraOptimizations
    {
        private static Camera _playerCamera;
        private static float[] _cameraCullDistances = new float[32];

        public enum CameraLayer
        {
            DefaultLayer = 0,
            LargeObjectLayer = 9,
            WaterLayer = 4,
            MediumObjectLayer = 6,
            SmallObjectLayer = 7,
            CharaLayer = 10,
            MapLayer = 11
        }

        public enum CameraLayerMask
        {
            DefaultLayer = (1 << CameraLayer.DefaultLayer),
            LargeObjectLayer = (1 << CameraLayer.LargeObjectLayer),
            WaterLayer = (1 << CameraLayer.WaterLayer),
            MediumObjectLayer = (1 << CameraLayer.MediumObjectLayer),
            SmallObjectLayer = (1 << CameraLayer.SmallObjectLayer),
            CharaLayer = (1 << CameraLayer.CharaLayer),
            MapLayer = (1 << CameraLayer.MapLayer)
        }

        public static void InitializeCamera(Camera camera, float nearClipPlane, float farClipPlane)
        {
            Console.WriteLine("InitializeCamera");

            _playerCamera = camera;

            for (int layer = 0; layer < _cameraCullDistances.Length; layer++)
                _cameraCullDistances[layer] = farClipPlane;

            UpdateCameraFarClipPlane(_cameraCullDistances);
            UpdateCameraNearClipPlane(nearClipPlane);
        }

        private static void UpdateCameraFarClipPlane(float[] distance)
        {
            if (_playerCamera == null)
                return;

            _cameraCullDistances = distance;
            _playerCamera.layerCullDistances = _cameraCullDistances;
        }

        public static void UpdateCameraFarClipPlane(CameraLayer layer, float distance)
        {
            if (_playerCamera == null || (int)layer >= _cameraCullDistances.Length)
                return;

            _cameraCullDistances[(int)layer] = distance;
            _playerCamera.layerCullDistances = _cameraCullDistances;
        }
        public static void UpdateCameraNearClipPlane(float distance)
        {
            _playerCamera.nearClipPlane = distance;
        }

        public static void UpdateCameraSphericalCulling(bool sphericalCulling)
        {
            if (_playerCamera == null)
                return;
            
            _playerCamera.layerCullSpherical = sphericalCulling;
        }
    }
}
