using System;
using System.Text;
using Sisus.Init.Internal;

namespace Sisus.Init
{
    /// <summary>
    /// The exception that is thrown when an object is initialized
    /// without all the services that it depends on having been provided to it.
    /// </summary>
    public sealed class MissingInitArgumentsException : InitArgsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingInitArgumentsException"/> class.
        /// </summary>
        public MissingInitArgumentsException() : base(GenerateMessage(null)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingInitArgumentsException"/> class.
        /// </summary>
        /// <param name="clientType"> Type of the client that was initialized without the necessary arguments. </param>
        public MissingInitArgumentsException(Type clientType) : base(GenerateMessage(clientType)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingInitArgumentsException"/> class.
        /// </summary>
        /// <param name="client"> The client object that was initialized without the necessary arguments. </param>
        public MissingInitArgumentsException(object client) : base(GenerateMessage(client?.GetType())) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingInitArgumentsException"/> class.
        /// </summary>
        /// <param name="message"> The error message that explains the reason for the exception. </param>
        /// <param name="inner">
        /// The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.
        /// </param>
        public MissingInitArgumentsException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingInitArgumentsException"/> class.
        /// </summary>
        /// <param name="message"> The error message that explains the reason for the exception. </param>
        public MissingInitArgumentsException(string message) : base(message) { }

        private static string GenerateMessage(Type clientType)
        {
            var sb = new StringBuilder();
            string clientTypeName;
            Type[] argumentTypes;

            if(clientType != null)
            {
                clientTypeName = TypeUtility.ToString(clientType);

                foreach(var interfaceType in clientType.GetInterfaces())
                {
                    if(!interfaceType.IsGenericType)
                    {
                        continue;
                    }

                    var genericTypeDefinition = interfaceType.IsGenericTypeDefinition ? interfaceType : interfaceType.GetGenericTypeDefinition();
                    Type initializableType;
                    if(InitializerUtility.argumentCountsByIArgsTypeDefinition.TryGetValue(genericTypeDefinition, out int argumentCount))
					{
                        argumentTypes = interfaceType.GetGenericArguments();
                        initializableType = InitializerUtility.GetIInitializableType(argumentTypes);
                        if(!initializableType.IsAssignableFrom(clientType))
						{
                            sb.Append(clientTypeName);
                            sb.Append(" was initialized without being provided with all of the ");
                            sb.Append(argumentCount);
                            sb.Append(" initialization arguments that it requires.\n");
                            sb.Append("Make sure that all the Init arguments are registered as services using the [Service] attribute, ServiceTag or Services components.\n");
                            
                            sb.Append("You can also manually pass all the arguments to the client using ");
                            sb.Append(clientTypeName);
                            sb.Append(".Instantiate<");
                            sb.Append(TypeUtility.ToString(argumentTypes[0]));
                            for(int i = 1, count = argumentTypes.Length; i < count; i++)
							{
                                sb.Append(", ");
                                sb.Append(TypeUtility.ToString(argumentTypes[i]));
							}

                            sb.Append("> or GameObject.AddComponent<");
                            sb.Append(clientTypeName);
                            sb.Append(TypeUtility.ToString(argumentTypes[0]));
                            for(int i = 1, count = argumentTypes.Length; i < count; i++)
							{
                                sb.Append(", ");
                                sb.Append(TypeUtility.ToString(argumentTypes[i]));
							}

                            sb.Append(".");
                            return sb.ToString();
						}
					}
                    else if(InitializerUtility.argumentCountsByIInitializableTypeDefinition.TryGetValue(genericTypeDefinition, out argumentCount))
					{
                        initializableType = interfaceType;
					}
                    else
					{
                        initializableType = null;
                        argumentCount = -1;
					}

                    if(initializableType != null)
					{
                        sb.Append(clientTypeName);
                        sb.Append(" was initialized without being provided with all of the ");
                        if(argumentCount > 0)
						{
                            sb.Append(argumentCount);
                            sb.Append(" ");
						}

                        sb.Append("initialization arguments that it requires.\n");

                        sb.Append("Make sure that all the Init arguments are registered as services using the [Service] attribute, ServiceTag or Services components,");
                        sb.Append(" or attach an initializer and configure the services using the Inspector.\n");
                            
                        sb.Append("You can also manually pass all the arguments to the client using ");
                        sb.Append(clientTypeName);
                        sb.Append(".Instantiate<");
                        argumentTypes = interfaceType.GetGenericArguments();
                        sb.Append(TypeUtility.ToString(argumentTypes[0]));
                        for(int i = 1, count = argumentTypes.Length; i < count; i++)
						{
                            sb.Append(", ");
                            sb.Append(TypeUtility.ToString(argumentTypes[i]));
						}

                        sb.Append("> or GameObject.AddComponent<");
                        sb.Append(clientTypeName);
                        for(int i = 0, count = argumentTypes.Length; i < count; i++)
						{
                            sb.Append(", ");
                            sb.Append(TypeUtility.ToString(argumentTypes[i]));
						}

                        sb.Append(".");
                        return sb.ToString();
					}
                }

                if(typeof(IInitializable).IsAssignableFrom(clientType))
				{
                    sb.Append(clientTypeName);
                    sb.Append(" was initialized without it having access to all the services that it depends on.\n");
                    sb.Append("Make sure that all objects that the client depends on have been registered as services using the [Service] attribute, ServiceTag or Services components.");
                    return sb.ToString();
				}
            }
            else
			{
                clientTypeName = "Client";
			}

            sb.Append(clientTypeName);
            sb.Append(" was initialized without it having access to all the services that it depends on.");
            return sb.ToString();
        }
    }
}