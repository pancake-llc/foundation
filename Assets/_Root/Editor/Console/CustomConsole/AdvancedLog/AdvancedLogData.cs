using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Needle.Console
{
	public class AdvancedLogData
	{
		public readonly int MaxSize;
		public readonly List<ILogData> Data;
		public int MaxId { get; private set; }
		
		public bool TryGetData<T>(List<ILogData<T>> data, int id)
		{
			if (Data == null) return false;
			data.Clear();
			for (var i = 0; i < Data.Count; i++)
			{
				var entry = Data[i];
				if (entry.Id == id)
				{
					if (entry is LogData<T> vl)
					{
						data.Add(vl);
					}
				}
			}
			return data.Count > 0;
		}

		public AdvancedLogData() : this(300) {}
		public AdvancedLogData(int maxSize)
		{
			this.MaxSize = maxSize;
			this.Data = new List<ILogData>(maxSize);
		}

		public void AddData<T>(T entry, int id)
		{
			if (Data.Count + 1 > MaxSize)
			{
				Data.RemoveAt(0);
			}

			MaxId = Mathf.Max(MaxId, id);
			
			Data.Add(new LogData<T>
			{
				Id = id,
				Value = entry,
				Frame = -1, // todo: we can access time or frame here because it happens not immediately
				Time = -1,
			});
		}
	}

	public interface ILogData
	{
		int Id { get; set; }
		float Time { get; set; }
		int Frame { get; set; }
	}

	public interface ILogData<T> : ILogData
	{
		T Value { get; set; }
	}

	public struct LogData<T> : ILogData<T>
	{
		public int Id { get; set; }
		public float Time { get; set; }
		public int Frame { get; set; }
		public T Value { get; set; }
	}
}