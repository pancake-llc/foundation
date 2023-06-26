using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public abstract class BasePulsableSensor : MonoBehaviour
    {
        // Should cause the sensor to perform it's 'sensing' routine, so that its list of detected objects
        // is up to date at the time of calling. Each sensor can be configured to pulse automatically at
        // fixed intervals or each timestep, however, if you need more control over when this occurs then
        // you can call this method manually.
        public abstract void Pulse();

        // If this sensor has input sensors, then the inputs are pulsed first and then this one is pulsed.
        public abstract void PulseAll();

        public abstract event System.Action OnPulsed;

        public abstract void Clear();

        public bool ShowDetectionGizmos { get; set; }
    }
}