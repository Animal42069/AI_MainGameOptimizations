using UnityEngine;

namespace AI_MainGameOptimizations
{
    class AnimalOptimizations
    {
        public static void InitializeAnimalOptimizations(AnimatorCullingMode cullingMode)
        {
            UpdateAnimatorCulling(cullingMode);
        }

        public static void UpdateAnimatorCulling(AnimatorCullingMode cullingMode)
        {
            var animalRoot = GameObject.Find("CommonSpace/MapRoot/SpawnPoint/AnimalRoot");

            if (animalRoot == null)
                return;

            var animalAnimators = animalRoot.GetComponentsInChildren<Animator>(true);

            foreach (var animator in animalAnimators)
                animator.cullingMode = cullingMode;
        }
    }
}
