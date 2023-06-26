using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public interface ISteeringSensor
    {
        GameObject gameObject { get; }
        SteerSeek Seek { get; }
        SteerAvoid Avoid { get; }
        LocomotionSystem Locomotion { get; }
        bool IsDestinationReached { get; }
        Vector3 GetSteeringVector();
    }
}