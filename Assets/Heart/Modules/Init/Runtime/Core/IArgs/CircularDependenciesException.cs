using System;
using System.Collections.Generic;
using System.Text;
using Sisus.Init.Internal;

namespace Sisus.Init
{
	/// <summary>
	/// The exception that is thrown when creating a service object requires another service object to exist, but to creating
	/// that other service object also requires the prior service object to exist, thus making it impossible to create neither service object.
	/// </summary>
	public sealed class CircularDependenciesException : ServiceInitFailedException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CircularDependenciesException"/> class.
		/// </summary>
		/// <param name="dependencyChain"> Collection of two or services, forming the circular dependency. The first and last items in the collection should be the same service. </param>
		/// <param name="exception">
		/// The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.
		/// </param>
		internal CircularDependenciesException(ServiceInfo serviceInfo, List<ServiceInfo> dependencyChain, Exception exception = null) : base(serviceInfo, ServiceInitFailReason.CircularDependencies, GenerateMessage(serviceInfo, dependencyChain), exception) { }

		internal static string GenerateMessage(ServiceInfo serviceInfo, List<ServiceInfo> dependencyChain)
		{
			int count = dependencyChain.Count;
			var sb = new StringBuilder();
			if(count == 0)
			{
				sb.Append("Unable to initialize service because its constructor requires another service, which can not be constructed without the prior service existing first.");
				AppendServiceInfo(sb, serviceInfo);
				AppendMoreHelpUrl(sb);
				return sb.ToString();
			}

			string firstName = TypeUtility.ToString(dependencyChain[0].ConcreteOrDefiningType);

			if(count == 1)
			{
				sb.Append("Unable to initialize service ");
				sb.Append(firstName);
				sb.Append(" because it requires itself as a constructor argument.\n");
				sb.Append("... → <color=red>");
				sb.Append(firstName);
				sb.Append("</color> → <color=red>");
				sb.Append(firstName);
				sb.Append("</color> → ...");
				return sb.ToString();
			}
			
			string secondName = TypeUtility.ToString(dependencyChain[1].ConcreteOrDefiningType);
			sb.Append("Unable to initialize service ");
			sb.Append(firstName);
			sb.Append(" because its constructor requires another service, which can not be constructed without the prior service existing first.\n");

			sb.Append("... → <color=red>");
			sb.Append(firstName);
			sb.Append("</color> → ");
			sb.Append(secondName);

			for(int i = 2; i < count; i++)
			{
				sb.Append(" → ");
				sb.Append(TypeUtility.ToString(dependencyChain[i].ConcreteOrDefiningType));
			}

			sb.Append(" → <color=red>");
			sb.Append(firstName);
			sb.Append("</color> → ...");

			AppendServiceInfo(sb, serviceInfo);
			AppendMoreHelpUrl(sb);

			return sb.ToString();
		}
	}
}