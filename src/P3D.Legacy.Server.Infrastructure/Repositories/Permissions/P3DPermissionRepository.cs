using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Infrastructure.Models.P3D;
using P3D.Legacy.Server.Infrastructure.Models.Permissions;
using P3D.Legacy.Server.Infrastructure.Utils;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Permissions
{
    [RequiresUnreferencedCode("Newtonsoft.Json")]
    public class P3DPermissionRepository
    {
        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly IHttpClientFactory _httpClientFactory;

        public P3DPermissionRepository(ILogger<P3DPermissionRepository> logger, TracerProvider traceProvider, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Infrastructure");
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<PermissionEntity> GetByGameJoltIdAsync(GameJoltId id, CancellationToken ct)
        {
            using var span = _tracer.StartActiveSpan("P3DPermissionRepository GetByGameJoltAsync", SpanKind.Client);

            var permissions = PermissionTypes.UnVerified;

            HttpResponseMessage response;

            try
            {
                using var client = _httpClientFactory.CreateClient("P3D.API");
                response = await client.GetAsync(
                    new Uri($"gamejoltaccount/{id}", UriKind.Relative),
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);
            }
            catch (Exception e) when (e is TaskCanceledException or HttpRequestException)
            {
                return new PermissionEntity(permissions);
            }

            try
            {
                if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    if (JsonConvert.DeserializeObject<GameJoltResponseDTO?>(content) is { User: { }, UserGameJolt: { } } dto)
                    {
                        permissions &= ~PermissionTypes.UnVerified;
                        permissions |= PermissionTypes.User;

                        foreach (var permissionDto in dto.User.Roles?.SelectMany(static x => x.Permissions) ?? Enumerable.Empty<PermissionDTO>())
                        {
                            if (permissionDto.Name.Equals("gameserver.debug", StringComparison.OrdinalIgnoreCase))
                                permissions |= PermissionTypes.Debug;

                            if (permissionDto.Name.Equals("gameserver.moderator", StringComparison.OrdinalIgnoreCase))
                                permissions |= PermissionTypes.Moderator;

                            if (permissionDto.Name.Equals("gameserver.admin", StringComparison.OrdinalIgnoreCase))
                                permissions |= PermissionTypes.Administrator;

                            if (permissionDto.Name.Equals("gameserver.server", StringComparison.OrdinalIgnoreCase))
                                permissions |= PermissionTypes.Server;
                        }
                        return new PermissionEntity(permissions);
                    }
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    permissions &= ~PermissionTypes.UnVerified;
                    permissions |= PermissionTypes.User;
                }

                return new PermissionEntity(permissions);
            }
            finally
            {
                response.Dispose();
            }
        }

        [JsonConverter(typeof(JsonPathConverter))]
        [SuppressMessage("Performance", "CA1812")]
        private record UserResponseDTO(
            [property: JsonProperty("data")] UserDTO User,
            [property: JsonProperty("data.gamejolt")] GameJoltDTO UserGameJolt,
            [property: JsonProperty("data.forum")] ForumDTO UserForum);

        [JsonConverter(typeof(JsonPathConverter))]
        [SuppressMessage("Performance", "CA1812")]
        private record GameJoltResponseDTO(
            [property: JsonProperty("data.user")] UserDTO User,
            [property: JsonProperty("data")] GameJoltDTO UserGameJolt);

        [JsonConverter(typeof(JsonPathConverter))]
        [SuppressMessage("Performance", "CA1812")]
        private record ForumResponseDTO(
            [property: JsonProperty("data.user")] UserDTO User,
            [property: JsonProperty("data")] ForumDTO UserForum);
    }
}