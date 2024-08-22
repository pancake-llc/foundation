using System;
using System.Collections.Generic;
using UnityEngine;
using Math = Pancake.Common.Math;

namespace Pancake.Component
{
    [EditorIcon("icon_default")]
    public class HealthBar : GameUnit, IDamageable
    {
        [SerializeField] private float width = 100f;
        [SerializeField, Range(0f, 1f)] private float opacity = 1f;
        [SerializeField] private Color color = Color.green;
        [SerializeField] private Vector2 offset;

        public static readonly List<HealthBar> ActivesHealthBar = new();

        private float _hp;
        private Action _onDeadEvent;
        private Transform _transform;

        /// <summary>
        /// The second parameter indicates whether the blood volume is increased when true and decreased when false.
        /// </summary>
        public event Action<float, bool> OnHpChangeEvent;

        public Vector2 Offset => offset;
        public float Percentage => _hp / MaxHp;
        public float Width => width;
        public Color Color => color;
        public float Opacity => opacity;

        public float MaxHp { get; private set; }

        public Transform CachedTransform
        {
            get
            {
                if (_transform == null) _transform = transform;
                return _transform;
            }
        }

        public float Hp
        {
            get => _hp;
            protected set
            {
                if (_hp < value) OnHpChangeEvent?.Invoke(value, true);
                else if (_hp > value) OnHpChangeEvent?.Invoke(value, false);

                _hp = value;
            }
        }

        public bool IsAlive => _hp > 0;

        public void Initialize(float maxHp, Action<float, bool> onHpChangeEvent, Action onDeadEvent)
        {
            Hp = maxHp;
            MaxHp = maxHp;
            OnHpChangeEvent = onHpChangeEvent;
            _onDeadEvent = onDeadEvent;
        }

        public virtual void TakeDamage(float damage)
        {
            if (damage <= 0) return;

            float value = Math.Min(_hp, damage);
            Hp -= value;
            if (_hp <= 0f) Die();
        }

        public virtual void Die() { _onDeadEvent?.Invoke(); }

        public virtual void Dispose() { }

        protected override void OnEnabled() { ActivesHealthBar.Add(this); }

        protected override void OnDisabled() { ActivesHealthBar.Remove(this); }
    }
}