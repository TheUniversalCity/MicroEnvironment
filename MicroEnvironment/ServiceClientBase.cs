using MicroEnvironment.HubConnectors.RabbitMq;
using MicroEnvironment.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MicroEnvironment
{
    public abstract class ServiceClientBase<T>
    {
        protected Dictionary<string, object> MessageSenders = new Dictionary<string, object>();

        public ServiceClientBase(RabbitMqConfig config)
        {
            Initialize(config);
        }
        private void Initialize(RabbitMqConfig config)
        {
            Type serviceType = typeof(T);
            MethodInfo[] methods = serviceType
                            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
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

                string queueName = serviceType.Name.Substring(1) + "_" + method.Name;
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
                object requestConnectorInstance = Activator.CreateInstance(genericRequestConnectorType, config);

                if (outputType != null)
                {
                    //ResponseConnecter instance
                    Type responseConnectorType = typeof(RabbitMqMessageHubConnector<>);
                    Type genericResponseConnectorType = responseConnectorType.MakeGenericType(new Type[] { outputType });
                    object responseConnectorInstance = Activator.CreateInstance(genericResponseConnectorType, config);

                    //MessageListener instance with return type
                    Type messageListenerType = typeof(MessageSender<,>);
                    genericMessageListenerType = messageListenerType.MakeGenericType(new Type[] { inputType, outputType });
                    messageListenerInstance = Activator.CreateInstance(genericMessageListenerType, queueName, requestConnectorInstance, responseConnectorInstance);
                }
                else
                {
                    //MessageListener instance without return type
                    Type messageListenerType = typeof(MessageSender<>);
                    genericMessageListenerType = messageListenerType.MakeGenericType(new Type[] { inputType });
                    messageListenerInstance = Activator.CreateInstance(genericMessageListenerType, queueName, requestConnectorInstance);
                }

                MessageSenders.Add(method.Name, messageListenerInstance);
            }
        }
        public Task StartAsync()
        {
            var taskList = new List<Task>();

            foreach (var messageSenderObj in MessageSenders)
            {
                var startAsync = messageSenderObj.Value.GetType()
                                                       .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                                       .Where(m => m.Name == "StartAsync" && m.ReturnType == typeof(Task))
                                                       .FirstOrDefault();

                if (startAsync != null)
                {
                    var task = (Task)startAsync.Invoke(messageSenderObj.Value, Array.Empty<object>());

                    taskList.Add(task);
                }
            }

            return Task.WhenAll(taskList);
        }
    }
}
