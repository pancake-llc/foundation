using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
#pragma warning disable CS0618

namespace PrimeTween {
    public partial struct Tween {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TweenAwaiter GetAwaiter() {
            return new TweenAwaiter(this);
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This struct is needed for async/await support, you should not use it directly.")]
        public readonly struct TweenAwaiter : INotifyCompletion {
            readonly Tween tween;
            readonly Sequence sequence;

            internal TweenAwaiter(Tween tween) {
                this.tween = tween;
                sequence = default;
            }

            internal TweenAwaiter(Sequence sequence) {
                tween = default;
                this.sequence = sequence;
            }

            public bool IsCompleted => !isAlive;

            public void OnCompleted([NotNull] Action continuation) {
                // try-catch is needed here because any exception that is thrown inside the OnCompleted will be silenced
                // probably because this try in UnitySynchronizationContext.cs has no exception handling:
                // https://github.com/Unity-Technologies/UnityCsReference/blob/dd0d959800a675836a77dbe188c7dd55abc7c512/Runtime/Export/Scripting/UnitySynchronizationContext.cs#L157
                try {
                    Assert.IsTrue(isAlive);
                    var infiniteSettings = new TweenSettings<float>(0, 0, float.MaxValue, Ease.Linear, -1);
                    var wait = animate(PrimeTweenManager.dummyTarget, ref infiniteSettings, t => {
                        if (t._isAlive && !t.coroutineEnumerator.isAlive) {
                            t.ForceComplete();
                        }
                    }, null);
                    Assert.IsTrue(wait.isAlive);
                    wait.tween.OnComplete(continuation, true);
                    Assert.IsTrue(tween.IsCreated ^ sequence.IsCreated);
                    if (tween.IsCreated) {
                        wait.tween.coroutineEnumerator.Setup(tween);
                    } else {
                        wait.tween.coroutineEnumerator.Setup(sequence);
                    }
                } catch (Exception e) {
                    Debug.LogException(e);
                    throw;
                }
            }

            bool isAlive {
                get {
                    Assert.IsFalse(tween.isAlive && sequence.isAlive);
                    return tween.isAlive || sequence.isAlive;
                }
            }
            
            public void GetResult() {
            }
        }
    }

    public partial struct Sequence {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Tween.TweenAwaiter GetAwaiter() {
            return new Tween.TweenAwaiter(this);
        }
    }
}