using Microsoft.Extensions.DependencyInjection;
using ProductService.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MicroEnvironment.Test
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        CustomerService customerService;
        CustomerService customerService2;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;

            ThreadPool.SetMinThreads(200, 200);

            customerService = new CustomerService(this.output);

            customerService.ListenToRabbitMQ();
            customerService.ListenToKafka();

            customerService2 = new CustomerService(this.output);

            customerService2.ListenToRabbitMQ();
            customerService2.ListenToKafka();

            //var customerService2 = new CustomerService();
            //customerService2.ListenToRabbitMQ();
            //customerService2.ListenToKafka();
        }

        private IServiceProvider Initialize(Action<IServiceCollection> action = null)
        {
            var serviceCollection = new ServiceCollection();

            action(serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }

        string message = string.Join("", Enumerable.Range(0, 10000).Select(c => 'k'));

        [Fact] 
        public async Task Send_Async_Message_RabbitMq_Serial()
        {
            var customerServiceClient = new CustomerServiceRabbitMQClient();

            await customerServiceClient.StartAsync();
            
            var sw = new Stopwatch();

            sw.Start();

            for (int i = 0; i < 10; i++)
            {
                var guid = Guid.NewGuid().ToString();

                var result = await customerServiceClient.CustomerCreate(guid); //await CustomerFlow(customerServiceClient);

                Assert.Equal(guid, result);
            }

            sw.Stop();

            output.WriteLine("elapsed {0}", sw.Elapsed);
        }

        [Fact]
        public async Task Send_Async_Message_Kafka_Serial()
        {
            var customerServiceClient = new CustomerServiceKafkaClient();

            await customerServiceClient.StartAsync();

            var sw = new Stopwatch();

            sw.Start();

            for (int i = 0; i < 10; i++)
            {
                var guid = Guid.NewGuid().ToString();

                var result = await customerServiceClient.CustomerCreate(guid);

                Assert.Equal(guid, result);
            }

            sw.Stop();

            output.WriteLine("elapsed {0}", sw.Elapsed);
        }

        [Fact]
        public async Task Send_Async_Message_RabbitMq_Parallel()
        {
            var customerServiceClient = new CustomerServiceRabbitMQClient();

            await customerServiceClient.StartAsync();

            var sw = new Stopwatch();
            var guid = Guid.NewGuid().ToString();

            sw.Start();

            var taskList = new List<Task<string>>();

            for (int i = 0; i < 100000; i++)
            {
                //taskList.Add(CustomerFlow(customerServiceClient));
                taskList.Add(customerServiceClient.CustomerCreate(guid));//CustomerFlow(customerServiceClient));
                //taskList.Add(customerServiceClient.CustomerDelete(message));//CustomerFlow(customerServiceClient));
            }

            await Task.WhenAll(taskList);

            sw.Stop();

            output.WriteLine("counter1 {0}", customerService.Counter);
            output.WriteLine("counter2 {0}", customerService2.Counter);
            output.WriteLine("elapsed {0}", sw.Elapsed);
        }

        [Fact]
        public async Task Send_Async_Message_Kafka_Parallel()
        {
            var customerServiceClient = new CustomerServiceKafkaClient();
            
            await customerServiceClient.StartAsync();

            var sw = new Stopwatch();

            sw.Start();

            var taskList = new List<Task<string>>();

            for (int i = 0; i < 100000; i++)
            {
                var guid = Guid.NewGuid().ToString();

                taskList.Add(customerServiceClient.CustomerCreate(guid));// customerServiceClient));
            }

            await Task.WhenAll(taskList);

            sw.Stop();

            output.WriteLine("elapsed {0}", sw.Elapsed);
        }
    }
}
