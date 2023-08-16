using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
#pragma warning disable CS0618

namespace PrimeTween {
    public partial struct Tween {
        public TweenAwaiter GetAwaiter() {
            return new TweenAwaiter(this);
        }
        
        [Obsolete("This struct is needed for async/await support, please don't use it directly. Marked as obsolete to exclude from IDE suggestions.")]
        public readonly struct TweenAwaiter : INotifyCompletion {
            readonly Tween tween;

            internal TweenAwaiter(Tween tween) {
                this.tween = tween;
            }

            public bool IsCompleted => !tween.IsAlive;

            public void OnCompleted([NotNull] Action continuation) {
                // try-catch is needed here because any exception that is thrown inside the OnCompleted will be silenced
                // probably because this try in UnitySynchronizationContext.cs has no exception handling:
                // https://github.com/Unity-Technologies/UnityCsReference/blob/dd0d959800a675836a77dbe188c7dd55abc7c512/Runtime/Export/Scripting/UnitySynchronizationContext.cs#L157
                try {
                    Assert.IsTrue(tween.IsAlive);
                    // waitFor doesn't postpone the await until the next frame because all tweens are updated in order.
                    // waitFor serves two purposes:
                    // 1. Work around the limitation of one onComplete callback by wrapping the tween inside a new tween with waitFor dependency.
                    // 2. If tween is stopped manually, onComplete callback will not be executed.
                    waitFor(tween).OnComplete(continuation);
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }

            public void GetResult() {
            }
        }
    }

    public partial struct Sequence {
        public Tween.TweenAwaiter GetAwaiter() {
            return new Tween.TweenAwaiter(GetLongestOrDefault());
        }
    }
}