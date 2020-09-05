using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Cloud.WebApp;
using Repository;
using System;

namespace Cloud.Test
{
    public static class TestSetup
    {
        public static ContainerBuilder GetContainerBuilder()
        {
            //Mock<IConfiguration> mockConfiguration = new Mock<IConfiguration>();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .Build();

            IServiceCollection services = new ServiceCollection();
            var containerBuilder = BuildContainer(services);

            // TODO don't register here
            containerBuilder.RegisterInstance<ILocalContextConfiguration>(new LocalContextConfiguration(
                accountEndPoint: "https://abc.documents.azure.com:443/",
                accountKey: "...",
                defaultContainer: "Items",
                databaseName: "ToDoList"));

            var startup = new Startup(configuration);
            startup.ConfigureServices(services);
            startup.ConfigureContainer(containerBuilder);

            return containerBuilder;
        }

        private static ContainerBuilder BuildContainer(IServiceCollection services)
        {
            var factory = new AutofacServiceProviderFactory();
            var result = factory.CreateBuilder(services);
            return result;
        }
    }
}
