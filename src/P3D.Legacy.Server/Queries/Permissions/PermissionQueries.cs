using P3D.Legacy.Server.Services;

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Queries.Permissions
{
    public class PermissionQueries : IPermissionQueries
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DefaultJsonSerializer _jsonSerializer;

        public PermissionQueries(IHttpClientFactory httpClientFactory, DefaultJsonSerializer jsonSerializer)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        }

        public async Task<PermissionViewModel?> GetGameJoltAsync(ulong id, CancellationToken ct)
        {
            HttpResponseMessage response;

            try
            {
                response = await _httpClientFactory.CreateClient("P3D.API").GetAsync(
                    $"gamejoltaccount/{id}",
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);
            }
            catch (Exception e) when (e is TaskCanceledException)
            {
                return null;
            }

            try
            {
                /*
                if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                {
                    var content = await response.Content.ReadAsStreamAsync(ct);
                    if (await _jsonSerializer.DeserializeAsync<PermissionDTO?>(content, ct) is { } tuple)
                    {
                        var (gameId, modId, threadId) = tuple;
                        return new PermissionViewModel(gameId, modId, threadId);
                    }
                }
                */
                return null;
            }
            finally
            {
                response.Dispose();
            }
        }
    }
}