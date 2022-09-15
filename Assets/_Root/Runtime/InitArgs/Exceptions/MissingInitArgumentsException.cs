using System;

namespace Pancake.Init
{
    /// <summary>
    /// The exception that is thrown when attempting to create an Instance of an object that implements an
    /// <see cref="IArgs{}"/> interface without having provided the arguments that it requires.
    /// </summary>
    public sealed class MissingInitArgumentsException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new Instance of the <see cref="MissingInitArgumentsException"/> class.
        /// </summary>
        public MissingInitArgumentsException() : base(GenerateMessage(null)) { }

        /// <summary>
        /// Initializes a new Instance of the <see cref="MissingInitArgumentsException"/> class.
        /// </summary>
        /// <param name="clientType"> Type of the client that was initialized without the necessary arguments. </param>
        public MissingInitArgumentsException(Type clientType) : base(GenerateMessage(clientType)) { }

        /// <summary>
        /// Initializes a new Instance of the <see cref="MissingInitArgumentsException"/> class.
        /// </summary>
        /// <param name="client"> The client object that was initialized without the necessary arguments. </param>
        public MissingInitArgumentsException(object client) : base(GenerateMessage(client?.GetType())) { }

        /// <summary>
        /// Initializes a new Instance of the <see cref="MissingInitArgumentsException"/> class.
        /// </summary>
        /// <param name="message"> The error message that explains the reason for the exception. </param>
        /// <param name="inner">
        /// The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.
        /// </param>
        public MissingInitArgumentsException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new Instance of the <see cref="MissingInitArgumentsException"/> class.
        /// </summary>
        /// <param name="message"> The error message that explains the reason for the exception. </param>
        public MissingInitArgumentsException(string message) : base(message) { }

        private static string GenerateMessage(Type clientType)
        {
            if(clientType != null)
            {
                foreach(var interfaceType in clientType.GetInterfaces())
                {
                    if(!interfaceType.IsGenericType)
                    {
                        continue;
                    }

                    var genericTypeDefinition = interfaceType.IsGenericTypeDefinition ? interfaceType : interfaceType.GetGenericTypeDefinition();

                    if(genericTypeDefinition == typeof(IArgs<>) || genericTypeDefinition == typeof(IInitializable<>))
                    {
                        var arguments = interfaceType.GetGenericArguments();
                        return $"Attempted to initialize an Instance of {clientType.Name} without providing the arguments that it requires. Use GameObject.AddComponent<{clientType.Name}, {arguments[0].Name}> or {clientType.Name}.Instantiate<{arguments[0].Name}> to create an Instance with the necessary dependencies.";
                    }
                    
                    if(genericTypeDefinition == typeof(IArgs<,>) || genericTypeDefinition == typeof(IInitializable<,>))
                    {
                        var arguments = interfaceType.GetGenericArguments();
                        return $"Attempted to initialize an Instance of {clientType.Name} without providing the arguments that it requires. Use GameObject.AddComponent<{clientType.Name}, {arguments[0].Name}, {arguments[1].Name}> or {clientType.Name}.Instantiate<{arguments[0].Name}, {arguments[1].Name}> to create an Instance with the necessary dependencies.";
                    }
                    
                    if(genericTypeDefinition == typeof(IArgs<,,>) || genericTypeDefinition == typeof(IInitializable<,,>))
                    {
                        var arguments = interfaceType.GetGenericArguments();
                        return $"Attempted to initialize an Instance of {clientType.Name} without providing the arguments that it requires. Use GameObject.AddComponent<{clientType.Name}, {arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}> or {clientType.Name}.Instantiate<{arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}> to create an Instance with the necessary dependencies.";
                    }
                    
                    if(genericTypeDefinition == typeof(IArgs<,,,>) || genericTypeDefinition == typeof(IInitializable<,,,>))
                    {
                        var arguments = interfaceType.GetGenericArguments();
                        return $"Attempted to initialize an Instance of {clientType.Name} without providing the arguments that it requires. Use GameObject.AddComponent<{clientType.Name}, {arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}, {arguments[3].Name}> or {clientType.Name}.Instantiate<{arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}, {arguments[3].Name}> to create an Instance with the necessary dependencies.";
                    }
                    
                    if(genericTypeDefinition == typeof(IArgs<,,,,>) || genericTypeDefinition == typeof(IInitializable<,,,,>))
                    {
                        var arguments = interfaceType.GetGenericArguments();
                        return $"Attempted to initialize an Instance of {clientType.Name} without providing the arguments that it requires. Use GameObject.AddComponent<{clientType.Name}, {arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}, {arguments[3].Name}, {arguments[4].Name}> or {clientType.Name}.Instantiate<{arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}, {arguments[3].Name}, {arguments[4].Name}> to create an Instance with the necessary dependencies.";
                    }
                }
            }

            return $"Attempted to initialize an Instance without providing the arguments that it requires. Use GameObject.AddComponent<T, TFirstArgument...TLastArgument> or prefab.Instantiate<TArgument, TFirstArgument...TLastArgument> to create an Instance with the necessary dependencies.";
        }
    }
}