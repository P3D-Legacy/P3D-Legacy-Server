using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace P3D.Legacy.Server.Extensions
{
    public class ServerBuilderContext
    {
        public IHostEnvironment HostingEnvironment { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public ServerBuilderContext(IHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            HostingEnvironment = hostingEnvironment;
            Configuration = configuration;
        }
    }
}