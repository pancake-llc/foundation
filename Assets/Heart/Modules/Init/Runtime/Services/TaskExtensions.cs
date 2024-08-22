using System.Reflection;
using System.Threading.Tasks;

namespace Sisus.Init.Internal
{
	internal static class TaskExtensions
	{
		internal static async Task<object> GetResult(this Task task)
		{
			object result;
			do
			{
				if(task is Task<object> objectTask)
				{
					result = await objectTask;
				}
				else
				{
					await task;
					if(task.GetType().GetProperty(nameof(Task<object>.Result)) is not PropertyInfo resultProperty)
					{
						return Task.FromResult<object>(null);
					}
					
					result = resultProperty.GetValue(task);
				}

				task = result as Task;
			}
			while(task is not null);

			return result;
		}
	}
}