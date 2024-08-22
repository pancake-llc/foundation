#if PANCAKE_SPINE
using System;
using System.Linq;
using Pancake.Common;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Pancake.Spine
{
    public static class Extension
    {
        public static float Duration(this SkeletonAnimation skeleton, string animationName)
        {
            var animation = skeleton.AnimationState.Data.SkeletonData.Animations.Items.FirstOrDefault(animation => animation.Name.Equals(animationName));
            if (animation == null) return 0;
            return animation.Duration;
        }

        public static float Duration(this SkeletonGraphic skeleton, string animationName)
        {
            var animation = skeleton.AnimationState.Data.SkeletonData.Animations.Items.FirstOrDefault(animation => animation.Name.Equals(animationName));
            if (animation == null) return 0;
            return animation.Duration;
        }

        public static float Duration(this SkeletonAnimation skeleton, int track = 0)
        {
            var animation = skeleton.AnimationState.GetCurrent(track);
            if (animation == null) return 0;
            return animation.Animation.Duration;
        }

        public static float Duration(this SkeletonGraphic skeleton, int track = 0)
        {
            var animation = skeleton.AnimationState.GetCurrent(track);
            if (animation == null) return 0;
            return animation.Animation.Duration;
        }

        public static SkeletonAnimation Play(this SkeletonAnimation skeleton, string animationName, bool loop = false)
        {
            skeleton.ClearState();
            skeleton.AnimationName = animationName;
            skeleton.loop = loop;
            skeleton.LateUpdate();
            skeleton.Initialize(true);

            return skeleton;
        }

        public static SkeletonGraphic PlayOnly(this SkeletonGraphic skeleton, string animationName, bool loop = false, int trackIndex = 0)
        {
            skeleton.startingAnimation = animationName;
            skeleton.AnimationState.SetAnimation(trackIndex, animationName, loop);
            return skeleton;
        }

        public static SkeletonAnimation PlayOnly(this SkeletonAnimation skeleton, string animationName, bool loop = false, int trackIndex = 0)
        {
            skeleton.AnimationName = animationName;
            skeleton.AnimationState.SetAnimation(trackIndex, animationName, loop);
            return skeleton;
        }

        public static SkeletonGraphic Play(this SkeletonGraphic skeleton, string animationName, bool loop = false, int trackIndex = 0)
        {
            skeleton.Clear();
            skeleton.startingAnimation = animationName;
            skeleton.startingLoop = loop;
            skeleton.AnimationState.SetAnimation(trackIndex, animationName, loop);
            skeleton.LateUpdate();
            skeleton.Initialize(true);

            return skeleton;
        }

        public static SkeletonAnimation PlayWithNextAnimation(
            this SkeletonAnimation skeleton,
            string animationName,
            string nextAnimation,
            bool loop = false,
            int trackIndex = 0,
            float delay = 0)
        {
            skeleton.AnimationName = animationName;
            skeleton.AnimationState.SetAnimation(trackIndex, animationName, loop);
            skeleton.AddAnimation(nextAnimation, loop, trackIndex, delay);
            return skeleton;
        }

        public static SkeletonAnimation AddAnimation(
            this SkeletonAnimation skeleton,
            string animationName,
            bool loop = false,
            int trackIndex = 0,
            float delay = 0,
            float mixDuration = 0f)
        {
            var trackEntry = skeleton.AnimationState.AddAnimation(trackIndex, animationName, loop, delay);
            trackEntry.MixDuration = mixDuration;
            return skeleton;
        }

        public static SkeletonAnimation AddEmptyAnimation(this SkeletonAnimation skeleton, int trackIndex = 0, float mixDuration = 0f, float delay = 0f)
        {
            skeleton.AnimationState.AddEmptyAnimation(trackIndex, mixDuration, delay);
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

        public static SkeletonAnimation ChangeSkins(this SkeletonAnimation skeleton, params string[] skinNames)
        {
            var skin = new Skin("temp");
            var skeletonData = skeleton.Skeleton.Data;
            foreach (string skinName in skinNames)
            {
                skin.AddSkin(skeletonData.FindSkin(skinName));
            }

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

        public static SkeletonGraphic ChangeSkins(this SkeletonGraphic skeleton, params string[] skinNames)
        {
            var skin = new Skin("temp");
            var skeletonData = skeleton.Skeleton.Data;
            foreach (string skinName in skinNames)
            {
                skin.AddSkin(skeletonData.FindSkin(skinName));
            }

            skeleton.Skeleton.SetSkin(skin);
            skeleton.Skeleton.SetSlotsToSetupPose();
            skeleton.LateUpdate();

            return skeleton;
        }


        public static SkeletonAnimation OnComplete(this SkeletonAnimation skeleton, Action onComplete, int track = 0, MonoBehaviour target = null)
        {
            App.Delay(target, skeleton.Duration(track), onComplete);
            return skeleton;
        }

        public static SkeletonGraphic OnComplete(this SkeletonGraphic skeleton, Action onComplete, int track = 0, MonoBehaviour target = null)
        {
            App.Delay(target, skeleton.Duration(track), onComplete);
            return skeleton;
        }

        public static SkeletonAnimation OnUpdate(this SkeletonAnimation skeleton, Action<float> onUpdate, int track = 0, MonoBehaviour target = null)
        {
            App.Delay(target, skeleton.Duration(track), null, onUpdate);
            return skeleton;
        }

        public static SkeletonGraphic OnUpdate(this SkeletonGraphic skeleton, Action<float> onUpdate, int track = 0, MonoBehaviour target = null)
        {
            App.Delay(target, skeleton.Duration(track), null, onUpdate);
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

        public static SkeletonGraphic ChangeAttachment(this SkeletonGraphic skeleton, string slotName, string attachmentName)
        {
            var slotIndex = skeleton.Skeleton.Data.FindSlot(slotName).Index;
            var attachment = skeleton.Skeleton.GetAttachment(slotIndex, attachmentName);
            var skin = new Skin("temp");
            skin.SetAttachment(slotIndex, slotName, attachment);
            skeleton.Skeleton.SetSkin(skin);
            skeleton.Skeleton.SetSlotsToSetupPose();
            skeleton.LateUpdate();
            return skeleton;
        }
    }
}
#endif