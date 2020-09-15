using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using newsletter;

[assembly: FunctionsStartup(typeof(SwashBuckleStartup))]
namespace newsletter
{
        internal class SwashBuckleStartup : FunctionsStartup
        {
            public override void Configure(IFunctionsHostBuilder builder)
                => builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
        }
}