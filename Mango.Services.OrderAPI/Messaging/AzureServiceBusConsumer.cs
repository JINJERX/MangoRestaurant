using System.Text;
using System.Text.Json.Serialization;
using AutoMapper;
using Azure.Messaging.ServiceBus;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Repository;
using Newtonsoft.Json;

namespace Mango.Services.OrderAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly string _serviceBusConnectionString;
    private readonly string _subscriptionCheckout;
    private readonly string _checkoutMessageTopic;

    private ServiceBusProcessor _checkoutProcessor;
    
    private readonly OrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AzureServiceBusConsumer(OrderRepository orderRepository, IMapper mapper, IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _configuration = configuration;

        _serviceBusConnectionString = configuration.GetValue<string>("ServiceBusConnectionString");
        _subscriptionCheckout = configuration.GetValue<string>("SubscriptionCheckout");
        _checkoutMessageTopic = configuration.GetValue<string>("CheckoutMessageTopic");

        var client = new ServiceBusClient(_serviceBusConnectionString);
        _checkoutProcessor = client.CreateProcessor(_checkoutMessageTopic, _subscriptionCheckout);
    }

    public async Task Start()
    {
        _checkoutProcessor.ProcessMessageAsync += OnCheckOutMessageReceived;
        _checkoutProcessor.ProcessErrorAsync += ErrorHandler;
        await _checkoutProcessor.StartProcessingAsync();
    }
    
    public async Task Stop()
    {
        _checkoutProcessor.ProcessMessageAsync -= OnCheckOutMessageReceived;
        _checkoutProcessor.ProcessErrorAsync -= ErrorHandler;
        await _checkoutProcessor.StopProcessingAsync();
        await _checkoutProcessor.DisposeAsync();
    }

    private Task ErrorHandler(ProcessErrorEventArgs arg)
    {
        Console.WriteLine(arg.Exception.ToString());
        return Task.CompletedTask;
    }

    private async Task OnCheckOutMessageReceived(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);
        CheckoutHeaderDto checkoutHeaderDto = JsonConvert.DeserializeObject<CheckoutHeaderDto>(body);
        OrderHeader orderHeader = new()
        {
            UserId = checkoutHeaderDto.UserId,
            FirstName = checkoutHeaderDto.FirstName,
            LastName = checkoutHeaderDto.LastName,
            OrderDetails = new List<OrderDetails>(),
            CardNumber = checkoutHeaderDto.CardNumber,
            CouponCode = checkoutHeaderDto.CouponCode,
            CVV = checkoutHeaderDto.CVV,
            DiscountTotal = checkoutHeaderDto.DiscountTotal,
            Email = checkoutHeaderDto.Email,
            ExpiryMonthYear = checkoutHeaderDto.ExpiryMonthYear,
            OrderTime = DateTime.Now,
            OrderTotal = checkoutHeaderDto.OrderTotal,
            PaymentStatus = false,
            Phone = checkoutHeaderDto.Phone,
            PickupDateTime = checkoutHeaderDto.PickupDateTime
        };
        foreach(var detailList in checkoutHeaderDto.CartDetails)
        {
            OrderDetails orderDetails = new()
            {
                ProductId = detailList.ProductId,
                ProductName = detailList.Product.Name,
                Price = detailList.Product.Price,
                Count = detailList.Count
            };
            orderHeader.CartTotalItems += detailList.Count;
            orderHeader.OrderDetails.Add(orderDetails);
        }
        await _orderRepository.AddOrderAsync(orderHeader);
    }
}