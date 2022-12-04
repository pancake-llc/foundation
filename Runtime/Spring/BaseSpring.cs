/* Code obtained originally from: https://github.com/thammin/unity-spring.
 * Modified by Chris from LlamAcademy
MIT License

Copyright (c) 2019 Paul Young 
Copyright (c) 2022 yenmoc

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace Pancake
{
    public abstract class BaseSpring<T>
    {
        // Default to critically damped
        public virtual float Damping { get; set; } = 26f;
        public virtual float Mass { get; set; } = 1f;
        public virtual float Stiffness { get; set; } = 169f;
        public virtual T StartValue { get; set; }
        public virtual T EndValue { get; set; }
        public virtual T InitialVelocity { get; set; }
        public virtual T CurrentValue { get; set; }
        public virtual T CurrentVelocity { get; set; }

        /// <summary>
        /// Reset all values to initial states.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Update the end value in the middle of motion.
        /// This reuse the current velocity and interpolate the value smoothly afterwards.
        /// </summary>
        /// <param name="value">End value</param>
        public virtual void UpdateEndValue(T value) => UpdateEndValue(value, CurrentVelocity);

        /// <summary>
        /// Update the end value in the middle of motion but using a new velocity.
        /// </summary>
        /// <param name="value">End value</param>
        /// <param name="velocity">New velocity</param>
        public abstract void UpdateEndValue(T value, T velocity);

        /// <summary>
        /// Advance a step by deltaTime(seconds).
        /// </summary>
        /// <param name="deltaTime">Delta time since previous frame</param>
        /// <returns>Evaluated value</returns>
        public abstract T Evaluate(float deltaTime);
    }
}