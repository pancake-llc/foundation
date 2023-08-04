#if PANCAKE_SPINE
using System;
using System.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Pancake.Spine
{
    public static class Extension
    {
        public static SkeletonAnimation Play(this SkeletonAnimation skeleton, string animationName, bool loop = false)
        {
            skeleton.ClearState();
            skeleton.AnimationName = animationName;
            skeleton.loop = loop;
            skeleton.LateUpdate();
            skeleton.Initialize(true);

            return skeleton;
        }

        public static SkeletonGraphic Play(this SkeletonGraphic skeleton, string animationName, bool loop = false)
        {
            skeleton.Clear();
            skeleton.startingAnimation = animationName;
            skeleton.startingLoop = loop;
            skeleton.AnimationState.SetAnimation(0, animationName, loop);
            skeleton.LateUpdate();
            skeleton.Initialize(true);

            return skeleton;
        }

        public static SkeletonAnimation ChangeSkin(this SkeletonAnimation skeleton, string skinName)
        {
            var skin = new Skin("temp");
            skin.AddSkin(skeleton.skeleton.Data.FindSkin(skinName));
            skeleton.initialSkinName = "temp";
            skeleton.skeleton.SetSkin(skin);
            skeleton.skeleton.SetSlotsToSetupPose();
            skeleton.LateUpdate();
            skeleton.AnimationState.Apply(skeleton.skeleton);

            return skeleton;
        }

        public static SkeletonGraphic ChangeSkin(this SkeletonGraphic skeleton, string skinName)
        {
            var skin = new Skin("temp");
            skin.AddSkin(skeleton.Skeleton.Data.FindSkin(skinName));
            skeleton.Skeleton.SetSkin(skin);
            skeleton.Skeleton.SetSlotsToSetupPose();
            skeleton.LateUpdate();

            return skeleton;
        }

        public static SkeletonAnimation OnComplete(this SkeletonAnimation skeleton, Action onComplete, MonoBehaviour target = null)
        {
            var animation = skeleton.AnimationState.Data.SkeletonData.Animations.Items.FirstOrDefault(_ => _.Name == skeleton.AnimationName);
            if (animation == null) return skeleton;

            App.Delay(animation.Duration, onComplete, target: target);

            return skeleton;
        }

        public static SkeletonGraphic OnComplete(this SkeletonGraphic skeleton, Action onComplete, MonoBehaviour target = null)
        {
            var animation = skeleton.AnimationState.Data.SkeletonData.Animations.Items.FirstOrDefault(_ => _.Name == skeleton.startingAnimation);
            if (animation == null) return skeleton;

            App.Delay(animation.Duration, onComplete, target: target);

            return skeleton;
        }

        public static SkeletonAnimation OnUpdate(this SkeletonAnimation skeleton, Action<float> onUpdate, MonoBehaviour target = null)
        {
            var animation = skeleton.AnimationState.Data.SkeletonData.Animations.Items.FirstOrDefault(_ => _.Name == skeleton.AnimationName);
            if (animation == null) return skeleton;

            App.Delay(animation.Duration, null, onUpdate, target: target);

            return skeleton;
        }

        public static SkeletonGraphic OnUpdate(this SkeletonGraphic skeleton, Action<float> onUpdate, MonoBehaviour target = null)
        {
            var animation = skeleton.AnimationState.Data.SkeletonData.Animations.Items.FirstOrDefault(_ => _.Name == skeleton.startingAnimation);
            if (animation == null) return skeleton;

            App.Delay(animation.Duration, null, onUpdate, target: target);

            return skeleton;
        }

        public static SkeletonAnimation ChangeAttachment(this SkeletonAnimation skeleton, string slotName, string attachmentName)
        {
            var slotIndex = skeleton.skeleton.Data.FindSlot(slotName).Index;
            var attachment = skeleton.skeleton.GetAttachment(slotIndex, attachmentName);
            var skin = new Skin("temp");
            skin.SetAttachment(slotIndex, slotName, attachment);
            skeleton.skeleton.SetSkin(skin);
            skeleton.skeleton.SetSlotsToSetupPose();
            skeleton.LateUpdate();
            return skeleton;
        }

        public static SkeletonGraphic ChangeAttachment(this SkeletonGraphic skeleton, string slotName, string attachmentName) { return skeleton; }

        public static SkeletonAnimation MixSkin(this SkeletonAnimation skeleton, params string[] skinNames) { return skeleton; }

        public static SkeletonGraphic MixSkin(this SkeletonGraphic skeleton, params string[] skinNames) { return skeleton; }
    }
}
#endif