using UnityEngine;
using UnityEngine.Events;

namespace Pancake.SOA
{
    [System.Serializable]
    public class AudioClipEvent : UnityEvent<AudioClip> { }

	[CreateAssetMenu(
	    fileName = "AudioClipVariable.asset",
	    menuName = SOArchitecture_Utility.ADVANCED_VARIABLE_SUBMENU + "AudioClip",
	    order = 120)]
	public class AudioClipVariable : BaseVariable<AudioClip, AudioClipEvent>
	{
	}
}