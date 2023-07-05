using Microsoft.Win32;
using System.Fabric.Query;
using System;
using BridgetItService.Settings;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Net.Http.Headers;

namespace BridgetItService.Services
{
    public class UpdateService : IDisposable
    {
        private readonly ApiHandler _apiHandler;
        private Timer _timer;
        private readonly IOptions<IntervalMinutes> _options;
        public UpdateService(ApiHandler apiHandler, IServiceProvider provider)
        {
            _options = provider.GetService<IOptions<IntervalMinutes>>();
            _apiHandler = apiHandler;
        }

        public void Start()
        {
            _timer = new Timer(async _ => await UpdateAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(_options.Value.Minutes));
        }

        private async Task UpdateAsync()
        {
            DateTime currentTime = DateTime.Now.AddMinutes(-_options.Value.Minutes);
            await _apiHandler.UpdateShopifyAsync(currentTime);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
