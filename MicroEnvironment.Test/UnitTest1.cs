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

            ThreadPool.SetMinThreads(1500, 500);

            customerService = new CustomerService();
            customerService.ListenToRabbitMQ();
            customerService.ListenToKafka();

            customerService2 = new CustomerService();
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
            var customerServiceClient2 = new CustomerServiceRabbitMQClient();

            await customerServiceClient.StartAsync();
            await customerServiceClient2.StartAsync();

            var sw = new Stopwatch();

            sw.Start();

            for (int i = 0; i < 2; i++)
            {
                var result = await customerServiceClient.CustomerDelete(message); //await CustomerFlow(customerServiceClient);
                var result2 = await customerServiceClient2.CustomerDelete(message); //await CustomerFlow(customerServiceClient);

                Assert.Equal(message, result);
            }

            sw.Stop();

            output.WriteLine("elapsed {0}", sw.Elapsed);
        }

        [Fact]
        public async Task Send_Async_Message_RabbitMq_Serial2()
        {
            var productServiceClient = new ProductServiceRabbitMQClient();
            //var customerServiceClient2 = new ProductServiceRabbitMQClient();

            await productServiceClient.StartAsync();
            //await customerServiceClient2.StartAsync();

            var sw = new Stopwatch();

            sw.Start();

            for (int i = 0; i < 10; i++)
            {
                var result = await productServiceClient.GetProductById(1); //await CustomerFlow(customerServiceClient);
                //var result2 = await customerServiceClient2.CustomerDelete(message); //await CustomerFlow(customerServiceClient);

                //Assert.Equal(message, result);
            }

            sw.Stop();

            output.WriteLine("elapsed {0}", sw.Elapsed);
        }

        [Fact]
        public async Task Send_Async_Message_Kafka_Serial()
        {
            var customerServiceClient = new CustomerServiceKafkaClient();
            var sw = new Stopwatch();

            sw.Start();

            for (int i = 0; i < 200; i++)
            {
                var result = await CustomerFlow(null);//customerServiceClient);

                Assert.Equal(message, result);
            }

            sw.Stop();

            output.WriteLine("elapsed {0}", sw.Elapsed);
        }

        [Fact]
        public async Task Send_Async_Message_RabbitMq_Parallel()
        {
            var customerServiceClient = new ProductServiceRabbitMQClient();

            await customerServiceClient.StartAsync();

            var sw = new Stopwatch();
            var guid = Guid.NewGuid().ToString();

            sw.Start();

            var taskList = new List<Task<Product>>();

            for (int i = 0; i < 1000; i++)
            {
                //taskList.Add(CustomerFlow(customerServiceClient));
                taskList.Add(customerServiceClient.GetProductById(1));//CustomerFlow(customerServiceClient));
                //taskList.Add(customerServiceClient.CustomerDelete(message));//CustomerFlow(customerServiceClient));
            }

            await Task.WhenAll(taskList);

            sw.Stop();

            output.WriteLine("counter1 {0}", customerService.Counter);
            output.WriteLine("counter2 {0}", customerService2.Counter);
            output.WriteLine("elapsed {0}", sw.Elapsed);
        }

        [Fact]
        public async Task Send_Async_Message_RabbitMq_Parallel2()
        {
            var customerServiceClient = new CustomerServiceRabbitMQClient();

            await customerServiceClient.StartAsync();

            var sw = new Stopwatch();
            var guid = Guid.NewGuid().ToString();

            sw.Start();

            var taskList = new List<Task<string>>();

            for (int i = 0; i < 1000; i++)
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
            var sw = new Stopwatch();

            sw.Start();

            var taskList = new List<Task<string>>();

            for (int i = 0; i < 2; i++)
            {
                taskList.Add(CustomerFlow(null));// customerServiceClient));
            }

            await Task.WhenAll(taskList);

            sw.Stop();

            output.WriteLine("elapsed {0}", sw.Elapsed);
        }

        public async Task<string> CustomerFlow(ICustomerService customerService)
        {
            //1. Thread
            var result = customerService.CustomerCreate(message);

            //1. Thread
            var result2 = customerService.CustomerCreate(result);

            //3-2-1
            return result2;
        }
    }
}
