using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI_MainGameOptimizations
{
    public static class UIOptimizations
    {

        private static List<AIProject.UI.MenuUIBehaviour> UIMenuList = new List<AIProject.UI.MenuUIBehaviour>();
        private static readonly Dictionary<AIProject.UI.MenuUIBehaviour, bool> UIMenuInitialState = new Dictionary<AIProject.UI.MenuUIBehaviour, bool>();

        private static GameObject MiniMapObject;
        private static Transform MiniMapCamera;
        private static Transform AllAreaCamera;

        private static bool miniMapSkipEnabled = false;
        private static float miniMapNextUpdateTime = -1f;
        private static int miniMapVisibleMode = -1;

        public static void InitializeUserInterfaceOptimizations()
        {
            UIMenuList = BuildMenuUIList();
            GetMiniMapObjects();
        }

        private static void GetMiniMapObjects()
        {
            MiniMapObject = GameObject.Find("MiniMapObjects_07(Clone)");
            if (MiniMapObject == null)
                return;
            MiniMapCamera = MiniMapObject.GetComponentsInChildren<Transform>(true).Where(x => x.name.Contains("MinimapCamera")).FirstOrDefault();
            AllAreaCamera = MiniMapObject.GetComponentsInChildren<Transform>(true).Where(x => x.name.Contains("AllAreaCamera")).FirstOrDefault();
        }

        private static List<AIProject.UI.MenuUIBehaviour> BuildMenuUIList()
        {
            List<AIProject.UI.MenuUIBehaviour> UIList = new List<AIProject.UI.MenuUIBehaviour>();

            var gameObject = GameObject.Find("MapScene");
            if (gameObject == null)
                return UIList;

            AIProject.UI.MenuUIBehaviour[] UIMenuArray = gameObject.GetComponentsInChildren<AIProject.UI.MenuUIBehaviour>(true);
            if (UIMenuArray.IsNullOrEmpty())
                return UIList;

            foreach (var UIMenu in UIMenuArray)
            {
                if (!UIMenu.name.Contains("Clone") || UIMenuInitialState.TryGetValue(UIMenu, out _))
                    continue;

                UIList.Add(UIMenu);
                UIMenuInitialState.Add(UIMenu, UIMenu.gameObject.activeSelf);
            }
            return UIList;
        }

        public static void UIActiveCheck(int startIndex, int updateRate)
        {
            if (UIMenuList.IsNullOrEmpty())
                return;

            for (int index = startIndex; index < UIMenuList.Count; index += updateRate)
            {
                if (UIMenuList[index].gameObject == null)
                    continue;

                if (UIMenuList[index].gameObject.activeSelf != UIMenuList[index].IsActiveControl)
                    UIMenuList[index].gameObject.SetActive(UIMenuList[index].IsActiveControl);
            }

            if (startIndex == 0 && MiniMapObject != null && MiniMapCamera != null && AllAreaCamera != null)
            {
                bool cameraActive = MiniMapCamera.gameObject.activeSelf || AllAreaCamera.gameObject.activeSelf;

                if (MiniMapObject.activeSelf != cameraActive)
                    MiniMapObject.SetActive(cameraActive);
            }
        }

        public static void DestroyOptimizers()
        {
            if (UIMenuList != null)
            {
                foreach (var UIMenu in UIMenuList)
                {
                    if (UIMenu == null)
                        continue;

                    if (UIMenuInitialState.TryGetValue(UIMenu, out bool initialState))
                        UIMenu.gameObject.SetActive(initialState);
                }
            }

            UIMenuList.Clear();
            UIMenuInitialState.Clear();

            if (MiniMapObject != null)
                MiniMapObject.SetActive(true);

            MiniMapObject = null;
            MiniMapCamera = null;
            AllAreaCamera = null;
        }

        public static bool ShouldUpdateMiniMap()
        {
            return (!miniMapSkipEnabled || Time.fixedUnscaledTime >= miniMapNextUpdateTime);
        }

        public static void UpdateMiniMapTimer(int visibleMode, int miniMapMaxFPS)
        {
            if (miniMapVisibleMode != visibleMode)
            {
                miniMapVisibleMode = visibleMode;
                miniMapSkipEnabled = (visibleMode == 0);
                miniMapNextUpdateTime = -1f;
            }

            if (miniMapSkipEnabled && Time.fixedUnscaledTime > miniMapNextUpdateTime)
                miniMapNextUpdateTime = Time.fixedUnscaledTime + (1f / miniMapMaxFPS);
        }
    }
}
