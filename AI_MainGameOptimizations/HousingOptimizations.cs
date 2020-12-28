using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI_MainGameOptimizations
{
    public static class HousingOptimizations
    {
        private static GameObject _playerObject;
        private static Camera _playerCamera;

        private static List<Collider> soundColliders = new List<Collider>();
        private static List<Collider> cameraColliders = new List<Collider>();
        private static List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        private static List<PSMeshRendererUpdater> particleMeshRenderers = new List<PSMeshRendererUpdater>();
        private static List<Light> housingLights = new List<Light>();


        private static float _footStepRange;
        private static float _cameraColliderRange;
        private static float _particleRange;

        public static void InitializeHousingOptimizations(GameObject player, Camera camera, float footStepRange, float cameraColliderRange, float particleRange, AnimatorCullingMode cullingMode, bool useAlternateLayers, double largeLayerThreshold, double smallLayerThreshold, float heightLimit)
        {
            Console.WriteLine("InitializeHousing");

            _playerObject = player;
            _playerCamera = camera;
            SetDetectionRanges(footStepRange, cameraColliderRange, particleRange);
            BuildHousingOptimizationLists();
            UpdateAnimatorCulling(cullingMode);
            UpdateHousingLayers(useAlternateLayers, largeLayerThreshold, smallLayerThreshold);
            UpdateHousingShadows(heightLimit);
        }

        public static void DestroyOptimizers()
        {
            UpdateHousingLayers(false);

            _playerObject = null;
            _playerCamera = null;

            foreach(var collider in soundColliders)
            {
                collider.gameObject.SetActive(true);
                collider.enabled = true;
            }

            foreach (var collider in cameraColliders)
            {
                collider.gameObject.SetActive(true);
                collider.enabled = true;
            }

            foreach (var particleMeshRenderer in particleMeshRenderers)
            {
                particleMeshRenderer.gameObject.SetActive(true);
                particleMeshRenderer.enabled = true;
            }

            foreach (var light in housingLights)
            {
                light.enabled = true;
            }

            soundColliders = new List<Collider>();
            cameraColliders = new List<Collider>();
            particleSystems = new List<ParticleSystem>();
            particleMeshRenderers = new List<PSMeshRendererUpdater>();
            housingLights = new List<Light>();
        }

        private static void BuildHousingOptimizationLists()
        {
            Console.WriteLine("BuildHousingOptimizationLists");

            soundColliders = BuildColliderList("SEmesh");
            cameraColliders = BuildColliderList("camera");
            particleSystems = BuildParticleSystemList();
            particleMeshRenderers = BuildPSMeshRendererUpdaterList();
            housingLights = BuildHousingLightList();
        }

        private static List<Collider> BuildColliderList(string key)
        {
            List<Collider> colliders = new List<Collider>();

            var gameObject = GameObject.Find("CommonSpace/MapRoot/NavMeshSurface");
            if (gameObject == null)
                return colliders;

            Collider[] colliderList = gameObject.GetComponentsInChildren<Collider>(true);
            if (colliderList == null)
                return colliders;

            foreach (var collider in colliderList)
                if (collider.name.Contains(key))
                    colliders.Add(collider);

            return colliders;
        }

        private static List<Animator> BuildAnimatorList()
        {
            List<Animator> housingAnimators = new List<Animator>();

            var gameObject = GameObject.Find("CommonSpace/MapRoot/NavMeshSurface");
            if (gameObject == null)
                return housingAnimators;

            Animator[] animators = gameObject.GetComponentsInChildren<Animator>(true);
            if (animators == null)
                return housingAnimators;

            foreach (var animator in animators)
                housingAnimators.Add(animator);

            return housingAnimators;
        }

        private static List<Renderer> BuildRendererList()
        {
            List<Renderer> housingRenderers = new List<Renderer>();

            var gameObject = GameObject.Find("CommonSpace/MapRoot/NavMeshSurface");
            if (gameObject == null)
                return housingRenderers;

            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            if (renderers == null)
                return housingRenderers;

            foreach (var renderer in renderers)
                housingRenderers.Add(renderer);

            return housingRenderers;
        }

        private static List<ParticleSystem> BuildParticleSystemList()
        {
            List<ParticleSystem> housingParticleSystems = new List<ParticleSystem>();

            var gameObject = GameObject.Find("CommonSpace/MapRoot/NavMeshSurface");
            if (gameObject == null)
                return housingParticleSystems;

            ParticleSystem[] particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>(true);
            if (particleSystems != null)
            {
          /*      foreach (var system in housingParticleSystems)
                {
                    var main = system.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.Local;
                }*/

                foreach (var system in particleSystems)
                {
                    if (!system.automaticCullingEnabled)
                        housingParticleSystems.Add(system);
                }
            }

            gameObject = GameObject.Find("map00_Beach");
            if (gameObject == null)
                return housingParticleSystems;

            particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>(true);
            if (particleSystems != null)
            {
                /*      foreach (var system in housingParticleSystems)
                      {
                          var main = system.main;
                          main.simulationSpace = ParticleSystemSimulationSpace.Local;
                      }*/

                foreach (var system in particleSystems)
                {
                    if (!system.automaticCullingEnabled)
                        housingParticleSystems.Add(system);
                }
            }

            return housingParticleSystems;
        }

        private static List<PSMeshRendererUpdater> BuildPSMeshRendererUpdaterList()
        {
            List<PSMeshRendererUpdater> housingParticleMeshRenderer = new List<PSMeshRendererUpdater>();

            var gameObject = GameObject.Find("CommonSpace/MapRoot/NavMeshSurface");
            if (gameObject == null)
                return housingParticleMeshRenderer;

            PSMeshRendererUpdater[] particleMeshRenderers = gameObject.GetComponentsInChildren<PSMeshRendererUpdater>(true);
            if (particleMeshRenderers != null)
            {
                foreach (var renderer in particleMeshRenderers)
                    housingParticleMeshRenderer.Add(renderer);
            }

            gameObject = GameObject.Find("map00_Beach");
            if (gameObject == null)
                return housingParticleMeshRenderer;

            particleMeshRenderers = gameObject.GetComponentsInChildren<PSMeshRendererUpdater>(true);
            if (particleMeshRenderers != null)
            {
                foreach (var renderer in particleMeshRenderers)
                    housingParticleMeshRenderer.Add(renderer);
            }

            return housingParticleMeshRenderer;
        }

        private static List<Light> BuildHousingLightList()
        {
            List<Light> housingLights = new List<Light>();

            var gameObject = GameObject.Find("CommonSpace/MapRoot/NavMeshSurface");
            if (gameObject == null)
                return housingLights;

            Light[] lights = gameObject.GetComponentsInChildren<Light>(true);
            if (lights == null)
                return housingLights;

            foreach (var light in lights)
                housingLights.Add(light);

            return housingLights;
        }

        public static void SetDetectionRanges(float footstepRange, float cameraRange, float particleRange)
        {
            _footStepRange = footstepRange;
            _cameraColliderRange = cameraRange;
            _particleRange = particleRange;
        }

        public static void HousingColliderCheck(int startIndex, int updateRate)
        {
            if (_playerCamera == null || _playerObject == null)
                return;

            float playerPositionX = _playerObject.transform.position.x;
            float playerPositionZ = _playerObject.transform.position.z;
            float cameraPositionX = _playerCamera.transform.position.x;
            float cameraPositionZ = _playerCamera.transform.position.z;

            EnableColliderByRoughRange(soundColliders, playerPositionX, playerPositionZ, startIndex, updateRate, _footStepRange);
            EnableColliderByRoughRange(cameraColliders, cameraPositionX, cameraPositionZ, startIndex, updateRate, _cameraColliderRange);
        }

        public static void HousingLightAndParticleCheck(int startIndex, int updateRate)
        {
            if (_playerCamera == null)
                return;

            float cameraPositionX = _playerCamera.transform.position.x;
            float cameraPositionZ = _playerCamera.transform.position.z;

            HousingParticleSystemCheck(startIndex, updateRate);
            EnableParticlesByRoughRange(cameraPositionX, cameraPositionZ, startIndex, updateRate, _particleRange);         
            EnableLightsByRoughRange(cameraPositionX, cameraPositionZ, startIndex, updateRate, _particleRange);
        }

        private static void HousingParticleSystemCheck(int startIndex, int updateRate)
        {
            if (particleSystems == null)
                return;

            for (int index = startIndex; index < particleSystems.Count; index += updateRate)
            {
                if (!particleSystems[index].gameObject.activeSelf)
                    continue;

                ParticleSystemRenderer particleSystemRenderer = particleSystems[index].gameObject.GetComponent<ParticleSystemRenderer>();

                if (particleSystemRenderer == null)
                    continue;

                if (particleSystems[index].isPaused && particleSystemRenderer.isVisible)
                    particleSystems[index].Play(true);
                else if (particleSystems[index].isPlaying && !particleSystemRenderer.isVisible)
                    particleSystems[index].Pause(true);
            }
        }


        private static void EnableParticlesByRoughRange(float positionX, float positionZ, int startIndex, int updateRate, float rangeLimit)
        {
            if (particleMeshRenderers == null)
                return;

            for (int index = startIndex; index < particleMeshRenderers.Count; index += updateRate)
            {
                if (particleMeshRenderers[index].MeshObject == null)
                    continue;

                MeshRenderer meshRenderer = particleMeshRenderers[index].MeshObject.GetComponent<MeshRenderer>();

                if (meshRenderer == null)
                    continue;

                bool visibleOrInRange = meshRenderer.isVisible ||
                                     Math.Abs(positionX - particleMeshRenderers[index].transform.position.x) < rangeLimit ||
                                     Math.Abs(positionZ - particleMeshRenderers[index].transform.position.z) < rangeLimit;

                if (particleMeshRenderers[index].enabled != visibleOrInRange)
                {
                    particleMeshRenderers[index].enabled = visibleOrInRange;
                    particleMeshRenderers[index].gameObject.SetActive(visibleOrInRange);
                }
            }
        }

        private static void EnableLightsByRoughRange(float positionX, float positionZ, int startIndex, int updateRate, float rangeLimit)
        {
            if (housingLights == null)
                return;

            for (int index = startIndex; index < housingLights.Count; index += updateRate)
            {
                bool inRange = Math.Abs(positionX - housingLights[index].transform.position.x) < rangeLimit ||
                               Math.Abs(positionZ - housingLights[index].transform.position.z) < rangeLimit;

                if (housingLights[index].enabled != inRange)
                    housingLights[index].enabled = inRange;
            }
        }

        private static void EnableColliderByRoughRange(List<Collider> colliders, float positionX, float positionZ, int startIndex, int updateRate, float rangeLimit)
        {
            if (colliders == null)
                return;

            for (int collider = startIndex; collider < colliders.Count; collider += updateRate)
            {
                if (colliders[collider].enabled)
                {
                    if ((Math.Abs(positionX - colliders[collider].transform.position.x) > rangeLimit || Math.Abs(positionZ - colliders[collider].transform.position.z) > rangeLimit))
                    {
                        colliders[collider].gameObject.SetActive(false);
                        colliders[collider].enabled = false;
                    }
                }
                else
                {
                    if ((Math.Abs(positionX - colliders[collider].transform.position.x) < rangeLimit && Math.Abs(positionZ - colliders[collider].transform.position.z) < rangeLimit))
                    {
                        colliders[collider].gameObject.SetActive(true);
                        colliders[collider].enabled = true;
                    }
                }
            }
        }

        public static void UpdateAnimatorCulling(AnimatorCullingMode cullingMode)
        {
            List<Animator> buildingAnimators = BuildAnimatorList();

            if (buildingAnimators == null)
                return;

            foreach (var animator in buildingAnimators)
                animator.cullingMode = cullingMode;
        }

        public static void UpdateHousingLayers(bool useAlternateLayers, double largeLayerThreshold = 1000, double smallLayerThreshold = 100)
        {
            UpdateHousingRendererLayers(useAlternateLayers, largeLayerThreshold, smallLayerThreshold);
            UpdateHousingLightLayers(useAlternateLayers);
        }

        private static void UpdateHousingRendererLayers(bool useAlternateLayers, double largeLayerThreshold, double smallLayerThreshold)
        {
            List<Renderer> housingRenders = BuildRendererList();

            if (housingRenders == null)
                return;

            if (useAlternateLayers)
            {
                foreach (var housingRenderer in housingRenders)
                {
                    double rendererFaceSize = housingRenderer.bounds.extents.y * housingRenderer.bounds.extents.y * Math.Sqrt((housingRenderer.bounds.extents.x * housingRenderer.bounds.extents.x) + (housingRenderer.bounds.extents.z * housingRenderer.bounds.extents.z));

                    if (housingRenderer.name.Contains("house") || rendererFaceSize >= largeLayerThreshold)
                        housingRenderer.gameObject.layer = (int)CameraOptimizations.CameraLayer.LargeObjectLayer;
                    else if (rendererFaceSize <= smallLayerThreshold)
                        housingRenderer.gameObject.layer = (int)CameraOptimizations.CameraLayer.SmallObjectLayer;
                    else
                        housingRenderer.gameObject.layer = (int)CameraOptimizations.CameraLayer.SmallObjectLayer;
                }
            }
            else
            {
                foreach (var housingMeshRenderer in housingRenders)
                    housingMeshRenderer.gameObject.layer = (int)CameraOptimizations.CameraLayer.MapLayer;
            }
        }

        public static void UpdateHousingShadows(float heightLimit)
        {
            List<Renderer> housingRenders = BuildRendererList();

            if (housingRenders == null)
                return;

            foreach (var housingRenderer in housingRenders)
            {
                if (!housingRenderer.name.Contains("house") && housingRenderer.bounds.extents.y <= heightLimit)
                    housingRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        public static void UpdateHousingLightLayers(bool useAlternateLayers)
        {
            if (housingLights == null)
                return;

            int newLayer = (int)CameraOptimizations.CameraLayer.MapLayer;   
            if (useAlternateLayers)
                newLayer = (int)CameraOptimizations.CameraLayer.SmallObjectLayer;

            foreach (var housingLight in housingLights)
                housingLight.gameObject.layer = newLayer;
        }
    }
}
