using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SenecSourceWebAppTest
{
    [TestClass]
    public class AssemblySetup
    {

        public static ContainerBuilder BuildContainer(IServiceCollection services)
        {
            var factory = new AutofacServiceProviderFactory();
            var cb = factory.CreateBuilder(services);
            return cb;
        }
    }
}
