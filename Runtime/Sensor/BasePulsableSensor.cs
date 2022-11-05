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

        public abstract event System.Action OnPulsed;

        public bool ShowDetectionGizmos { get; set; }
    }
}