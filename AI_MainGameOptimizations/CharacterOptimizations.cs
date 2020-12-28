using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI_MainGameOptimizations
{
    public static class CharacterOptimizations
    {
        private enum VisibilityMask
        {
            None = 0,
            VaginaMask = 1 << 0,
            HairMask = 1 << 1,
            ClothingMask = 1 << 2,
            AllMask = ClothingMask | HairMask | VaginaMask
        };

        private static List<AIChara.ChaControl> _characters = new List<AIChara.ChaControl>();
        private static AIChara.ChaControl _playerCharacter;
        private static GameObject _playerObject;
        private static float _genitalRangeSquared = 1;
        private static float _hairRangeSquared = 40000;
        private static float _clothingRangeSquared = 40000;
        private static float _bodyRangeSquared = 40000;
        private static bool inHScene = false;
        private static AnimatorCullingMode _cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        public static void InitializeCharacterOptimizations(GameObject player, float genitalRange, float hairRange, float clothingRange, float bodyRange, AnimatorCullingMode cullingMode)
        {
            Console.WriteLine("InitializeCharacterOptimizations");

            _playerObject = player;
            _cullingMode = cullingMode;
            SetColliderRanges( genitalRange, hairRange, clothingRange, bodyRange);
            BuildCharacterList();
            UpdateAnimatorCulling(_cullingMode);
        }

        public static void SetColliderRanges(float genitalRange, float hairRange, float clothingRange, float bodyRange)
        {
            _genitalRangeSquared = genitalRange * genitalRange;
            _hairRangeSquared = hairRange * hairRange;
            _clothingRangeSquared = clothingRange * clothingRange;
            _bodyRangeSquared = bodyRange * bodyRange;
        }


        public static void DestroyOptimizers()
        {
            _characters = new List<AIChara.ChaControl>();
            _playerCharacter = null;
            _playerObject = null;
        }

        public static void AddCharacter(AIChara.ChaControl character)
        {
            if (character == null || character.isPlayer || (_characters != null && _characters.Exists(x => x.chaFile.charaFileName == character.chaFile.charaFileName)))
                return;

            _characters.Add(character);
            UpdateAnimatorCulling(_cullingMode);
        }

        public static void RemoveCharacter(AIChara.ChaControl character)
        {
            if (character == null || _characters == null || !_characters.Exists(x => x.chaFile.charaFileName == character.chaFile.charaFileName))
                return;

            _characters.Remove(character);
        }

        private static void BuildCharacterList()
        {
            _characters = new List<AIChara.ChaControl>();
            AIChara.ChaControl[] allCharacters = GameObject.FindObjectsOfType<AIChara.ChaControl>();

            foreach (var character in allCharacters)
            {
                if (!character.isPlayer)
                    _characters.Add(character);
                else
                    _playerCharacter = character;
            }
        }

        public static void DynamicBoneCheck(int startIndex, int updateRate)
        {
            if (_characters == null || inHScene || startIndex >= _characters.Count)
                return;

            for (int characterIndex = startIndex; characterIndex < _characters.Count; characterIndex += updateRate)
                DynamicBoneOptimize(_characters[characterIndex]);
        }

        private static void DynamicBoneOptimize(AIChara.ChaControl character)
        {
            VisibilityMask visibilityMask = VisibilityMask.None;
            bool bodyVisibleInRange = false;

            if (character.IsVisibleInCamera)
            {
                if (CheckDistance(character.cmpBoneBody.transform.position, _genitalRangeSquared))
                    visibilityMask |= VisibilityMask.VaginaMask;

                if ((visibilityMask != 0) || CheckDistance(character.cmpBoneBody.transform.position, _hairRangeSquared))
                    visibilityMask |= VisibilityMask.HairMask;

                if ((visibilityMask != 0) || CheckDistance(character.cmpBoneBody.transform.position, _clothingRangeSquared))
                    visibilityMask |= VisibilityMask.ClothingMask;

                if (CheckDistance(character.cmpBoneBody.transform.position, _bodyRangeSquared))
                    bodyVisibleInRange = true;
            }

            SetStateDynamicBones(character, character.IsVisibleInCamera, visibilityMask, bodyVisibleInRange, true);
        }

        private static void DynamicBoneForce(AIChara.ChaControl character, bool onState)
        {
            if (character == null)
                return;

            if (onState)
                SetStateDynamicBones(character, true, VisibilityMask.AllMask, true, true);
            else
                SetStateDynamicBones(character, false, VisibilityMask.None, false, true);
        }

        public static void SetPlayerDynamicBones(bool onState)
        {
            if (_playerCharacter == null)
                return;

            foreach (var dynamicBoneV2 in _playerCharacter.GetComponentsInChildren<DynamicBone_Ver02>(true))
            {
                if (!dynamicBoneV2)
                    continue;

                if (dynamicBoneV2.enabled != onState)
                    dynamicBoneV2.enabled = onState;
            }
        }

        private static bool CheckDistance(Vector3 transformPosition, float sqrMagnitudeRange)
        {
            if (_playerObject.transform == null)
                return false;

            return (_playerObject.transform.position - transformPosition).sqrMagnitude < sqrMagnitudeRange;
        }

        public static void UpdateAnimatorCulling(AnimatorCullingMode cullingMode)
        {
            if (_characters != null)
            {
                foreach (var character in _characters)
                    character.animBody.cullingMode = cullingMode;
            }

            if (_playerCharacter != null)
                _playerCharacter.animBody.cullingMode = cullingMode;
        }

        public static void InitializeHScene(AIChara.ChaControl[] hSceneFemales)
        {
            if (_playerCharacter == null || _characters == null || hSceneFemales == null)
                return;

            inHScene = true;
            foreach (AIChara.ChaControl character in _characters)
            {
                if (character == null)
                    continue;

                bool characterInHScene = false;
                foreach (AIChara.ChaControl hSceneFemale in hSceneFemales)
                {
                    if (hSceneFemale == null)
                        continue;

                    if (hSceneFemale.loadNo == character.loadNo)
                    {
                        characterInHScene = true;
                        break;
                    }            
                }

                DynamicBoneForce(character, characterInHScene);

                // make other characters invisible?
                if (!characterInHScene)
                {
                    Console.WriteLine($"Character {character.loadNo} Not in HScene");
                }
                else
                {
                    character.animBody.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                }
            }

            _playerCharacter.animBody.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            SetPlayerDynamicBones(true);
        }

        private static void SetStateDynamicBones(AIChara.ChaControl character, bool isVisible, VisibilityMask visibilityMask, bool isVisibleInBodyRange = false, bool setIllusionColliders = false)
        {
            if (AIMainGameOptimizations._IKSolverChecks.Value)
            {
                if (character.fullBodyIK.enabled != isVisible)
                    character.fullBodyIK.enabled = isVisible;
            }

            if (AIMainGameOptimizations._LookControllerChecks.Value)
            {
                if (character.neckLookCtrl.enabled != isVisible)
                    character.neckLookCtrl.enabled = isVisible;

                if (character.eyeLookCtrl.enabled != isVisible)
                    character.eyeLookCtrl.enabled = isVisible;
            }

            foreach (var dynamicBone in character.GetComponentsInChildren<DynamicBone>(true))
            {
                if (!dynamicBone)
                    continue;

                bool bIsVisibleInRange = isVisible;
                if (dynamicBone.m_Root != null)
                {
                    if (dynamicBone.m_Root.name.Contains("Vagina"))
                        bIsVisibleInRange = (visibilityMask & VisibilityMask.VaginaMask) == VisibilityMask.VaginaMask;
                    else if (dynamicBone.m_Root.name.Contains("hair"))
                        bIsVisibleInRange = (visibilityMask & VisibilityMask.HairMask) == VisibilityMask.HairMask;
                    else
                        bIsVisibleInRange = (visibilityMask & VisibilityMask.ClothingMask) == VisibilityMask.ClothingMask;
                }

                if (dynamicBone.enabled != bIsVisibleInRange)
                    dynamicBone.enabled = bIsVisibleInRange;
            }

            foreach (var dynamicBoneCollider in character.GetComponentsInChildren<DynamicBoneCollider>(true))
            {
                if (!dynamicBoneCollider)
                    continue;

                if (dynamicBoneCollider.enabled != isVisibleInBodyRange)
                    dynamicBoneCollider.enabled = isVisibleInBodyRange;
            }

            if (!setIllusionColliders)
                return;

            foreach (var dynamicBoneV2 in character.GetComponentsInChildren<DynamicBone_Ver02>(true))
            {
                if (!dynamicBoneV2)
                    continue;

                if (dynamicBoneV2.enabled != isVisibleInBodyRange)
                    dynamicBoneV2.enabled = isVisibleInBodyRange;
            }
        }

        public static void EndHScene()
        {
            inHScene = false;
            // make other characters visible?
        }
    }
}
