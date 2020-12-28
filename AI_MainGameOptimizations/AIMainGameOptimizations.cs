using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AI_MainGameOptimizations
{
    [BepInPlugin(GUID, "AI Main Game Optimizations", VERSION)]
    [BepInProcess("AI-Syoujyo")]
    public class AIMainGameOptimizations : BaseUnityPlugin
    {
        public const string VERSION = "1.0.1.0";
        private const string GUID = "animal42069.aimaingameoptimizations";
        
        internal static ConfigEntry<bool> _enableIllusionDynamicBoneChecks;
        internal static ConfigEntry<bool> _IKSolverChecks;
        internal static ConfigEntry<bool> _LookControllerChecks;
        internal static ConfigEntry<int> _dynamicBoneUpdateRate;
        internal static ConfigEntry<float> _dynamicBoneBodyRange;
        internal static ConfigEntry<float> _dynamicBoneHairRange;
        internal static ConfigEntry<float> _dynamicBoneClothingRange;
        internal static ConfigEntry<float> _dynamicBoneGenitalRange;

        internal static ConfigEntry<ShadowProjection> _shadowProjection;
        internal static ConfigEntry<LightShadows> _citySpotLightShadows;
        internal static ConfigEntry<LightShadows> _cityPointLightShadows;
        internal static ConfigEntry<float> _shadowDistance;
        internal static ConfigEntry<int> _characterShadowDistance;
        internal static ConfigEntry<int> _worldShadowDistance;
        internal static ConfigEntry<int> _mediumShadowDistance;
        internal static ConfigEntry<int> _smallShadowDistance;
        internal static ConfigEntry<int> _largeShadowDistance;
        internal static ConfigEntry<int> _waterShadowDistance;
        internal static ConfigEntry<bool> _terrainCastShadows;

        internal static ConfigEntry<int> _housingColliderUpdateRate;
        internal static ConfigEntry<int> _housingParticleUpdateRate;
        internal static ConfigEntry<float> _footstepRange;
        internal static ConfigEntry<float> _cameraColliderRange;
        internal static ConfigEntry<float> _particleEmitterRange;

        internal static ConfigEntry<AnimatorCullingMode> _characterAnimationCulling;
        internal static ConfigEntry<AnimatorCullingMode> _housingAnimatorCulling;
        internal static ConfigEntry<AnimatorCullingMode> _animalAnimatorCulling;
        internal static ConfigEntry<AnimatorCullingMode> _worldAnimatorCulling;

        internal static ConfigEntry<int> _characterClipDistance;
        internal static ConfigEntry<int> _worldClipDistance;
        internal static ConfigEntry<int> _mediumClipDistance;
        internal static ConfigEntry<int> _smallClipDistance;
        internal static ConfigEntry<int> _terrainClipDistance;
        internal static ConfigEntry<int> _waterClipDistance;
        internal static ConfigEntry<int> _hSceneClipDistance;
        internal static ConfigEntry<bool> _sphericalCulling;

        internal static ConfigEntry<int> _commandUpdateRate;
        internal static ConfigEntry<int> _searchUpdateRate;
        internal static ConfigEntry<int> _UIMenuUpdateRate;
        internal static ConfigEntry<bool> _enableInactiveUIChecks;
        internal static ConfigEntry<bool> _playerDynamicBones;

        internal static ConfigEntry<bool> _moveTerrainLayer;
        internal static ConfigEntry<bool> _moveHousingLayer;
        internal static ConfigEntry<float> _housingLargeObjectSize;
        internal static ConfigEntry<float> _housingSmallObjectSize;
        internal static ConfigEntry<float> _housingDisableShadowHeight;
        internal static ConfigEntry<bool> _enableCityPointLights;
        internal static ConfigEntry<float> _citySpotLightIntensity;
        internal static ConfigEntry<float> _basemapDistance;
        internal static ConfigEntry<bool> _drawTreesAndFoliage;
        internal static ConfigEntry<int> _maximumLOD;
        internal static ConfigEntry<float> _LODBias;
        internal static ConfigEntry<bool> _simplifySunPosition;

        private static GameObject _playerObject;
        private static Camera _playerCamera;

        private static bool _bMapLoaded = false;

        private void Awake()
        {
            _enableIllusionDynamicBoneChecks = Config.Bind("Character Optimizations", "Illusion Dynamic Bone Checks", false, "(ILLUSION DEFAULT true) Disable to turn off Illusion Dynamic Bone Checks");
            _dynamicBoneUpdateRate = Config.Bind("Character Optimizations", "Dynamic Bone Enable Rate", 10, new ConfigDescription("(ILLUSION DEFAULT 1) Number of frames to spread out enabling/disabling dynamic bone checks, quicker response but lower perfomance", new AcceptableValueRange<int>(1, 60)));
            (_dynamicBoneClothingRange = Config.Bind("Character Optimizations", "Dynamic Bone Range - Accesories/Clothing", 250f, new ConfigDescription("(ILLUSION DEFAULT 10000) Range where clothing dynamic bones are enabled, always enabled during HScenes", new AcceptableValueRange<float>(1, 10000)))).SettingChanged += (s, e) =>
            { CharacterOptimizations.SetColliderRanges(_dynamicBoneGenitalRange.Value * _dynamicBoneGenitalRange.Value, _dynamicBoneHairRange.Value * _dynamicBoneHairRange.Value, _dynamicBoneClothingRange.Value * _dynamicBoneClothingRange.Value, _dynamicBoneBodyRange.Value * _dynamicBoneBodyRange.Value); };
            (_dynamicBoneHairRange = Config.Bind("Character Optimizations", "Dynamic Bone Range - Hair", 150f, new ConfigDescription("(ILLUSION DEFAULT 10000) Range where hair dynamic bones are enabled, always enabled during HScenes", new AcceptableValueRange<float>(1, 10000)))).SettingChanged += (s, e) =>
            { CharacterOptimizations.SetColliderRanges(_dynamicBoneGenitalRange.Value * _dynamicBoneGenitalRange.Value, _dynamicBoneHairRange.Value * _dynamicBoneHairRange.Value, _dynamicBoneClothingRange.Value * _dynamicBoneClothingRange.Value, _dynamicBoneBodyRange.Value * _dynamicBoneBodyRange.Value); };
            (_dynamicBoneBodyRange = Config.Bind("Character Optimizations", "Dynamic Bone Range - TnA", 100f, new ConfigDescription("(ILLUSION DEFAULT 10000) Range where body dynamic bones are enabled, always enabled during HScenes", new AcceptableValueRange<float>(1, 10000)))).SettingChanged += (s, e) =>
            { CharacterOptimizations.SetColliderRanges(_dynamicBoneGenitalRange.Value * _dynamicBoneGenitalRange.Value, _dynamicBoneHairRange.Value * _dynamicBoneHairRange.Value, _dynamicBoneClothingRange.Value * _dynamicBoneClothingRange.Value, _dynamicBoneBodyRange.Value * _dynamicBoneBodyRange.Value); };
            (_dynamicBoneGenitalRange = Config.Bind("Character Optimizations", "Dynamic Bone Range - Vagina", 1f, new ConfigDescription("(ILLUSION DEFAULT 10000) Range where genital dynamic bones are enabled, always enabled during HScenes", new AcceptableValueRange<float>(1, 10000)))).SettingChanged += (s, e) =>
            { CharacterOptimizations.SetColliderRanges(_dynamicBoneGenitalRange.Value * _dynamicBoneGenitalRange.Value, _dynamicBoneHairRange.Value * _dynamicBoneHairRange.Value, _dynamicBoneClothingRange.Value * _dynamicBoneClothingRange.Value, _dynamicBoneBodyRange.Value * _dynamicBoneBodyRange.Value); };
            (_characterAnimationCulling = Config.Bind("Character Optimizations", "Optimizations - Animation Culling", AnimatorCullingMode.CullUpdateTransforms, "(ILLUSION DEFAULT AlwaysAnimate) What animators should do when they are not visible")).SettingChanged += (s, e) =>
            { CharacterOptimizations.UpdateAnimatorCulling(_characterAnimationCulling.Value); };
            _IKSolverChecks = Config.Bind("Character Optimizations", "Optimizations - IK Solver Checks", true, "(ILLUSION DEFAULT true) Skip IK solvers if character is not visible");
            _LookControllerChecks = Config.Bind("Character Optimizations", "Optimizations - Look Controller Checks", true, "(ILLUSION DEFAULT true) Skip head look controls if character is not visible");

            _searchUpdateRate = Config.Bind("Character Optimizations", "Search Update Rate", 30, new ConfigDescription("Number of frames to spread out character checks to see what objects are closeby to interact with, lower values = quicker response but lower perfomance", new AcceptableValueRange<int>(1, 60)));

            (_shadowDistance = Config.Bind("Shadow Optimizations", "Shadow Distance", 400f, new ConfigDescription("(ILLUSION DEFAULT 10000) Overall maximum shadow distance", new AcceptableValueRange<float>(1, 10000)))).SettingChanged += (s, e) =>
            { QualitySettings.shadowDistance = _shadowDistance.Value; };
            (_shadowProjection = Config.Bind("Shadow Optimizations", "Shadow Projection Mode", ShadowProjection.StableFit, "(ILLUSION DEFAULT CloseFit) Close fit shadows update more often, but stable fit jitter less and render slightly quicker")).SettingChanged += (s, e) =>
            { QualitySettings.shadowProjection = _shadowProjection.Value; };
            (_characterShadowDistance = Config.Bind("Shadow Optimizations", "Shadows Distance - Characters", 150, new ConfigDescription("(ILLUSION DEFAULT 10000) Distance to render character shadows", new AcceptableValueRange<int>(1, 10000)))).SettingChanged += (s, e) =>
            { MainLightOptimizations.UpdateShadowClipPlane((int)CameraOptimizations.CameraLayer.CharaLayer, _characterShadowDistance.Value); };
            (_largeShadowDistance = Config.Bind("Shadow Optimizations", "Shadows Distance - Objects, Large/Terrain", 250, new ConfigDescription("(ILLUSION DEFAULT 10000) Distance to render terrain shadows", new AcceptableValueRange<int>(1, 10000)))).SettingChanged += (s, e) =>
            { MainLightOptimizations.UpdateShadowClipPlane((int)CameraOptimizations.CameraLayer.LargeObjectLayer, _largeShadowDistance.Value); };
            (_mediumShadowDistance = Config.Bind("Shadow Optimizations", "Shadows Distance - Objects, Medium", 200, new ConfigDescription("(ILLUSION DEFAULT 10000) Distance to render large housing shadows", new AcceptableValueRange<int>(1, 10000)))).SettingChanged += (s, e) =>
            { MainLightOptimizations.UpdateShadowClipPlane((int)CameraOptimizations.CameraLayer.MediumObjectLayer, _mediumShadowDistance.Value); };
            (_smallShadowDistance = Config.Bind("Shadow Optimizations", "Shadows Distance - Objects, Small", 150, new ConfigDescription("(ILLUSION DEFAULT 10000) Distance to render small housing shadows", new AcceptableValueRange<int>(1, 10000)))).SettingChanged += (s, e) =>
            { MainLightOptimizations.UpdateShadowClipPlane((int)CameraOptimizations.CameraLayer.SmallObjectLayer, _smallShadowDistance.Value); };
            (_waterShadowDistance = Config.Bind("Shadow Optimizations", "Shadows Distance - Water", 100, new ConfigDescription("(ILLUSION DEFAULT 10000) Distance to render water shadows", new AcceptableValueRange<int>(1, 10000)))).SettingChanged += (s, e) =>
            { MainLightOptimizations.UpdateShadowClipPlane((int)CameraOptimizations.CameraLayer.WaterLayer, _waterShadowDistance.Value); };
            (_worldShadowDistance = Config.Bind("Shadow Optimizations", "Shadows Distance - World", 250, new ConfigDescription("(ILLUSION DEFAULT 10000) Distance to render world shadows", new AcceptableValueRange<int>(1, 10000)))).SettingChanged += (s, e) =>
            { MainLightOptimizations.UpdateShadowClipPlane((int)CameraOptimizations.CameraLayer.MapLayer, _worldShadowDistance.Value); };

            _housingColliderUpdateRate = Config.Bind("Housing Optimizations", "Collider Update Rate", 30, new ConfigDescription("(ILLUSION DEFAULT N/A) Number of frames to spread out housing collider checks, lower values = more responsive colliders but lower perfomance", new AcceptableValueRange<int>(1, 60)));
            _housingParticleUpdateRate = Config.Bind("Housing Optimizations", "Light/Particle Update Rate", 10, new ConfigDescription("(ILLUSION DEFAULT N/A) Number of frames to spread out housing particle enable checks, lower values = more responsive particle enabling but lower perfomance", new AcceptableValueRange<int>(1, 60)));
            (_footstepRange = Config.Bind("Housing Optimizations", "Collider Range - Footsteps", 50f, new ConfigDescription("(ILLUSION DEFAULT N/A) Range at which to enable footstep colliders for activating different footstep sounds", new AcceptableValueRange<float>(1f, 10000f)))).SettingChanged += (s, e) =>
            { HousingOptimizations.SetDetectionRanges(_footstepRange.Value, _cameraColliderRange.Value, _particleEmitterRange.Value); };
            (_cameraColliderRange = Config.Bind("Housing Optimizations", "Collider Range - Camera", 50f, new ConfigDescription("(ILLUSION DEFAULT N/A) Range where camera colliders are enabled", new AcceptableValueRange<float>(1, 10000)))).SettingChanged += (s, e) =>
            { HousingOptimizations.SetDetectionRanges(_footstepRange.Value, _cameraColliderRange.Value, _particleEmitterRange.Value); };
            (_particleEmitterRange = Config.Bind("Housing Optimizations", "Light/Particle Range", 100f, new ConfigDescription("(ILLUSION DEFAULT N/A) Range where housing lights and particle emitters (like torches/steam) are enabled.  Visible emitters are always enabled.", new AcceptableValueRange<float>(1, 10000)))).SettingChanged += (s, e) =>
            { HousingOptimizations.SetDetectionRanges(_footstepRange.Value, _cameraColliderRange.Value, _particleEmitterRange.Value); };
            (_moveHousingLayer = Config.Bind("Housing Optimizations", "Move Housing objects to alternate layers", true, "(ILLUSION DEFAULT false) Moves the housing objects to alternate layers so they can be clipped")).SettingChanged += (s, e) =>
            { HousingOptimizations.UpdateHousingLayers(_moveHousingLayer.Value, _housingLargeObjectSize.Value, _housingSmallObjectSize.Value); };
            (_housingLargeObjectSize = Config.Bind("Housing Optimizations", "Housing objects - Large Object Threshold", 1000f, "(ILLUSION DEFAULT N/A) Objects larger than this will be moved to Large Object Layer.  Size is computed by Y axis squared times X/Z diagonal")).SettingChanged += (s, e) =>
            { HousingOptimizations.UpdateHousingLayers(_moveHousingLayer.Value, _housingLargeObjectSize.Value, _housingSmallObjectSize.Value); };
            (_housingSmallObjectSize = Config.Bind("Housing Optimizations", "Housing objects - Small Object Threshold", 100f, "(ILLUSION DEFAULT N/A) Objects smaller than this will be moved to Small Object Layer.  Size is computed by Y axis squared times X/Z diagonal")).SettingChanged += (s, e) =>
            { HousingOptimizations.UpdateHousingLayers(_moveHousingLayer.Value, _housingLargeObjectSize.Value, _housingSmallObjectSize.Value); };
            (_housingAnimatorCulling = Config.Bind("Housing Optimizations", "Housing Animator Culling", AnimatorCullingMode.CullCompletely, "(ILLUSION DEFAULT CullUpdateTransforms) What animators should do when they are not visible")).SettingChanged += (s, e) =>
            { HousingOptimizations.UpdateAnimatorCulling(_housingAnimatorCulling.Value); };
            (_housingDisableShadowHeight = Config.Bind("Housing Optimizations", "Shadow Height Limit", 0f, new ConfigDescription("(ILLUSION DEFAULT 0) Disable shadows for housing items shorter than limit (Items like clover patches, ground ivy, etc).", new AcceptableValueRange<float>(0, 5)))).SettingChanged += (s, e) =>
            { HousingOptimizations.UpdateHousingShadows(_housingDisableShadowHeight.Value); };

            (_sphericalCulling = Config.Bind("Camera Clip Optimizations", "Spherical Clipping", true, new ConfigDescription("(ILLUSION DEFAULT false) Clips objects in a spherical radius so items don't appear/dissapear as you rotate the camera."))).SettingChanged += (s, e) =>
            { CameraOptimizations.UpdateCameraSphericalCulling(_sphericalCulling.Value); };
            (_characterClipDistance = Config.Bind("Camera Clip Optimizations", "Clip Distance - Characters", 250, new ConfigDescription("(ILLUSION DEFAULT 10000) Clipping Distance for Characters", new AcceptableValueRange<int>(1, 10000)))).SettingChanged += (s, e) =>
            { CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.CharaLayer, _characterClipDistance.Value); };
            (_terrainClipDistance = Config.Bind("Camera Clip Optimizations", "Clip Distance - Objects, Large/Terrain", 500, new ConfigDescription("(ILLUSION DEFAULT 10000) Clipping Distance for the Terrain (ground)", new AcceptableValueRange<int>(1, 10000)))).SettingChanged += (s, e) =>
            { CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.LargeObjectLayer, _terrainClipDistance.Value); };
            (_mediumClipDistance = Config.Bind("Camera Clip Optimizations", "Clip Distance - Objects, Medium", 350, new ConfigDescription("(ILLUSION DEFAULT 10000) Clipping Distance for Large Housing Objects", new AcceptableValueRange<int>(1, 10000)))).SettingChanged += (s, e) =>
            { CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.MediumObjectLayer, _mediumClipDistance.Value); };
            (_smallClipDistance = Config.Bind("Camera Clip Optimizations", "Clip Distance - Objects, Small", 200, new ConfigDescription("(ILLUSION DEFAULT 10000) Clipping Distance for Small Housing Objects", new AcceptableValueRange<int>(1, 10000)))).SettingChanged += (s, e) =>
            { CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.SmallObjectLayer, _smallClipDistance.Value); };
            (_waterClipDistance = Config.Bind("Camera Clip Optimizations", "Clip Distance - Water", 2000, new ConfigDescription("(ILLUSION DEFAULT 10000) Clipping Distance for Water", new AcceptableValueRange<int>(1, 10000)))).SettingChanged += (s, e) =>
            { CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.WaterLayer, _waterClipDistance.Value); };
            (_worldClipDistance = Config.Bind("Camera Clip Optimizations", "Clip Distance - World", 2000, new ConfigDescription("(ILLUSION DEFAULT 10000) Clipping Distance for the Map", new AcceptableValueRange<int>(1, 10000)))).SettingChanged += (s, e) =>
            { CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.MapLayer, _worldClipDistance.Value); };
            _hSceneClipDistance = Config.Bind("Camera Clip Optimizations", "HScene Clip Distance", 250, new ConfigDescription("(ILLUSION DEFAULT 10000) Max Clipping Distance during hScenes", new AcceptableValueRange<int>(1, 10000)));

            (_cityPointLightShadows = Config.Bind("World Optimizations", "City Point Light Shadows", LightShadows.None, "(ILLUSION DEFAULT Hard) Turn shadows on/off for the point lights on the main island.  Point lights are very expensive to cast shadows.")).SettingChanged += (s, e) =>
            { WorldOptimizations.SetCityLightShadows(_citySpotLightShadows.Value, _cityPointLightShadows.Value); };
            (_enableCityPointLights = Config.Bind("World Optimizations", "City Point Lights Enabled", false, "(ILLUSION DEFAULT true) Disable City Point Lights.  All city lights have a spot light and a point light.  Point lights can be expensive to render, especially if they have shadows.")).SettingChanged += (s, e) =>
            { WorldOptimizations.EnableCityPointLights(_enableCityPointLights.Value); };
            (_citySpotLightShadows = Config.Bind("World Optimizations", "City Spot Light Shadows", LightShadows.Hard, "(ILLUSION DEFAULT Hard) Turn shadows on/off for the city lights on the main island")).SettingChanged += (s, e) =>
            { WorldOptimizations.SetCityLightShadows(_citySpotLightShadows.Value, _cityPointLightShadows.Value); };
            (_citySpotLightIntensity = Config.Bind("World Optimizations", "City Spot Lights Intensity", 2.0f, new ConfigDescription("(ILLUSION DEFAULT 1.5) City spot light intensity. Can be increased to counteract loss of point lights if they are disabled.", new AcceptableValueRange<float>(0, 4)))).SettingChanged += (s, e) =>
            { WorldOptimizations.SetCitySpotLightIntensity(_citySpotLightIntensity.Value); };
            (_moveTerrainLayer = Config.Bind("World Optimizations", "Move Main Island Terrain to Large Object/Terrain Layer", true, "(ILLUSION DEFAULT false) Moves the terrain to an unused layer (Terrain Layer) so it can be clipped")).SettingChanged += (s, e) =>
            { WorldOptimizations.MoveTerrainLayer(_moveTerrainLayer.Value); };
            (_LODBias = Config.Bind("World Optimizations", "Terrain Level of Detail Bias", 2.0f, new ConfigDescription("(ILLUSION DEFAULT 2.0) Deterimines how far away to swith to higher detail models. Higher values means use higher detail further away", new AcceptableValueRange<float>(0, 10)))).SettingChanged += (s, e) =>
            { QualitySettings.lodBias = _LODBias.Value; };
            (_maximumLOD = Config.Bind("World Optimizations", "Terrain Level of Detail Maximum", 0, new ConfigDescription("(ILLUSION DEFAULT 0) Maximum Level of Detail to use for objects. LOD 0 is highest detail, LOD 3 is lowest", new AcceptableValueRange<int>(0, 3)))).SettingChanged += (s, e) =>
            { QualitySettings.maximumLODLevel = _maximumLOD.Value; };
            (_basemapDistance = Config.Bind("World Optimizations", "Terrain Detail Distance", 300f, new ConfigDescription("(ILLUSION DEFAULT 0 for Main Island, 2000 for Residential Island) Distance to render high res textures for the terrain", new AcceptableValueRange<float>(0, 10000)))).SettingChanged += (s, e) =>
            { WorldOptimizations.SetTerrainDetailDistance(_basemapDistance.Value); };
            (_drawTreesAndFoliage = Config.Bind("World Optimizations", "Terrain Trees and Foliage", true, "(ILLUSION DEFAULT true) Set to false to disable rendering trees and foliage baked in to the terrain")).SettingChanged += (s, e) =>
            { WorldOptimizations.EnableTreeRendering(_drawTreesAndFoliage.Value); };
            (_terrainCastShadows = Config.Bind("World Optimizations", "Terrain Casts Shadows", false, "(ILLUSION DEFAULT true) Turn shadows on/off for the terrain on Residential Island")).SettingChanged += (s, e) =>
            { WorldOptimizations.SetTerrainShadows(_terrainCastShadows.Value); };
            (_animalAnimatorCulling = Config.Bind("World Optimizations", "Animal Animator Culling", AnimatorCullingMode.CullCompletely, "(ILLUSION DEFAULT CullUpdateTransforms) What animal animators should do when they are not visible")).SettingChanged += (s, e) =>
            { AnimalOptimizations.UpdateAnimatorCulling(_animalAnimatorCulling.Value); };
            (_worldAnimatorCulling = Config.Bind("World Optimizations", "Main Island Animator Culling", AnimatorCullingMode.CullCompletely, "(ILLUSION DEFAULT AlwaysAnimate) What world animators should do when they are not visible")).SettingChanged += (s, e) =>
            { WorldOptimizations.UpdateAnimatorCulling(_worldAnimatorCulling.Value); };

    //        (_simplifySunPosition = Config.Bind("World Optimizations", "Simplify Sun Position", false, "(ILLUSION DEFAULT false) What world animators should do when they are not visible")).SettingChanged += (s, e) =>
  //          {};

            _commandUpdateRate = Config.Bind("Player Optimizations", "Command Update Rate", 15, new ConfigDescription("Number of frames to spread out command checks, lower values = more responsive but lower perfomance", new AcceptableValueRange<int>(1, 60)));
            _UIMenuUpdateRate = Config.Bind("Player Optimizations", "Inactive UI Update Rate", 5, new ConfigDescription("Number of frames to spread out checks to see if a UI has become active and needs to be displayed, lower values = more responsive but lower perfomance", new AcceptableValueRange<int>(1, 60)));
            _enableInactiveUIChecks = Config.Bind("Player Optimizations", "Inactive UI Checks", true, "When enabled, disables UI windows that aren't visible, so they aren't being rendered in the background");
            (_playerDynamicBones = Config.Bind("Player Optimizations", "Dynamic Bones", false, "(ILLUSION DEFAULT true) Enable/disable player dynamic bones")).SettingChanged += (s, e) =>
            { CharacterOptimizations.SetPlayerDynamicBones(_playerDynamicBones.Value); };

            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
            Harmony.CreateAndPatchAll(typeof(AIMainGameOptimizations));
        }

        private static int housingColliderUpdateCount = 0;
        private static int housingParticleUpdateCount = 0;
        private static int dynamicBoneUpdateCount = 0;
        private static int UIUpdateCount = 0;
        private static int animalUpdateCount = 0;
        private const int AnimalUpdateRate = 6000;
        private void Update()
        {
            if (!_bMapLoaded)
                return;

            // spread the check out over multiple frames
            if (++housingColliderUpdateCount >= _housingColliderUpdateRate.Value)
                housingColliderUpdateCount = 0;

            if (++housingParticleUpdateCount >= _housingParticleUpdateRate.Value)
                housingParticleUpdateCount = 0;

            if (++dynamicBoneUpdateCount >= _dynamicBoneUpdateRate.Value)
                dynamicBoneUpdateCount = 0;

            HousingOptimizations.HousingColliderCheck(housingColliderUpdateCount, _housingColliderUpdateRate.Value);
            HousingOptimizations.HousingLightAndParticleCheck(housingParticleUpdateCount, _housingParticleUpdateRate.Value);
            CharacterOptimizations.DynamicBoneCheck(dynamicBoneUpdateCount, _dynamicBoneUpdateRate.Value);

            if (!_enableInactiveUIChecks.Value)
                return;

            if (++UIUpdateCount >= _UIMenuUpdateRate.Value)
                UIUpdateCount = 0;

            UIOptimizations.UIActiveCheck(UIUpdateCount, _UIMenuUpdateRate.Value);

            if (++animalUpdateCount >= AnimalUpdateRate)
            {
                animalUpdateCount = 0;
                AnimalOptimizations.UpdateAnimatorCulling(_animalAnimatorCulling.Value);
            }


        }
/*
        [HarmonyPrefix, HarmonyPatch(typeof(AIChara.ChaControl), "LateUpdateForce")]
        public static bool AICharaChaControl_LateUpdateForce(AIChara.ChaControl __instance)
        {
            if (!_characterLateUpdateChecks.Value)
                return true;

            if (__instance = null)
                return false;

            return (__instance.IsVisibleInCamera);
        }

        /*
                [HarmonyPrefix, HarmonyPatch(typeof(EnviroSky), "UpdateSunAndMoonPosition")]
                public static bool EnviroSky_UpdateSunAndMoonPosition()
                {
                    return !_simplifySunPosition.Value;
                }


                private static Stopwatch stopWatch = new Stopwatch();
                [HarmonyPrefix, HarmonyPatch(typeof(EnviroSky), "Update")]
                public static void EnviroSky_PreUpdate()
                {
                    stopWatch = new Stopwatch();
                    stopWatch.Start();
                }

                [HarmonyPostfix, HarmonyPatch(typeof(EnviroSky), "Update")]
                public static void EnviroSky_PostUpdate()
                {
                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;

                    Console.WriteLine($"EnviroSky_tUpdate {ts.TotalMilliseconds} ms");
                }
        */
        [HarmonyPostfix, HarmonyPatch(typeof(EnviroSky), "AssignAndStart")]
        private static void EnviroSky_AssignAndStart(EnviroSky __instance, GameObject player, Camera Camera)
        {
            _playerObject = player;
            _playerCamera = Camera;

            Console.WriteLine("EnviroSky_AssignAndStart");

            CameraOptimizations.InitializeCamera(Camera, 0.1f, 10000f);
            MainLightOptimizations.InitializeLighting(__instance.Components.DirectLight.GetComponent<Light>(), 10000f);

            CameraOptimizations.UpdateCameraSphericalCulling(_sphericalCulling.Value);
            CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.CharaLayer, _characterClipDistance.Value);
            CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.MapLayer, _worldClipDistance.Value);
            CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.MediumObjectLayer, _mediumClipDistance.Value);
            CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.SmallObjectLayer, _smallClipDistance.Value);
            CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.LargeObjectLayer, _terrainClipDistance.Value);
            CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.WaterLayer, _waterClipDistance.Value);
            MainLightOptimizations.UpdateShadowClipPlane((int)CameraOptimizations.CameraLayer.CharaLayer, _characterShadowDistance.Value);
            MainLightOptimizations.UpdateShadowClipPlane((int)CameraOptimizations.CameraLayer.MapLayer, _worldShadowDistance.Value);
            MainLightOptimizations.UpdateShadowClipPlane((int)CameraOptimizations.CameraLayer.MediumObjectLayer, _mediumShadowDistance.Value);
            MainLightOptimizations.UpdateShadowClipPlane((int)CameraOptimizations.CameraLayer.SmallObjectLayer, _smallShadowDistance.Value);
            MainLightOptimizations.UpdateShadowClipPlane((int)CameraOptimizations.CameraLayer.LargeObjectLayer, _largeShadowDistance.Value);
            MainLightOptimizations.UpdateShadowClipPlane((int)CameraOptimizations.CameraLayer.WaterLayer, _waterShadowDistance.Value);
            QualitySettings.shadowProjection = _shadowProjection.Value;
            QualitySettings.shadowDistance = _shadowDistance.Value;
            QualitySettings.lodBias = _LODBias.Value;
            QualitySettings.maximumLODLevel = _maximumLOD.Value;
        }

        private static readonly Dictionary<AIProject.AgentActor, int> _updateThrottleList = new Dictionary<AIProject.AgentActor, int>();
        [HarmonyPrefix, HarmonyPatch(typeof(AIProject.SearchArea), "OnUpdate")]
        public static bool SearchAreaUpdatePrefix(AIProject.AgentActor ____agent)
        {
            if (____agent == null)
                return true;

            if (!_updateThrottleList.TryGetValue(____agent, out _))
            {
                int updateCount = _updateThrottleList.Count + 1;
                _updateThrottleList.Add(____agent, updateCount);
            }

            if (++_updateThrottleList[____agent] <= _searchUpdateRate.Value)
                return false;

            _updateThrottleList[____agent] = 0;
            return true;
        }

        private static int commandCount = 0;
        [HarmonyPrefix, HarmonyPatch(typeof(AIProject.CommandArea), "Update")]
        public static bool CommandAreaUpdatePrefix()
        {
            if (++commandCount <= _commandUpdateRate.Value)
                return false;

            commandCount = 0;
            return true;
        }
  
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "SetStartVoice")]
        public static void HSceneSetStartVoicePrefix(HScene __instance)
        {
            if (__instance == null)
                return;

            if (_hSceneClipDistance.Value < _characterClipDistance.Value)
                CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.CharaLayer, _hSceneClipDistance.Value);

            if (_hSceneClipDistance.Value < _worldClipDistance.Value)
                CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.MapLayer, _hSceneClipDistance.Value);

            if (_hSceneClipDistance.Value < _mediumClipDistance.Value)
                CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.MediumObjectLayer, _hSceneClipDistance.Value);

            if (_hSceneClipDistance.Value < _smallClipDistance.Value)
                CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.SmallObjectLayer, _hSceneClipDistance.Value);

            if (_hSceneClipDistance.Value < _terrainClipDistance.Value)
                CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.LargeObjectLayer, _hSceneClipDistance.Value);

            if (_hSceneClipDistance.Value < _waterClipDistance.Value)
                CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.WaterLayer, _hSceneClipDistance.Value);

            AIChara.ChaControl[] females = __instance.GetFemales();

            CharacterOptimizations.InitializeHScene(females);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "EndProc")]
        public static void HSceneEndProcPostfix()
        {
            CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.CharaLayer, _characterClipDistance.Value);
            CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.MapLayer, _worldClipDistance.Value);
            CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.MediumObjectLayer, _mediumClipDistance.Value);
            CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.SmallObjectLayer, _smallClipDistance.Value);
            CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.LargeObjectLayer, _terrainClipDistance.Value);
            CameraOptimizations.UpdateCameraFarClipPlane(CameraOptimizations.CameraLayer.WaterLayer, _waterClipDistance.Value);
            CharacterOptimizations.SetPlayerDynamicBones(_playerDynamicBones.Value);
            CharacterOptimizations.UpdateAnimatorCulling(_characterAnimationCulling.Value);
            CharacterOptimizations.EndHScene();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Manager.Map), "RemoveAgent")]
        public static void MapManager_RemoveAgent(AIProject.AgentActor agent)
        {
            Console.WriteLine($"MapManager_RemoveAgent");
            if (_bMapLoaded && agent != null)
            {
                Console.WriteLine($"ChaControl {agent.ChaControl}");
                CharacterOptimizations.RemoveCharacter(agent.ChaControl);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Manager.Map), "AddAgent")]
        public static void MapManager_AddAgent(AIProject.AgentActor __result)
        {
            Console.WriteLine($"MapManager_AddAgent");
            if (_bMapLoaded && __result != null)
            {
                Console.WriteLine($"ChaControl {__result.ChaControl}");
                CharacterOptimizations.AddCharacter(__result.ChaControl);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Manager.Map), "InitSearchActorTargetsAll")]
        public static void MapManager_InitSearchActorTargetsAll()
        {
            Console.WriteLine("MapManager_InitSearchActorTargetsAll");
            InitializeOptimizers();
            _bMapLoaded = true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Manager.Map), "ReleaseMap")]
        public static void MapManager_ReleaseMap()
        {
            Console.WriteLine("MapManager_ReleaseMap");
            DestroyOptimizers();
            _bMapLoaded = false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Manager.Housing), "EndHousing")]
        private static void Housing_EndHousing()
        {
            Console.WriteLine("Housing_EndHousing");
            InitializeOptimizers();
            _bMapLoaded = true;
        }

        public static void InitializeOptimizers()
        {
            if (_playerObject == null || _playerCamera == null)
                return;

            HousingOptimizations.InitializeHousingOptimizations(_playerObject, _playerCamera, _footstepRange.Value, _cameraColliderRange.Value, _particleEmitterRange.Value, _housingAnimatorCulling.Value, _moveHousingLayer.Value, _housingLargeObjectSize.Value, _housingSmallObjectSize.Value, _housingDisableShadowHeight.Value);
            CharacterOptimizations.InitializeCharacterOptimizations(_playerObject, _dynamicBoneGenitalRange.Value, _dynamicBoneHairRange.Value, _dynamicBoneClothingRange.Value, _dynamicBoneBodyRange.Value, _characterAnimationCulling.Value);
            UIOptimizations.InitializeUserInterfaceOptimizations();
            WorldOptimizations.InitializeWorldOptimizations(_basemapDistance.Value, _terrainCastShadows.Value, _moveTerrainLayer.Value, _drawTreesAndFoliage.Value, _citySpotLightShadows.Value, _cityPointLightShadows.Value, _enableCityPointLights.Value, _citySpotLightIntensity.Value, _worldAnimatorCulling.Value);
            AnimalOptimizations.InitializeAnimalOptimizations(_animalAnimatorCulling.Value);
            CharacterOptimizations.SetPlayerDynamicBones(_playerDynamicBones.Value);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Manager.Housing), "StartHousing")]
        private static void Housing_StartHousing()
        {
            Console.WriteLine("Housing_StartHousing");
            DestroyOptimizers();
            _bMapLoaded = false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AIChara.CmpBoneBody), "EnableDynamicBonesBustAndHip")]
        private static bool CmpBoneBody_EnableDynamicBonesBustAndHip()
        {
            return _enableIllusionDynamicBoneChecks.Value;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AIChara.CmpHair), "EnableDynamicBonesHair")]
        private static bool CmpHair_EnableDynamicBonesHair()
        {
            return _enableIllusionDynamicBoneChecks.Value;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AIChara.CmpBase), "EnableDynamicBones")]
        private static bool CmpBase_EnableDynamicBones()
        {
            return _enableIllusionDynamicBoneChecks.Value;
        }

        private static void SceneManager_sceneUnloaded(Scene scene)
        {
            Console.WriteLine($"SceneManager_sceneunloaded { scene.name}");
            _bMapLoaded = false;
            DestroyOptimizers();
        }

        public static void DestroyOptimizers()
        {
            HousingOptimizations.DestroyOptimizers();
            CharacterOptimizations.DestroyOptimizers();
            UIOptimizations.DestroyOptimizers();
            WorldOptimizations.DestroyOptimizers();
        }
    }
}
