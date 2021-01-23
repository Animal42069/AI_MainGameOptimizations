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
        private static List<ParticleSystem> housingParticleSystems = new List<ParticleSystem>();
        private static List<ParticleSystem> housingParticleSystemsComplete = new List<ParticleSystem>();
        private static List<PSMeshRendererUpdater> particleMeshRenderers = new List<PSMeshRendererUpdater>();
        private static List<Light> housingLights = new List<Light>();

        private static float _footStepRange;
        private static float _cameraColliderRange;
        private static float _particleRange;

        public static void InitializeHousingOptimizations(GameObject player, Camera camera, float footStepRange, float cameraColliderRange, float particleRange, AnimatorCullingMode cullingMode, GameLayers.LayerStrategy layerStrategy, double largeLayerThreshold, double smallLayerThreshold, float heightLimit)
        {
            _playerObject = player;
            _playerCamera = camera;
            SetDetectionRanges(footStepRange, cameraColliderRange, particleRange);
            BuildHousingOptimizationLists();
            UpdateAnimatorCulling(cullingMode);
            UpdateHousingLayers(layerStrategy, largeLayerThreshold, smallLayerThreshold);
            UpdateHousingShadows(heightLimit);
        }

        public static void DestroyOptimizers()
        {
            UpdateHousingLayers(GameLayers.LayerStrategy.None);

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

            soundColliders.Clear();
            cameraColliders.Clear();
            housingParticleSystems.Clear();
            housingParticleSystemsComplete.Clear();
            particleMeshRenderers.Clear();
            housingLights.Clear();
        }

        private static void BuildHousingOptimizationLists()
        {
            soundColliders = BuildColliderList("SEmesh");
            cameraColliders = BuildColliderList("camera");
            BuildParticleSystemList();
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
            if (colliderList.IsNullOrEmpty())
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
            if (animators.IsNullOrEmpty())
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
            if (renderers.IsNullOrEmpty())
                return housingRenderers;

            foreach (var renderer in renderers)
                housingRenderers.Add(renderer);

            return housingRenderers;
        }

        private static void BuildParticleSystemList()
        {
            housingParticleSystems = new List<ParticleSystem>();
            housingParticleSystemsComplete = new List<ParticleSystem>();

            ParticleSystem[] particleSystems = GameObject.Find("CommonSpace/MapRoot/NavMeshSurface")?.GetComponentsInChildren<ParticleSystem>(true);
            if (particleSystems != null)
            {
                foreach (var particleSystem in particleSystems)
                {
                    var main = particleSystem.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.Local;

                    if (particleSystem.gameObject.activeSelf && !particleSystem.automaticCullingEnabled)
                        housingParticleSystems.Add(particleSystem);

                    housingParticleSystemsComplete.Add(particleSystem);
                }
            }

            particleSystems = GameObject.Find("map00_Beach")?.GetComponentsInChildren<ParticleSystem>();
            if (particleSystems == null)
                return;
            
            foreach (var particleSystem in particleSystems)
            {
                var main = particleSystem.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;

                if (!particleSystem.automaticCullingEnabled)
                    housingParticleSystems.Add(particleSystem);
            }       
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
            if (lights.IsNullOrEmpty())
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
            if (housingParticleSystems.IsNullOrEmpty())
                return;

            for (int index = startIndex; index < housingParticleSystems.Count; index += updateRate)
            {
                if (!housingParticleSystems[index].gameObject.activeSelf)
                    continue;

                ParticleSystemRenderer particleSystemRenderer = housingParticleSystems[index].gameObject.GetComponent<ParticleSystemRenderer>();

                if (particleSystemRenderer == null)
                    continue;

                if (housingParticleSystems[index].isPaused && particleSystemRenderer.isVisible)
                    housingParticleSystems[index].Play(true);
                else if (housingParticleSystems[index].isPlaying && !particleSystemRenderer.isVisible)
                    housingParticleSystems[index].Pause(true);
            }
        }

        public static void SetParticleSystemActive(bool active, string key)
        {
            Console.WriteLine($"SetParticleSystemActive {active} {key}");

            if (housingParticleSystemsComplete.IsNullOrEmpty())
                return;

            foreach(var particleSystem in housingParticleSystemsComplete)
            {
                Console.WriteLine($"particleSystem {particleSystem.name}");

                if (particleSystem.name.Contains(key))
                {
                    Console.WriteLine($"particleSystem {active}");
                    particleSystem.gameObject.SetActive(active);
                }
            }
        }

        private static void EnableParticlesByRoughRange(float positionX, float positionZ, int startIndex, int updateRate, float rangeLimit)
        {
            if (particleMeshRenderers.IsNullOrEmpty())
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
            if (housingLights.IsNullOrEmpty())
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
            if (colliders.IsNullOrEmpty())
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

            if (buildingAnimators.IsNullOrEmpty())
                return;

            foreach (var animator in buildingAnimators)
                animator.cullingMode = cullingMode;
        }

        public static void UpdateHousingLayers(GameLayers.LayerStrategy layerStrategy, double largeLayerThreshold = 1000, double smallLayerThreshold = 100)
        {
            UpdateHousingRendererLayers(layerStrategy, largeLayerThreshold, smallLayerThreshold);
            UpdateHousingLightLayers(layerStrategy);
        }

        private static void UpdateHousingRendererLayers(GameLayers.LayerStrategy layerStrategy, double largeLayerThreshold, double smallLayerThreshold)
        {
            List<Renderer> housingRenders = BuildRendererList();

            if (housingRenders.IsNullOrEmpty())
                return;

            if (layerStrategy == GameLayers.LayerStrategy.MultiLayer)
            {
                foreach (var housingRenderer in housingRenders)
                {
                    if (housingRenderer.gameObject.layer != (int)GameLayers.Layer.MapLayer &&
                        housingRenderer.gameObject.layer != (int)GameLayers.Layer.DefaultLayer)
                        continue;

                    double rendererFaceSize = housingRenderer.bounds.extents.y * housingRenderer.bounds.extents.y * Math.Sqrt((housingRenderer.bounds.extents.x * housingRenderer.bounds.extents.x) + (housingRenderer.bounds.extents.z * housingRenderer.bounds.extents.z));

                    if (housingRenderer.name.Contains("house") || rendererFaceSize >= largeLayerThreshold)
                        housingRenderer.gameObject.layer = (int)GameLayers.Layer.LargeObjectLayer;
                    else if (rendererFaceSize <= smallLayerThreshold)
                        housingRenderer.gameObject.layer = (int)GameLayers.Layer.SmallObjectLayer;
                    else
                        housingRenderer.gameObject.layer = (int)GameLayers.Layer.MediumObjectLayer;
                }
            }
            else if (layerStrategy == GameLayers.LayerStrategy.SingleLayer)
            {
                foreach (var housingRenderer in housingRenders)
                {
                    if (housingRenderer.gameObject.layer == (int)GameLayers.Layer.LargeObjectLayer ||
                        housingRenderer.gameObject.layer == (int)GameLayers.Layer.SmallObjectLayer ||
                        housingRenderer.gameObject.layer == (int)GameLayers.Layer.MediumObjectLayer ||
                        housingRenderer.gameObject.layer == (int)GameLayers.Layer.MapLayer)
                        housingRenderer.gameObject.layer = (int)GameLayers.Layer.DefaultLayer;
                }
            }
            else
            {
                foreach (var housingRenderer in housingRenders)
                {
                    if (housingRenderer.gameObject.layer == (int)GameLayers.Layer.LargeObjectLayer ||
                        housingRenderer.gameObject.layer == (int)GameLayers.Layer.SmallObjectLayer ||
                        housingRenderer.gameObject.layer == (int)GameLayers.Layer.MediumObjectLayer ||
                        housingRenderer.gameObject.layer == (int)GameLayers.Layer.DefaultLayer)
                        housingRenderer.gameObject.layer = (int)GameLayers.Layer.MapLayer;
                }
            }
        }

        public static void UpdateHousingShadows(float heightLimit)
        {
            List<Renderer> housingRenders = BuildRendererList();

            if (housingRenders.IsNullOrEmpty())
                return;

            foreach (var housingRenderer in housingRenders)
            {
                if (!housingRenderer.name.Contains("house") && housingRenderer.bounds.extents.y <= heightLimit)
                    housingRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        public static void UpdateHousingLightLayers(GameLayers.LayerStrategy layerStrategy)
        {
            if (housingLights.IsNullOrEmpty())
                return;
            int newLayer = (int)GameLayers.Layer.MapLayer;
            if (layerStrategy == GameLayers.LayerStrategy.MultiLayer)
                newLayer = (int)GameLayers.Layer.MediumObjectLayer;
            else if (layerStrategy == GameLayers.LayerStrategy.SingleLayer)
                newLayer = (int)GameLayers.Layer.DefaultLayer;

            foreach (var housingLight in housingLights)
            {
                if (housingLight.gameObject.layer == (int)GameLayers.Layer.SmallObjectLayer ||
                    housingLight.gameObject.layer == (int)GameLayers.Layer.DefaultLayer ||
                    housingLight.gameObject.layer == (int)GameLayers.Layer.MapLayer)
                    housingLight.gameObject.layer = newLayer;
            }
        }
    }
}
