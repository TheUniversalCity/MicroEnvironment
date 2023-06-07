using MicroEnvironment.Attributes;
using MicroEnvironment.HubConnectors.RabbitMq;
using MicroEnvironment.Messages;
using RabbitMQ.Client;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace MicroEnvironment
{
    public class ServiceListener<T>
    {
        private readonly RabbitMqConfig _config;
        private readonly T _serviceInstance;

        public ServiceListener(RabbitMqConfig config, T serviceInstance)
        {
            _serviceInstance = serviceInstance;
            _config = config;
        }

        public async Task ListenAsync(Action<string, Exception, MicroEnvironmentMessage> OnExceptionEventHandler = null)
        {
            Type serviceType = _serviceInstance.GetType();
            MethodInfo[] methods = serviceType
                            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                            .Where(m => m.GetCustomAttributes(typeof(MicroServiceMethodAttribute), false).Length > 0)
                            .ToArray();

            foreach (MethodInfo method in methods)
            {
                var parameters = method.GetParameters().Select(p => p.ParameterType);
                if (parameters.Count() != 1)
                {
                    throw new Exception($"MicroService methods should only take one parameter: {method.Name}");
                }

                Type inputType = parameters.FirstOrDefault();
                Type returnType = method.ReturnType;

                string queueName = serviceType.Name + "_" + method.Name;
                bool isAwaitable = returnType.GetMethod(nameof(Task.GetAwaiter)) != null;

                Type outputType = null;
                if (isAwaitable)
                {
                    if (returnType != typeof(Task))
                    {
                        if (returnType.GenericTypeArguments.Count() > 1)
                        {
                            throw new Exception($"Return type can have at most one generic type argument: {method.Name}");
                        }
                        outputType = returnType.GenericTypeArguments[0];
                    }
                }
                else if (returnType != typeof(void))
                {
                    outputType = returnType;
                }

                Type genericMessageListenerType;
                object messageListenerInstance;

                //RequestConnecter instance
                Type requestConnectorType = typeof(RabbitMqMessageHubConnector<>);
                Type genericRequestConnectorType = requestConnectorType.MakeGenericType(new Type[] { inputType });
                object requestConnectorInstance = Activator.CreateInstance(genericRequestConnectorType, _config);

                if (outputType != null)
                {
                    //ResponseConnecter instance
                    Type responseConnectorType = typeof(RabbitMqMessageHubConnector<>);
                    Type genericResponseConnectorType = responseConnectorType.MakeGenericType(new Type[] { outputType });
                    object responseConnectorInstance = Activator.CreateInstance(genericResponseConnectorType, _config);

                    //MessageListener instance with return type
                    Type messageListenerType = typeof(MessageListener<,>);
                    genericMessageListenerType = messageListenerType.MakeGenericType(new Type[] { inputType, outputType });
                    messageListenerInstance = Activator.CreateInstance(genericMessageListenerType, queueName, requestConnectorInstance, responseConnectorInstance);
                }
                else
                {
                    //MessageListener instance without return type
                    Type messageListenerType = typeof(MessageListener<>);
                    genericMessageListenerType = messageListenerType.MakeGenericType(new Type[] { inputType });
                    messageListenerInstance = Activator.CreateInstance(genericMessageListenerType, queueName, requestConnectorInstance);
                }

                if(OnExceptionEventHandler != null)
                {
                    ((MessageListener)messageListenerInstance).OnExceptionHandlingEvent += OnExceptionEventHandler;
                }
                
                Delegate methodDelegate = method.CreateDelegate(
                        Expression.GetDelegateType(new Type[] { inputType, returnType }), _serviceInstance);

                MethodInfo registerMethod;
                var listenerMethods = genericMessageListenerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                var registerMethods = listenerMethods.Where(m => m.Name == "Register" && m.ReturnType == typeof(void));
                var startAsyncMethod = listenerMethods.FirstOrDefault(m => m.Name == "StartAsync" && m.ReturnType == typeof(Task));

                if (isAwaitable)
                {
                    registerMethod = registerMethods
                        .Where(m => m.GetCustomAttributes(typeof(ForAwaitableAttribute), false).Length > 0)
                        .FirstOrDefault();
                }
                else
                {
                    registerMethod = registerMethods
                        .Where(m => m.GetCustomAttributes(typeof(ForAwaitableAttribute), false).Length == 0)
                        .FirstOrDefault();
                }

                await (Task)startAsyncMethod.Invoke(messageListenerInstance, null);
                registerMethod.Invoke(messageListenerInstance, new[] { methodDelegate });
            }
        }

        public async Task ListenOnSingleConnectionAsync(Action<string, Exception, MicroEnvironmentMessage> OnExceptionEventHandler = null)
        {
            var factory = new ConnectionFactory
            {
                UserName = _config.Username,
                Password = _config.Password,
                VirtualHost = "/",
                HostName = _config.Host,
                Port = _config.Port,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                RequestedChannelMax = 0
            };

            var _connection = factory.CreateConnection();

            Type serviceType = _serviceInstance.GetType();
            MethodInfo[] methods = serviceType
                            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                            .Where(m => m.GetCustomAttributes(typeof(MicroServiceMethodAttribute), false).Length > 0)
                            .ToArray();

            foreach (MethodInfo method in methods)
            {
                var parameters = method.GetParameters().Select(p => p.ParameterType);
                if (parameters.Count() != 1)
                {
                    throw new Exception($"MicroService methods should only take one parameter: {method.Name}");
                }

                Type inputType = parameters.FirstOrDefault();
                Type returnType = method.ReturnType;

                string queueName = serviceType.Name + "_" + method.Name;
                bool isAwaitable = returnType.GetMethod(nameof(Task.GetAwaiter)) != null;

                Type outputType = null;
                if (isAwaitable)
                {
                    if (returnType != typeof(Task))
                    {
                        if (returnType.GenericTypeArguments.Count() > 1)
                        {
                            throw new Exception($"Return type can have at most one generic type argument: {method.Name}");
                        }
                        outputType = returnType.GenericTypeArguments[0];
                    }
                }
                else if (returnType != typeof(void))
                {
                    outputType = returnType;
                }

                Type genericMessageListenerType;
                object messageListenerInstance;

                //RequestConnecter instance
                Type requestConnectorType = typeof(RabbitMqMessageHubConnector<>);
                Type genericRequestConnectorType = requestConnectorType.MakeGenericType(new Type[] { inputType });
                object requestConnectorInstance = Activator.CreateInstance(genericRequestConnectorType, _connection);

                if (outputType != null)
                {
                    //ResponseConnecter instance
                    Type responseConnectorType = typeof(RabbitMqMessageHubConnector<>);
                    Type genericResponseConnectorType = responseConnectorType.MakeGenericType(new Type[] { outputType });
                    object responseConnectorInstance = Activator.CreateInstance(genericResponseConnectorType, _connection);

                    //MessageListener instance with return type
                    Type messageListenerType = typeof(MessageListener<,>);
                    genericMessageListenerType = messageListenerType.MakeGenericType(new Type[] { inputType, outputType });
                    messageListenerInstance = Activator.CreateInstance(genericMessageListenerType, queueName, requestConnectorInstance, responseConnectorInstance);
                }
                else
                {
                    //MessageListener instance without return type
                    Type messageListenerType = typeof(MessageListener<>);
                    genericMessageListenerType = messageListenerType.MakeGenericType(new Type[] { inputType });
                    messageListenerInstance = Activator.CreateInstance(genericMessageListenerType, queueName, requestConnectorInstance);
                }

                if (OnExceptionEventHandler != null)
                {
                    ((MessageListener)messageListenerInstance).OnExceptionHandlingEvent += OnExceptionEventHandler;
                }

                Delegate methodDelegate = method.CreateDelegate(
                        Expression.GetDelegateType(new Type[] { inputType, returnType }), _serviceInstance);

                MethodInfo registerMethod;
                var listenerMethods = genericMessageListenerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                var registerMethods = listenerMethods.Where(m => m.Name == "Register" && m.ReturnType == typeof(void));
                var startAsyncMethod = listenerMethods.FirstOrDefault(m => m.Name == "StartAsync" && m.ReturnType == typeof(Task));

                if (isAwaitable)
                {
                    registerMethod = registerMethods
                        .Where(m => m.GetCustomAttributes(typeof(ForAwaitableAttribute), false).Length > 0)
                        .FirstOrDefault();
                }
                else
                {
                    registerMethod = registerMethods
                        .Where(m => m.GetCustomAttributes(typeof(ForAwaitableAttribute), false).Length == 0)
                        .FirstOrDefault();
                }

                await (Task)startAsyncMethod.Invoke(messageListenerInstance, null);
                registerMethod.Invoke(messageListenerInstance, new[] { methodDelegate });
            }
        }
    }
}
