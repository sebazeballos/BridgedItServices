using Microsoft.Win32;
using System.Fabric.Query;
using System;

namespace BridgetItService.Services
{
    public class UpdateService : IDisposable
    {
        private readonly ApiHandler _apiHandler;
        private Timer _timer;

        public UpdateService(ApiHandler apiHandler)
        {
            _apiHandler = apiHandler;
        }

        public void Start()
        {
            _timer = new Timer(async _ => await UpdateAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
        }

        private async Task UpdateAsync()
        {
            DateTime currentTime = DateTime.Now;
            await _apiHandler.UpdateShopifyAsync(currentTime);
            //await _apiHandler.UpdateTriquestraInfinityPosAsync();
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
