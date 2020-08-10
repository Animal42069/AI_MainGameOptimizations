using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace EnvironmentalLighting
{
    [BepInPlugin(GUID, "Environmental Lighting", VERSION)]
    [BepInProcess("AI-Syoujyo")]
    public class AIEnvironmentalLighting : BaseUnityPlugin
    {
        public const string VERSION = "1.0.0.0";
        private const string GUID = "animal42069.aienvironmentlighting";
        private static Harmony harmony;
        private static ConfigEntry<bool> _dhh_override;
        private static ConfigEntry<float> _sun_intensity_multipier;
        private static ConfigEntry<float> _sun_intensity_offset;
        private static ConfigEntry<float> _sun_shadow_multipier;
        private static ConfigEntry<float> _moon_intensity_multipier;
        private static ConfigEntry<float> _moon_intensity_offset;
        private static ConfigEntry<float> _moon_shadow_multipier;
        private static ConfigEntry<float> _ambient_day_intensity_multiplier;
        private static ConfigEntry<float> _ambient_day_sky_intensity_multiplier;
        private static ConfigEntry<float> _ambient_day_equator_intensity_multiplier;
        private static ConfigEntry<float> _ambient_day_ground_intensity_multiplier;
        private static ConfigEntry<float> _ambient_night_intensity_multiplier;
        private static ConfigEntry<float> _ambient_night_sky_intensity_multiplier;
        private static ConfigEntry<float> _ambient_night_equator_intensity_multiplier;
        private static ConfigEntry<float> _ambient_night_ground_intensity_multiplier;
        private static ConfigEntry<AmbientMode> _ambient_mode;

        private static float directLightIntensity = 0;
        private static float directLightShadowStrength = 0;
        private static float ambientIntensity = 0;
        private static Color ambientSkyColor = new Color(0, 0, 0, 0);
        private static Color ambientEquatorColor = new Color(0, 0, 0, 0);
        private static Color ambientGroundColor = new Color(0, 0, 0, 0);

        private static float nextAmbientSkyUpdate = 0;

        private void Awake()
        {
            _dhh_override = Config.Bind("Graphics DHH", "Override", true, "Replace DHH's static light intensity values with the original dynamic environmental lighting");
            _sun_intensity_multipier = Config.Bind("Sunlight", "Intensity Multiplier", 1.0f, "Amount to multiply sun direct light intensity by before applying.");
            _sun_intensity_offset = Config.Bind("Sunlight", "Intensity Offset", 0.0f, "Amount to offset sun direct light intensity by before applying.");
            _sun_shadow_multipier = Config.Bind("Sunlight", "Shadow Strength Multiplier", 1.0f, "Amount to multiply sun direct light shadow strength by before applying.");
            _moon_intensity_multipier = Config.Bind("Moonlight", "Intensity Multiplier", 1.0f, "Amount to multiply moon direct light intensity by before applying.");
            _moon_intensity_offset = Config.Bind("Moonlight", "Intensity Offset", 0.0f, "Amount to offset moon direct light intensity by before applying.");
            _moon_shadow_multipier = Config.Bind("Moonlight", "Shadow Strength Multiplier", 1.0f, "Amount to multiply moon direct shadow strength by before applying.");
            _ambient_mode = Config.Bind("Ambient Light", "Ambient Mode", AmbientMode.Trilight, "Ambient light mode to use");
            _ambient_day_intensity_multiplier = Config.Bind("Ambient Daylight", "Intensity Multiplier", 1.0f, "Amount to multiply ambient light intensity by before applying.");
            _ambient_day_sky_intensity_multiplier = Config.Bind("Ambient Daylight", "Trilight Sky Intensity Multiplier", 1.0f, "Amount to multiply Trilight ambient light intensity by before applying.");
            _ambient_day_equator_intensity_multiplier = Config.Bind("Ambient Daylight", "Trilight Equator Intensity Multiplier", 1.0f, "Amount to multiply Trilight ambient light intensity by before applying.");
            _ambient_day_ground_intensity_multiplier = Config.Bind("Ambient Daylight", "Trilight Ground Intensity Multiplier", 1.0f, "Amount to multiply Trilight ambient light intensity by before applying.");
            _ambient_night_intensity_multiplier = Config.Bind("Ambient Nightlight", "Intensity Multiplier", 1.0f, "Amount to multiply ambient light intensity by before applying.");
            _ambient_night_sky_intensity_multiplier = Config.Bind("Ambient Nightlight", "Trilight Sky Intensity Multiplier", 1.0f, "Amount to multiply Trilight ambient light intensity by before applying.");
            _ambient_night_equator_intensity_multiplier = Config.Bind("Ambient Nightlight", "Trilight Equator Intensity Multiplier", 1.0f, "Amount to multiply Trilight ambient light intensity by before applying.");
            _ambient_night_ground_intensity_multiplier = Config.Bind("Ambient Nightlight", "Trilight Ground Intensity Multiplier", 1.0f, "Amount to multiply Trilight ambient light intensity by before applying.");

            harmony = new Harmony("AIEnvironmentalLighting");
            harmony.PatchAll(typeof(AIEnvironmentalLighting));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(EnviroSky), "Update")]
        private static void EnviroSky_Update(EnviroSky __instance, Light ___MainLight)
        {
            float solarTime = __instance.GameTime.solarTime;

            if (solarTime > 0.45)
            {
                directLightIntensity = (___MainLight.intensity * _sun_intensity_multipier.Value) + _sun_intensity_offset.Value;
                directLightShadowStrength = ___MainLight.shadowStrength * _sun_shadow_multipier.Value;
            }
            else if (solarTime > 0.4)
            {
                float sunIntensity = (solarTime - 0.4f) * 20f;
                float lunarIntensity = 1 - sunIntensity;

                directLightIntensity = (((__instance.lightSettings.directLightSunIntensity.Evaluate(solarTime) * _sun_intensity_multipier.Value) + _sun_intensity_offset.Value) * sunIntensity)
                                     + (((__instance.lightSettings.directLightMoonIntensity.Evaluate(__instance.GameTime.lunarTime) * _moon_intensity_multipier.Value) + _moon_intensity_offset.Value) * lunarIntensity);
                directLightShadowStrength = ___MainLight.shadowStrength * (_sun_shadow_multipier.Value * sunIntensity + _moon_shadow_multipier.Value * lunarIntensity);
                __instance.Components.DirectLight.rotation = Quaternion.Lerp(__instance.Components.Moon.transform.rotation, __instance.Components.Sun.transform.rotation, sunIntensity);
            }
            else
            {
                directLightIntensity = (___MainLight.intensity * _moon_intensity_multipier.Value) + _moon_intensity_offset.Value;
                directLightShadowStrength = ___MainLight.shadowStrength * _moon_shadow_multipier.Value;
            }

            if (RenderSettings.ambientMode == AmbientMode.Trilight)
            {
                ambientSkyColor = RenderSettings.ambientSkyColor;
                ambientEquatorColor = RenderSettings.ambientEquatorColor;
                ambientGroundColor = RenderSettings.ambientGroundColor;
            }
            else
            {
                ambientIntensity = RenderSettings.ambientIntensity;
                if (RenderSettings.ambientMode == AmbientMode.Skybox && (solarTime < 0.5 && solarTime > 0.4) &&
                   (nextAmbientSkyUpdate < __instance.internalHour || nextAmbientSkyUpdate > __instance.internalHour + 0.011f))
                {
                    DynamicGI.UpdateEnvironment();
                    nextAmbientSkyUpdate = __instance.internalHour + 0.01f;
                }
            }

            if (!_dhh_override.Value)
            {
                ___MainLight.intensity = directLightIntensity;
                ___MainLight.shadowStrength = directLightShadowStrength;
                ApplyAmbientIntensities(solarTime);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(EnviroSky), "LateUpdate")]
        private static void EnviroSky_LateUpdate(EnviroSky __instance, Light ___MainLight)
        {
            if (_ambient_mode.Value != RenderSettings.ambientMode)
            {
                __instance.lightSettings.ambientMode = _ambient_mode.Value;
                RenderSettings.ambientMode = _ambient_mode.Value;

                if (RenderSettings.ambientMode == AmbientMode.Skybox)
                {
                    DynamicGI.UpdateEnvironment();
                    nextAmbientSkyUpdate = __instance.internalHour + 0.01f;
                }

                Console.WriteLine("Ambient Mode changed to " + RenderSettings.ambientMode);
            }

            if (_dhh_override.Value)
            {
                ___MainLight.intensity = directLightIntensity;
                ___MainLight.shadowStrength = directLightShadowStrength;
                ApplyAmbientIntensities(__instance.GameTime.solarTime);
            }
        }

        private static void ApplyAmbientIntensities(float solarTime)
        {
            if (solarTime > 0.45)
            {
                if (RenderSettings.ambientMode == AmbientMode.Trilight)
                {
                    RenderSettings.ambientSkyColor = ambientSkyColor * _ambient_day_sky_intensity_multiplier.Value;
                    RenderSettings.ambientEquatorColor = ambientEquatorColor * _ambient_day_equator_intensity_multiplier.Value;
                    RenderSettings.ambientGroundColor = ambientGroundColor * _ambient_day_ground_intensity_multiplier.Value;
                }
                else
                {
                    RenderSettings.ambientIntensity = ambientIntensity * _ambient_day_intensity_multiplier.Value;
                }
            }
            else if (solarTime > 0.4)
            {
                float sunIntensity = (solarTime - 0.4f) * 20f;
                float lunarIntensity = 1 - sunIntensity;
                if (RenderSettings.ambientMode == AmbientMode.Trilight)
                {
                    RenderSettings.ambientSkyColor = ambientSkyColor * (_ambient_day_sky_intensity_multiplier.Value * sunIntensity + _ambient_night_sky_intensity_multiplier.Value * lunarIntensity);
                    RenderSettings.ambientEquatorColor = ambientEquatorColor * (_ambient_day_equator_intensity_multiplier.Value * sunIntensity + _ambient_night_equator_intensity_multiplier.Value * lunarIntensity);
                    RenderSettings.ambientGroundColor = ambientGroundColor * (_ambient_day_ground_intensity_multiplier.Value * sunIntensity + _ambient_night_ground_intensity_multiplier.Value * lunarIntensity);
                }
                else
                {
                    RenderSettings.ambientIntensity = ambientIntensity * (_ambient_day_intensity_multiplier.Value * sunIntensity + _ambient_night_intensity_multiplier.Value * lunarIntensity);
                }
            }
            else
            {
                if (RenderSettings.ambientMode == AmbientMode.Trilight)
                {
                    RenderSettings.ambientSkyColor = ambientSkyColor * _ambient_night_sky_intensity_multiplier.Value;
                    RenderSettings.ambientEquatorColor = ambientEquatorColor * _ambient_night_equator_intensity_multiplier.Value;
                    RenderSettings.ambientGroundColor = ambientGroundColor * _ambient_night_ground_intensity_multiplier.Value;
                }
                else
                {
                    RenderSettings.ambientIntensity = ambientIntensity * _ambient_night_intensity_multiplier.Value;
                }
            }
        }
    }
}
