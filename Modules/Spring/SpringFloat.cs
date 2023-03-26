namespace Pancake.Spring
{
    /// <summary>
    /// Closed-form solution for the ODE of damped harmonic oscillator.
    /// https://en.wikipedia.org/wiki/Harmonic_oscillator#Damped_harmonic_oscillator
    ///
    /// Proof and derived from http://www.ryanjuckett.com/programming/damped-springs/
    /// </summary>
    /// 
    public class SpringFloat : BaseSpring<float>
    {
        protected float duration;

        public override void Reset()
        {
            duration = 0f;
            CurrentValue = 0f;
            CurrentVelocity = 0f;
            InitialVelocity = 0f;
        }

        public override void UpdateEndValue(float value, float velocity)
        {
            StartValue = CurrentValue;
            EndValue = value;
            InitialVelocity = velocity;
            duration = 0f;
        }

        public override float Evaluate(float deltaTime)
        {
            duration += deltaTime;

            float c = Damping;
            float m = Mass;
            float k = Stiffness;
            float v0 = -InitialVelocity;
            float t = duration;

            float zeta = c / (2 * Math.Sqrt(k * m)); // damping ratio
            float omega0 = Math.Sqrt(k / m); // undamped angular frequency of the oscillator (rad/s)
            float x0 = EndValue - StartValue;

            float omegaZeta = omega0 * zeta;
            float x;
            float v;

            if (zeta < 1) // Under damped
            {
                float omega1 = omega0 * Math.Sqrt(1.0f - zeta * zeta); // exponential decay
                float e = Math.Exp(-omegaZeta * t);
                float c1 = x0;
                float c2 = (v0 + omegaZeta * x0) / omega1;
                float cos = Math.Cos(omega1 * t);
                float sin = Math.Sin(omega1 * t);
                x = e * (c1 * cos + c2 * sin);
                v = -e * ((x0 * omegaZeta - c2 * omega1) * cos + (x0 * omega1 + c2 * omegaZeta) * sin);
            }
            else if (zeta > 1) // Over damped
            {
                float omega2 = omega0 * Math.Sqrt(zeta * zeta - 1.0f); // frequency of damped oscillation
                float z1 = -omegaZeta - omega2;
                float z2 = -omegaZeta + omega2;
                float e1 = Math.Exp(z1 * t);
                float e2 = Math.Exp(z2 * t);
                float c1 = (v0 - x0 * z2) / (-2 * omega2);
                float c2 = x0 - c1;
                x = c1 * e1 + c2 * e2;
                v = c1 * z1 * e1 + c2 * z2 * e2;
            }
            else // Critically damped
            {
                float e = Math.Exp(-omega0 * t);
                x = e * (x0 + (v0 + omega0 * x0) * t);
                v = e * (v0 * (1 - t * omega0) + t * x0 * (omega0 * omega0));
            }

            CurrentValue = EndValue - x;
            CurrentVelocity = v;

            return CurrentValue;
        }
    }
}