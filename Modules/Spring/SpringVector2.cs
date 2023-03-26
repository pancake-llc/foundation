using UnityEngine;

namespace Pancake.Spring
{
    public class SpringVector2 : BaseSpring<Vector2>
    {
        private SpringFloat _x = new SpringFloat();
        private SpringFloat _y = new SpringFloat();

        public override float Damping
        {
            get => base.Damping;
            set
            {
                _x.Damping = value;
                _y.Damping = value;
                base.Damping = value;
            }
        }

        public override float Stiffness
        {
            get => base.Stiffness;
            set
            {
                _x.Stiffness = value;
                _y.Stiffness = value;
                base.Stiffness = value;
            }
        }

        public override Vector2 InitialVelocity
        {
            get => new Vector2(_x.InitialVelocity, _y.InitialVelocity);
            set
            {
                _x.InitialVelocity = value.x;
                _y.InitialVelocity = value.y;
            }
        }

        public override Vector2 StartValue
        {
            get => new Vector2(_x.StartValue, _y.StartValue);
            set
            {
                _x.StartValue = value.x;
                _y.StartValue = value.y;
            }
        }

        public override Vector2 EndValue
        {
            get => new Vector2(_x.EndValue, _y.EndValue);
            set
            {
                _x.EndValue = value.x;
                _y.EndValue = value.y;
            }
        }

        public override Vector2 CurrentVelocity
        {
            get => new Vector2(_x.CurrentVelocity, _y.CurrentVelocity);
            set
            {
                _x.CurrentVelocity = value.x;
                _y.CurrentVelocity = value.y;
            }
        }

        public override Vector2 CurrentValue
        {
            get => new Vector2(_x.CurrentValue, _y.CurrentValue);
            set
            {
                _x.CurrentValue = value.x;
                _y.CurrentValue = value.y;
            }
        }

        public override void Reset()
        {
            _x.Reset();
            _y.Reset();
        }

        public override void UpdateEndValue(Vector2 value, Vector2 velocity)
        {
            _x.UpdateEndValue(value.x, velocity.x);
            _y.UpdateEndValue(value.y, velocity.y);
        }

        public override Vector2 Evaluate(float deltaTime)
        {
            CurrentValue = new Vector2(_x.Evaluate(deltaTime), _y.Evaluate(deltaTime));
            CurrentVelocity = new Vector2(_x.CurrentVelocity, _y.CurrentVelocity);
            return CurrentValue;
        }
    }
}