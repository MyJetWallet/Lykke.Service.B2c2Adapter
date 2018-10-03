﻿using Autofac;
using Common;
using JetBrains.Annotations;
using Lykke.B2c2Client;
using Lykke.B2c2Client.Settings;
using Lykke.Service.B2c2Adapter.RabbitPublishers;
using Lykke.Service.B2c2Adapter.Services;
using Lykke.Service.B2c2Adapter.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.B2c2Adapter.Modules
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;
        private B2c2AdapterSettings _settings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            _settings = _appSettings.CurrentValue.B2c2AdapterService;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // B2C2 Client Lybrary
            builder.RegisterB2С2RestClient(new B2C2ClientSettings(_settings.RestUrl, _settings.AuthorizationToken));
            builder.RegisterB2С2WebSocketClient(new B2C2ClientSettings(_settings.WebSocketUrl, _settings.AuthorizationToken),
                _settings.ForceReconnectionInterval);

            // Publishers
            builder.RegisterType<OrderBookPublisher>()
                .As<IOrderBookPublisher>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.RabbitMq.OrderBooks));
            builder.RegisterType<TickPricePublisher>()
                .As<ITickPricePublisher>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.RabbitMq.TickPrices));

            // Order books service
            builder.RegisterType<OrderBooksService>()
                .AsSelf()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.InstrumentLevels))
                .WithParameter(TypedParameter.From(_settings.PublishFromCacheInterval));
        }
    }
}
