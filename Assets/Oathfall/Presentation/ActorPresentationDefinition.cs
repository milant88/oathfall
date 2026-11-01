using UnityEngine;

namespace Oathfall.Presentation
{
    [CreateAssetMenu(menuName = "Oathfall/Presentation/Actor Definition", fileName = "ActorPresentationDefinition")]
    public sealed class ActorPresentationDefinition : ScriptableObject
    {
        [SerializeField] private string stableId;
        [SerializeField] private GameObject modelPrefab;
        [SerializeField] private Avatar avatar;
        public GameObject ModelPrefab => modelPrefab;
        public Avatar Avatar => avatar;
        [SerializeField] private WeaponMotionLibrary weaponMotion;
        public WeaponMotionLibrary WeaponMotion=>weaponMotion;
        [SerializeField] private AudioClip swingSound, impactSound, stepSound, blockSound;
        [SerializeField] private string weaponSocket="WeaponSocket",shieldSocket="ShieldSocket";
        public AudioClip SwingSound=>swingSound;
        public AudioClip ImpactSound=>impactSound;
        public AudioClip StepSound=>stepSound;
        public AudioClip BlockSound=>blockSound;
        public string WeaponSocket=>weaponSocket;
        public string ShieldSocket=>shieldSocket;
        [SerializeField] private ActorPresentationKind kind;
        [SerializeField] private RuntimeAnimatorController animatorController;
        [SerializeField] private Material skinMaterial;
        [SerializeField] private Material primaryMaterial;
        [SerializeField] private Material secondaryMaterial;
        [SerializeField] private Material metalMaterial;
        [SerializeField] private Material accentMaterial;
        [SerializeField] private Material telegraphMaterial;
        [SerializeField] private Material impactMaterial;
        public Material ImpactMaterial=>impactMaterial;

        public string StableId => stableId;
        public ActorPresentationKind Kind => kind;
        public RuntimeAnimatorController AnimatorController => animatorController;
        public Material SkinMaterial => skinMaterial;
        public Material PrimaryMaterial => primaryMaterial;
        public Material SecondaryMaterial => secondaryMaterial;
        public Material MetalMaterial => metalMaterial;
        public Material AccentMaterial => accentMaterial;
        public Material TelegraphMaterial => telegraphMaterial;
    }
}
