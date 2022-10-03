using System;

namespace Pancake.Init
{
    /// <summary>
    /// The exception that is thrown when arguments have been provided for an object being initialized but it fails to receive them.
    /// </summary>
    public sealed class InitArgumentsNotReceivedException : NotImplementedException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InitArgumentsNotReceivedException"/> class.
        /// </summary>
        public InitArgumentsNotReceivedException() : base(GenerateMessage(null, null)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InitArgumentsNotReceivedException"/> class.
        /// </summary>
        /// <param name="methodName"> Name of the method from which the exception originates. </param>
        public InitArgumentsNotReceivedException(string methodName) : base(GenerateMessage(methodName, null)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InitArgumentsNotReceivedException"/> class.
        /// </summary>
        /// <param name="methodName"> Name of the method from which the exception originates. </param>
        /// <param name="clientType"> Type of the client that failed to receive the arguments. </param>
        public InitArgumentsNotReceivedException(string methodName, Type clientType) : base(GenerateMessage(methodName, clientType)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InitArgumentsNotReceivedException"/> class.
        /// </summary>
        /// <param name="clientType"> Type of the client that failed to receive the arguments. </param>
        public InitArgumentsNotReceivedException(Type clientType) : base(GenerateMessage(null, clientType)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InitArgumentsNotReceivedException"/> class.
        /// </summary>
        /// <param name="client"> The client object that failed to receive the arguments. </param>
        public InitArgumentsNotReceivedException(object client) : base(GenerateMessage(null, client?.GetType())) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InitArgumentsNotReceivedException"/> class.
        /// </summary>
        /// <param name="methodName"> Name of the method from which the exception originates. </param>
        /// <param name="inner">
        /// The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.
        /// </param>
        public InitArgumentsNotReceivedException(string methodName, Exception inner) : base(GenerateMessage(methodName, null), inner) { }

        private static string GenerateMessage(string methodName, Type clientType)
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
                        if(!string.IsNullOrEmpty(methodName))
                        {
                            return $"{methodName}<{clientType.Name}, {arguments[0].Name}>() called but {clientType.Name} does not implement IInitializable<{arguments[0].Name}> and did not receive the provided argument during initialization.";
                        }

                        return $"{clientType.Name} does not implement IInitializable<{arguments[0].Name}> and did not receive the provided argument during initialization.";
                    }
                    
                    if(genericTypeDefinition == typeof(IArgs<,>) || genericTypeDefinition == typeof(IInitializable<,>))
                    {
                        var arguments = interfaceType.GetGenericArguments();
                        if(!string.IsNullOrEmpty(methodName))
                        {
                            return $"{methodName}<{clientType.Name}, {arguments[0].Name}, {arguments[1].Name}>() called but {clientType.Name} does not implement IInitializable<{arguments[0].Name}, {arguments[1].Name}> and did not receive the provided arguments during initialization.";
                        }

                        return $"{clientType.Name} does not implement IInitializable<{arguments[0].Name}, {arguments[1].Name}> and did not receive the provided arguments during initialization.";
                    }
                    
                    if(genericTypeDefinition == typeof(IArgs<,,>) || genericTypeDefinition == typeof(IInitializable<,,>))
                    {
                        var arguments = interfaceType.GetGenericArguments();
                        if(!string.IsNullOrEmpty(methodName))
                        {
                            return $"{methodName}<{clientType.Name}, {arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}>() called but {clientType.Name} does not implement IInitializable<{arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}> and did not receive the provided arguments during initialization.";
                        }

                        return $"{clientType.Name} does not implement IInitializable<{arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}> and did not receive the provided arguments during initialization.";
                    }
                    
                    if(genericTypeDefinition == typeof(IArgs<,,,>) || genericTypeDefinition == typeof(IInitializable<,,,>))
                    {
                        var arguments = interfaceType.GetGenericArguments();
                        if(!string.IsNullOrEmpty(methodName))
                        {
                            return $"{methodName}<{clientType.Name}, {arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}, {arguments[3].Name}>() called but {clientType.Name} does not implement IInitializable<{arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}, {arguments[3].Name}> and did not receive the provided arguments during initialization.";
                        }

                        return $"{clientType.Name} does not implement IInitializable<{arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}, {arguments[3].Name}> and did not receive the provided arguments during initialization.";
                    }
                    
                    if(genericTypeDefinition == typeof(IArgs<,,,,>) || genericTypeDefinition == typeof(IInitializable<,,,,>))
                    {
                        var arguments = interfaceType.GetGenericArguments();
                        if(!string.IsNullOrEmpty(methodName))
                        {
                            return $"{methodName}<{clientType.Name}, {arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}, {arguments[3].Name}, {arguments[4].Name}>() called but {clientType.Name} does not implement IInitializable<{arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}, {arguments[3].Name}, {arguments[4].Name}> and did not receive the provided arguments during initialization.";
                        }

                        return $"{clientType.Name} does not implement IInitializable<{arguments[0].Name}, {arguments[1].Name}, {arguments[2].Name}, {arguments[3].Name}, {arguments[4].Name}> and did not receive the provided arguments during initialization.";
                    }
                }
            }
            
            if(!string.IsNullOrEmpty(methodName))
            {
                return $"{methodName}() called but client does not implement IInitializable<> and did not receive the provided arguments during initialization.";
            }

            return $"Client does not implement IInitializable and did not receive the provided arguments during initialization.";
        }
    }
}