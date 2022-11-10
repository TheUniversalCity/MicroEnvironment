using MicroEnvironment.HubConnectors;
using ProductService.Contracts;
using ProductService.Contracts.Models;
using System.Threading.Tasks;

namespace MicroEnvironment.Test
{
    class ProductServiceRabbitMQClient : IProductService
    {
        static readonly string QUEUE_NAME_OF_CREATE_PRODUCT = nameof(IProductService).Substring(1) + "_" + nameof(IProductService.CreateProduct);
        static readonly string QUEUE_NAME_OF_GET_PRODUCT_BY_ID = nameof(IProductService).Substring(1) + "_" + nameof(IProductService.GetProductById);

        static RabbitMqConfig config = new RabbitMqConfig
        {
            Host = "20.108.110.152"
        };

        private MessageSender<Product, bool> CreateProductMessageSender { get; set; } = new MessageSender<Product, bool>(
            QUEUE_NAME_OF_CREATE_PRODUCT,
            new RabbitMqMessageHubConnector<Product>(config),
            new RabbitMqMessageHubConnector<bool>(config));

        private MessageSender<int, Product> GetProductByIdMessageSender { get; set; } = new MessageSender<int, Product>(
            QUEUE_NAME_OF_GET_PRODUCT_BY_ID,
            new RabbitMqMessageHubConnector<int>(config),
            new RabbitMqMessageHubConnector<Product>(config));
        
        public async Task StartAsync()
        {
            await CreateProductMessageSender.StartAsync();
            await GetProductByIdMessageSender.StartAsync();
        }

        public Task<Product> GetProductById(int productId)
        {
            return GetProductByIdMessageSender.Send(productId);
        }

        public Task<bool> CreateProduct(Product product)
        {
            return CreateProductMessageSender.Send(product);
        }
    }
}
