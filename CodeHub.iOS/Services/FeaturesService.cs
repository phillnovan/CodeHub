using CodeHub.Core.Services;
using CodeFramework.Core.Services;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace CodeHub.iOS.Services
{
    public class FeaturesService : IFeaturesService
    {
        private readonly IDefaultValueService _defaultValueService;
        private readonly IHttpClientService _httpClientService;
        private readonly IJsonSerializationService _jsonSerializationService;


        public FeaturesService(IDefaultValueService defaultValueService, IHttpClientService httpClientService, IJsonSerializationService jsonSerializationService)
        {
            _defaultValueService = defaultValueService;
            _httpClientService = httpClientService;
            _jsonSerializationService = jsonSerializationService;
        }

        public bool IsPushNotificationsActivated
        {
            get
            {
                return IsActivated(FeatureIds.PushNotifications);
            }
            set
            {
                _defaultValueService.Set(FeatureIds.PushNotifications, value);
            }
        }

        public void Activate(string id)
        {
            InAppPurchases.Instance.PurchaseProduct(id);
        }

        public bool IsActivated(string id)
        {
            bool value;
            return _defaultValueService.TryGet<bool>(id, out value) && value;
        }

        public async Task<IEnumerable<string>> GetAvailableFeatureIds()
        {
            var client = _httpClientService.Create();
            client.Timeout = new TimeSpan(0, 0, 15);
            var response = await client.GetAsync("http://push.codehub-app.com/in-app");
            var data = await response.Content.ReadAsStringAsync();
            return _jsonSerializationService.Deserialize<List<string>>(data).ToArray();
        }
    }
}

