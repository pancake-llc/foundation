using System;
using UnityEngine;

namespace  Pancake.SelectiveProfiling
{
	[Serializable]
	internal class UpdateSettingsCommand : NetworkCommand
	{
		[SerializeField]
		private SelectiveProfilerSettings instanceFromMain;

		public UpdateSettingsCommand(SelectiveProfilerSettings instanceFromMain)
		{
			this.instanceFromMain = instanceFromMain;
		}

		public override void Execute()
		{
			SelectiveProfilerSettings.Instance = instanceFromMain;
		}
	}
}