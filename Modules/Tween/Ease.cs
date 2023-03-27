using Unity.Burst;
using Unity.Mathematics;

namespace Pancake.Tween
{
    public enum Ease
    {
        Linear,
        Smooth,

        InQuad,
        OutQuad,
        InOutQuad,

        InCubic,
        OutCubic,
        InOutCubic,

        InQuart,
        OutQuart,
        InOutQuart,

        InQuint,
        OutQuint,
        InOutQuint,

        InSine,
        OutSine,
        InOutSine,

        InExpo,
        OutExpo,
        InOutExpo,
        InExpoOutBack,

        InCirc,
        OutCirc,
        InOutCirc,

        InElastic,
        OutElastic,
        InOutElastic,

        InBack,
        OutBack,
        InOutBack,
        InBackOutExpo,
        InBackOutElastic,

        InBounce,
        OutBounce,
        InOutBounce,

        /// <summary>
        /// This type needs to SetExtraParams(amplitude, speed) by TweenAction.
        /// </summary>
        ShakeX,

        /// <summary>
        /// This type needs to SetExtraParams(amplitude, spped) by TweenAction.
        /// </summary>
        ShakeY,

        /// <summary>
        /// This type needs to SetExtraParams(amplitude, spped) by TweenAction.
        /// </summary>
        ShakeZ,

        /// <summary>
        /// This type needs to SetExtraParams(posX) by TweenAction.
        /// </summary>
        BezierQuadraticX,

        /// <summary>
        /// This type needs to SetExtraParams(posY) by TweenAction.
        /// </summary>
        BezierQuadraticY,

        /// <summary>
        /// This type needs to SetExtraParams(posZ) by TweenAction.
        /// </summary>
        BezierQuadraticZ,

        /// <summary>
        /// This type needs to SetExtraParams(pos1X, pos2X) by TweenAction.
        /// </summary>
        BezierCubicX,

        /// <summary>
        /// This type needs to SetExtraParams(pos1Y, pos2Y) by TweenAction.
        /// </summary>
        BezierCubicY,

        /// <summary>
        /// This type needs to SetExtraParams(pos1Z, pos2Z) by TweenAction.
        /// </summary>
        BezierCubicZ,
    }


    [BurstCompile]
    internal static class TweenEaseFn
    {
        /// <summary>
        /// Eases one step by TweenEase.
        /// [values]: [x:from,    y:to, z:ease].
        /// [times] : [x:curTime, y:duration]
        /// </summary>
        [BurstCompile]
        internal static float Step(in float3 values, in float2 times)
        {
            var time = times.x / times.y;

            switch ((Ease) values.z)
            {
                case Ease.Linear:
                    break;

                case Ease.Smooth:
                    time = Smooth(time);
                    break;

                case Ease.InQuad:
                    time = InQuad(time);
                    break;
                case Ease.OutQuad:
                    time = OutQuad(time);
                    break;
                case Ease.InOutQuad:
                    time = InOutQuad(time);
                    break;

                case Ease.InCubic:
                    time = InCubic(time);
                    break;
                case Ease.OutCubic:
                    time = OutCubic(time);
                    break;
                case Ease.InOutCubic:
                    time = InOutCubic(time);
                    break;

                case Ease.InQuart:
                    time = InQuart(time);
                    break;
                case Ease.OutQuart:
                    time = OutQuart(time);
                    break;
                case Ease.InOutQuart:
                    time = InOutQuart(time);
                    break;

                case Ease.InQuint:
                    time = InQuint(time);
                    break;
                case Ease.OutQuint:
                    time = OutQuint(time);
                    break;
                case Ease.InOutQuint:
                    time = InOutQuint(time);
                    break;

                case Ease.InSine:
                    time = InSine(time);
                    break;
                case Ease.OutSine:
                    time = OutSine(time);
                    break;
                case Ease.InOutSine:
                    time = InOutSine(time);
                    break;

                case Ease.InExpo:
                    time = InExpo(time);
                    break;
                case Ease.OutExpo:
                    time = OutExpo(time);
                    break;
                case Ease.InOutExpo:
                    time = InOutExpo(time);
                    break;
                case Ease.InExpoOutBack:
                    time = InExpoOutBack(time);
                    break;

                case Ease.InCirc:
                    time = InCirc(time);
                    break;
                case Ease.OutCirc:
                    time = OutCirc(time);
                    break;
                case Ease.InOutCirc:
                    time = InOutCirc(time);
                    break;

                case Ease.InElastic:
                    time = InElastic(time);
                    break;
                case Ease.OutElastic:
                    time = OutElastic(time);
                    break;
                case Ease.InOutElastic:
                    time = InOutElastic(time);
                    break;

                case Ease.InBack:
                    time = InBack(time);
                    break;
                case Ease.OutBack:
                    time = OutBack(time);
                    break;
                case Ease.InOutBack:
                    time = InOutBack(time);
                    break;
                case Ease.InBackOutExpo:
                    time = InBackOutExpo(time);
                    break;
                case Ease.InBackOutElastic:
                    time = InBackOutElastic(time);
                    break;

                case Ease.InBounce:
                    time = InBounce(time);
                    break;
                case Ease.OutBounce:
                    time = OutBounce(time);
                    break;
                case Ease.InOutBounce:
                    time = InOutBounce(time);
                    break;
            }

            return math.lerp(values.x, values.y, time);
            // values.x + (values.y - values.x) * time;
        }


        /// <summary>
        /// Eases one step by TweenEase.
        /// [values]     : [x:from,    y:to, z:ease].
        /// [times]      : [x:curTime, y:duration].
        /// [extraParams]:
        ///     Shake           [0:amplitude, 1:speed]
        ///     BezierQuadratic [0:posX,      1:posY,  2:posZ]
        ///     BezierCubic     [0:pos1X,     1:pos2X, 2:pos1Y, 3:pos2Y, 4:pos1Z, 5:pos2Z]
        /// [index]      : the index of ease values.
        /// </summary>
        [BurstCompile]
        internal unsafe static float StepWithExtraParams(in float3 values, in float2 times, float* extraParams, int index)
        {
            var time = times.x / times.y;
            var ease = (Ease) values.z;

            switch (ease)
            {
                case Ease.ShakeX:
                case Ease.ShakeY:
                case Ease.ShakeZ:
                    return Shake(values.x,
                        values.y,
                        time,
                        extraParams[0],
                        extraParams[1],
                        ease);

                case Ease.BezierQuadraticX:
                case Ease.BezierQuadraticY:
                case Ease.BezierQuadraticZ:
                    return BezierQuadratic(values.x, values.y, time, extraParams[index]);

                // index 0 1 2
                //       x y z
                //       0 2 4
                //       1 3 5
                case Ease.BezierCubicX:
                case Ease.BezierCubicY:
                case Ease.BezierCubicZ:
                    index *= 2;
                    return BezierCubic(values.x,
                        values.y,
                        time,
                        extraParams[index],
                        extraParams[index + 1]);

                default:
                    break;
            }

            return 0.0f;
        }


        [BurstCompile]
        private static float Smooth(float time)
        {
            return math.mul(math.pow(time, 2.0f), math.mad(time, -2.0f, 3.0f));
            // time * time * (3.0f - 2.0f * time);
            // math.smoothstep(0.0f, 1.0f, time);
        }


        [BurstCompile]
        private static float InQuad(float time)
        {
            return math.pow(time, 2.0f);
            // time * time;
        }


        [BurstCompile]
        private static float OutQuad(float time)
        {
            return math.mul(time, 2.0f - time);
            // time * (2.0f - time);
        }


        [BurstCompile]
        private static float InOutQuad(float time)
        {
            if (time < 0.5f)
            {
                return math.mul(math.pow(time, 2.0f), 2.0f);
                // time * time * 2.0f; 
            }

            return math.mad(math.mul(2.0f, time), 2.0f - time, -1.0f);
            // 2.0f * time * (2.0f - time) - 1.0f;
        }


        [BurstCompile]
        private static float InCubic(float time)
        {
            return math.pow(time, 3.0f);
            // time * time * time;
        }


        [BurstCompile]
        private static float OutCubic(float time)
        {
            time -= 1.0f;
            return math.pow(time, 3.0f) + 1.0f;
            // time * time * time + 1.0f;
        }


        [BurstCompile]
        private static float InOutCubic(float time)
        {
            if (time < 0.5f)
            {
                return math.mul(4.0f, math.pow(time, 3.0f));
                // 4.0f * (time * time * time);
            }

            time -= 1.0f;
            return math.mad(4.0f, math.pow(time, 3.0f), 1.0f);
            // 4.0f * (time * time * time) + 1.0f;
        }


        [BurstCompile]
        private static float InQuart(float time)
        {
            return math.pow(time, 4.0f);
            // time * time * time * time; 
        }


        [BurstCompile]
        private static float OutQuart(float time)
        {
            time -= 1.0f;
            return 1.0f - math.pow(time, 4.0f);
            // 1.0f - time * time * time * time;
        }


        [BurstCompile]
        private static float InOutQuart(float time)
        {
            if (time < 0.5f)
            {
                return math.mul(8.0f, math.pow(time, 4.0f));
                // 8.0f * (time * time * time * time);
            }

            time -= 1.0f;
            return 1.0f - math.mul(8.0f, math.pow(time, 4.0f));
            // 1.0f - 8.0f * time * time * time * time;
        }


        [BurstCompile]
        private static float InQuint(float time)
        {
            return math.pow(time, 5.0f);
            // time * time * time * time * time;
        }


        [BurstCompile]
        private static float OutQuint(float time)
        {
            time -= 1.0f;
            return math.pow(time, 5.0f) + 1.0f;
            // time * time * time * time * time + 1.0f;
        }


        [BurstCompile]
        private static float InOutQuint(float time)
        {
            if (time < 0.5f)
            {
                return math.mul(16.0f, math.pow(time, 5.0f));
                // 16.0f * (time * time * time * time * time);
            }

            time -= 1.0f;
            return math.mad(16.0f, math.pow(time, 5.0f), 1.0f);
            // 16.0f * (time * time * time * time * time) + 1.0f;
        }


        [BurstCompile]
        private static float InSine(float time) { return 1.0f - math.cos(math.mul(time, 1.570796f)); }


        [BurstCompile]
        private static float OutSine(float time) { return math.sin(math.mul(time, 1.570796f)); }


        [BurstCompile]
        private static float InOutSine(float time)
        {
            return math.mad(-0.5f, math.cos(math.mul(time, 3.141593f)), 0.5f);
            // 0.5f -  0.5f * math.cos(time * 3.141593f);
            // 0.5f * (1.0f - math.cos(time * 3.141593f));
        }


        [BurstCompile]
        private static float InExpo(float time)
        {
            /* never come when time equals 0.0f
            if (time <= 0.0f)
            {
                return time;
            }
            */

            return math.pow(2.0f, math.mad(10.0f, time, -10.0f));
            // math.pow(2.0f, 10.0f * time - 10.0f);
        }


        [BurstCompile]
        private static float OutExpo(float time)
        {
            /* never come when time is 1.0f 
            if (time >= 1.0f)
            {
                return time;
            }
            */

            return 1.000977f - math.pow(2.0f, math.mul(-10.0f, time));
            // 1.0f + 1.0f / 1024.0f - math.pow(2.0f, -10.0f * time);
        }


        [BurstCompile]
        private static float InOutExpo(float time)
        {
            /* never come when time is 0.0f or 1.0f
            if (time <= 0.0f || time >= 1.0f)
            {
                return time;
            }
            */

            if (time < 0.5f)
            {
                return math.mul(0.5f, math.pow(2.0f, math.mad(20.0f, time, -10.0f)));
                // 0.5f * math.pow(2.0f, 20.0f * time - 10.0f);
            }

            return math.mad(-0.5f, math.pow(2.0f, math.mad(-20.0f, time, 10.0f)), 1.000488f);
            // 0.5f * (2.0f - math.pow(2.0f, 10.0f - 20.0f * time));
            // 1.0f + 1.0f / 2048.0f - 0.5f * math.pow(2.0f, 10.0f - 20.0f * time);
        }


        [BurstCompile]
        private static float InExpoOutBack(float time)
        {
            /* never come when time is 0.0f or 1.0f
            if (time <= 0.0f || time >= 1.0f)
            {
                return time;
            }
            */

            if (time < 0.5f)
            {
                return math.mul(0.5f, math.pow(2.0f, math.mad(20.0f, time, -10.0f)));
                // 0.5f * math.pow(2.0f, 20.0f * time - 10.0f);
            }

            time -= 1.0f;
            return math.mad(math.pow(time, 2.0f), math.mad(14.379638f, time, 5.189819f), 1.0f);
            // time * time * (14.379638f * time + 5.189819f) + 1.0f;
        }


        [BurstCompile]
        private static float InCirc(float time)
        {
            return 1.0f - math.sqrt(1.0f - math.pow(time, 2.0f));
            // 1.0f - math.sqrt(1.0f - time * time);
        }


        [BurstCompile]
        private static float OutCirc(float time)
        {
            return math.sqrt(math.mul(2.0f - time, time));
            // math.sqrt((2.0f - time) * time);
        }


        [BurstCompile]
        private static float InOutCirc(float time)
        {
            if (time < 0.5f)
            {
                return math.mad(-0.5f, math.sqrt(math.mad(-math.pow(time, 2.0f), 4.0f, 1.0f)), 0.5f);
                // 0.5f * (1.0f - math.sqrt(1.0f - time * time * 4.0f));
                // 0.5f -  0.5f * math.sqrt(1.0f - time * time * 4.0f);
            }

            time -= 1.0f;
            return math.mad(0.5f, math.sqrt(math.mad(-math.pow(time, 2.0f), 4.0f, 1.0f)), 0.5f);
            // 0.5f * (1.0f + math.sqrt(1.0f - time * time * 4.0f));
            // 0.5f +  0.5f * math.sqrt(1.0f - time * time * 4.0f);
        }


        [BurstCompile]
        private static float InElastic(float time)
        {
            /* never come when time is 0.0f or 1.0f
            if (time <= 0.0f || time >= 1.0f)
            {
                return time;
            }
            */

            return math.mul(-math.pow(2.0f, math.mad(10.0f, time, -10.0f)), math.sin(math.mad(20.943951f, time, -22.514747f)));
            // -math.pow(2.0f, 10.0f * time - 10.0f) * math.sin(20.943951f * time - 22.514747f);
        }


        [BurstCompile]
        private static float OutElastic(float time)
        {
            /* never come when time is 0.0f or 1.0f
            if (time <= 0.0f || time >= 1.0f)
            {
                return time;
            }
            */

            return math.mad(math.pow(2.0f, math.mul(-10.0f, time)), math.sin(math.mad(20.943951f, time, -1.570796f)), 0.999512f);
            // math.pow(2.0f, -10.0f * time) * math.sin(20.943951f * time - 1.570796f) +
            // 1.0f / 1024.0f * -math.sin(20.943951f - 1.570796f) + 1.0f;
        }


        [BurstCompile]
        private static float InOutElastic(float time)
        {
            /* never come when time is 0.0f or 1.0f
            if (time <= 0.0f || time >= 1.0f)
            {
                return time;
            }
            */

            if (time < 0.5f)
            {
                return math.mul(-0.5f, math.mul(math.pow(2.0f, math.mad(20.0f, time, -10.0f)), math.sin(math.mad(27.925268f, time, -15.533430f))));
                // -0.5f * math.pow(2.0f, 20.0f * time - 10.0f) * math.sin(27.925268f * time - 15.533430f);
            }

            return math.mad(0.5f, math.mul(math.pow(2.0f, math.mad(-20.0f, time, 10.0f)), math.sin(math.mad(27.925268f, time, -15.533430f))), 1.000171f);
            // 0.5f * math.pow(2.0f, -20.0f * time + 10.0f) * math.sin(27.925268f * time - 15.533430f) +
            // 1.0f / 1024.0f * -math.sin(27.925268f - 15.533430f) + 1.0f;
        }


        [BurstCompile]
        private static float InBack(float time)
        {
            return math.mul(math.pow(time, 2.0f), math.mad(2.70158f, time, -1.70158f));
            // time * time * (2.70158f * time - 1.70158f);
        }


        [BurstCompile]
        private static float OutBack(float time)
        {
            time -= 1.0f;
            return math.mad(math.pow(time, 2.0f), math.mad(2.70158f, time, 1.70158f), 1.0f);
            // time * time * (2.70158f * time + 1.70158f) + 1.0f;
        }


        [BurstCompile]
        private static float InOutBack(float time)
        {
            if (time < 0.5f)
            {
                return math.mul(math.pow(time, 2.0f), math.mad(14.379638f, time, -5.189819f));
                // time * time * (14.379638f * time - 5.189819f);
            }

            time -= 1.0f;
            return math.mad(math.pow(time, 2.0f), math.mad(14.379638f, time, 5.189819f), 1.0f);
            // time * time * (14.379638f * time + 5.189819f) + 1.0f;
        }


        [BurstCompile]
        private static float InBackOutExpo(float time)
        {
            if (time < 0.5f)
            {
                return math.mul(math.pow(time, 2.0f), math.mad(14.379638f, time, -5.189819f));
                // time * time * (14.379638f * time - 5.189819f);
            }

            return math.mad(-0.5f, math.pow(2.0f, math.mad(-20.0f, time, 10.0f)), 1.000488f);
            // 0.5f * (2.0f - math.pow(2.0f, 10.0f - 20.0f * time));
            // 1.0f + 1.0f / 2048.0f - 0.5f * math.pow(2.0f, 10.0f - 20.0f * time);
        }


        [BurstCompile]
        private static float InBackOutElastic(float time)
        {
            if (time < 0.5f)
            {
                return math.mul(math.pow(time, 2.0f), math.mad(14.379638f, time, -5.189819f));
                // time * time * (14.379638f * time - 5.189819f);
            }

            /* neve come when time is 1.0f
            if (time >= 1.0f)
            {
                return time;
            }
            */

            return math.mad(0.5f, math.mul(math.pow(2.0f, math.mad(-20.0f, time, 10.0f)), math.sin(math.mad(27.925268f, time, -15.533430f))), 1.000171f);
            // 0.5f * math.pow(2.0f, -20.0f * time + 10.0f) * math.sin(27.925268f * time - 15.533430f) +
            // 1.0f / 1024.0f * -math.sin(27.925268f - 15.533430f) + 1.0f;
        }


        [BurstCompile]
        private static float OutBounce(float time)
        {
            if (time < 0.363636f)
            {
                return math.mul(math.pow(time, 2.0f), 7.5625f);
                // time * time * 7.5625f;
            }

            if (time < 0.72727f)
            {
                time -= 0.545454f;
                return math.mad(math.pow(time, 2.0f), 7.5625f, 0.75f);
                // time * time * 7.5625f + 0.75f;
            }

            if (time < 0.909091f)
            {
                time -= 0.818182f;
                return math.mad(math.pow(time, 2.0f), 7.5625f, 0.9375f);
                // time * time * 7.5625f + 0.9375f;
            }

            /* neve come when time is 1.0f
            if (time >= 1.0f)
            {
                return time;
            }
            */

            time -= 0.954545f;
            return math.mad(math.pow(time, 2.0f), 7.5625f, 0.984375f);
            // time * time * 7.5625f + 0.984375f;
        }


        [BurstCompile]
        private static float InBounce(float time)
        {
            if (time > 0.636364f)
            {
                time = 1.0f - time;
                return math.mad(-math.pow(time, 2.0f), 7.5625f, 1.0f);
                // 1.0f - time * time * 7.5625f;
            }

            if (time > 0.27273f)
            {
                time = 0.454546f - time;
                return math.mad(-math.pow(time, 2.0f), 7.5625f, 0.25f);
                // 0.25f - time * time * 7.5625f;
            }

            if (time > 0.090909f)
            {
                time = 0.181818f - time;
                return math.mad(-math.pow(time, 2.0f), 7.5625f, 0.0625f);
                // 0.0625f - time * time * 7.5625f;
            }

            /* neve come when time is 1.0f
            if (time >= 1.0f)
            {
                return time;
            }
            */

            time = 0.045455f - time;
            return math.mad(-math.pow(time, 2.0f), 7.5625f, 0.015625f);
            // 0.015625f - time * time * 7.5625f;
        }


        [BurstCompile]
        private static float InOutBounce(float time)
        {
            if (time < 0.5f)
            {
                // bounce in
                if (time > 0.318182f)
                {
                    time = math.mad(-time, 2.0f, 1.0f);
                    // time = 1.0f - time * 2.0f;
                    return math.mad(-math.pow(time, 2.0f), 3.78125f, 0.5f);
                    // 0.5f - time * time * 3.78125f;
                }

                if (time > 0.136365f)
                {
                    time = math.mad(-time, 2.0f, 0.454546f);
                    // time = 0.454546f - time * 2.0f;
                    return math.mad(-math.pow(time, 2.0f), 3.78125f, 0.125f);
                    // 0.125f - time * time * 3.78125f;
                }

                if (time > 0.045455f)
                {
                    time = math.mad(-time, 2.0f, 0.181818f);
                    // time = 0.181818f - time * 2.0f;
                    return math.mad(-math.pow(time, 2.0f), 3.78125f, 0.03125f);
                    // 0.03125f - time * time * 3.78125f;
                }

                time = math.mad(-time, 2.0f, 0.045455f);
                // time = 0.045455f - time * 2.0f;
                return math.mad(-math.pow(time, 2.0f), 3.78125f, 0.007813f);
                // 0.007813f - time * time * 3.78125f;
            }

            // bounce out
            if (time < 0.681818f)
            {
                time = math.mad(time, 2.0f, -1.0f);
                // time = time * 2.0f - 1.0f;
                return math.mad(math.pow(time, 2.0f), 3.78125f, 0.5f);
                // time * time * 3.78125f + 0.5f;
            }

            if (time < 0.863635f)
            {
                time = math.mad(time, 2.0f, -1.545454f);
                // time = time * 2.0f - 1.545454f;
                return math.mad(math.pow(time, 2.0f), 3.78125f, 0.875f);
                // time * time * 3.78125f + 0.875f;
            }

            if (time < 0.954546f)
            {
                time = math.mad(time, 2.0f, -1.818182f);
                // time = time * 2.0f - 1.818182f;
                return math.mad(math.pow(time, 2.0f), 3.78125f, 0.96875f);
                // time * time * 3.78125f + 0.96875f;
            }

            /* neve come when time is 1.0f
            if (time >= 1.0f)
            {
                return time;
            }
            */

            time = math.mad(time, 2.0f, -1.954545f);
            // time = time * 2.0f - 1.954545f;
            return math.mad(math.pow(time, 2.0f), 3.78125f, 0.992188f);
            // time * time * 3.78125f + 0.992188f;
        }


        [BurstCompile]
        private unsafe static float Shake(float from, float to, float time, float amplitude, float speed, Ease ease)
        {
            float speedX = 0.0f;
            float speedY = 0.0f;
            speed = math.mul(time, speed + from);
            // time * speed + from * time;

            switch (ease)
            {
                case Ease.ShakeX:
                    speedX = speed;
                    speedY = 0.0f;
                    break;

                case Ease.ShakeY:
                    speedX = 0.0f;
                    speedY = speed;
                    break;

                case Ease.ShakeZ:
                    speedX = speed;
                    speedY = speed;
                    break;
            }

            var range = UnityEngine.Mathf.PerlinNoise(speedX, speedY);
            // map to [-1, 1]
            range = math.mad(range, 2.0f, -1.0f);
            // range * 2.0f - 1.0f

            if (time < 0.5f)
            {
                return math.mad(range, amplitude, to);
                // range * amplitude + to;
            }
            else
            {
                // speed decay
                return math.mad(math.mul(range, amplitude), math.mul(2.0f, 1.0f - time), to);
                // range * amplitude * (2.0f * (1.0f - time)) + to;
            }
        }


        [BurstCompile]
        private static float BezierQuadratic(float from, float to, float time, float pos)
        {
            return math.mad(math.pow(1.0f - time, 2.0f), from - pos, pos) + math.mul(math.pow(time, 2.0f), to - pos);
            // pos + math.pow(1.0f - time, 2.0f) * (from - pos) + math.pow(time, 2.0f) * (to - pos);
        }


        [BurstCompile]
        private static float BezierCubic(float from, float to, float time, float pos1, float pos2)
        {
            var leftTime = 1.0f - time;

            var fromTime = math.mul(math.mul(math.pow(leftTime, 2.0f), leftTime), from);
            var toTime = math.mul(math.mul(math.pow(time, 2.0f), time), to);

            var pos1Time = math.mul(leftTime, pos1);
            var pos2Time = math.mul(time, pos2);
            var posTime = math.mul(math.mul(leftTime, time), math.mul(3.0f, pos1Time + pos2Time));

            return fromTime + posTime + toTime;
        }
    }
}