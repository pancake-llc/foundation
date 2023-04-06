using UnityEngine;

namespace Pancake.Spring
{
    public class SpringVector3 : BaseSpring<Vector3>
    {
        private SpringFloat _x = new SpringFloat();
        private SpringFloat _y = new SpringFloat();
        private SpringFloat _z = new SpringFloat();

        public override float Damping
        {
            get => base.Damping;
            set
            {
                _x.Damping = value;
                _y.Damping = value;
                _z.Damping = value;
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
                _z.Stiffness = value;
                base.Stiffness = value;
            }
        }

        public override Vector3 StartValue
        {
            get => new Vector3(_x.StartValue, _y.StartValue, _z.StartValue);
            set
            {
                _x.StartValue = value.x;
                _y.StartValue = value.y;
                _z.StartValue = value.z;
            }
        }

        public override Vector3 EndValue
        {
            get => new Vector3(_x.EndValue, _y.EndValue, _z.EndValue);
            set
            {
                _x.EndValue = value.x;
                _y.EndValue = value.y;
                _z.EndValue = value.z;
            }
        }

        public override Vector3 InitialVelocity
        {
            get => new Vector3(_x.InitialVelocity, _y.InitialVelocity, _z.InitialVelocity);
            set
            {
                _x.InitialVelocity = value.x;
                _y.InitialVelocity = value.y;
                _z.InitialVelocity = value.z;
            }
        }

        public override Vector3 CurrentVelocity
        {
            get => new Vector3(_x.CurrentVelocity, _y.CurrentVelocity, _z.CurrentVelocity);
            set
            {
                _x.CurrentVelocity = value.x;
                _y.CurrentVelocity = value.y;
                _z.CurrentVelocity = value.z;
            }
        }

        public override Vector3 CurrentValue
        {
            get => new Vector3(_x.CurrentValue, _y.CurrentValue, _z.CurrentValue);
            set
            {
                _x.CurrentValue = value.x;
                _y.CurrentValue = value.y;
                _z.CurrentValue = value.z;
            }
        }

        public override void Reset()
        {
            _x.Reset();
            _y.Reset();
            _z.Reset();
        }

        public override void UpdateEndValue(Vector3 value, Vector3 velocity)
        {
            _x.UpdateEndValue(value.x, velocity.x);
            _y.UpdateEndValue(value.y, velocity.y);
            _z.UpdateEndValue(value.z, velocity.z);
        }

        public override Vector3 Evaluate(float deltaTime)
        {
            CurrentValue = new Vector3(_x.Evaluate(deltaTime), _y.Evaluate(deltaTime), _z.Evaluate(deltaTime));
            CurrentVelocity = new Vector3(_x.CurrentVelocity, _y.CurrentVelocity, _z.CurrentVelocity);
            return CurrentValue;
        }
    }
}