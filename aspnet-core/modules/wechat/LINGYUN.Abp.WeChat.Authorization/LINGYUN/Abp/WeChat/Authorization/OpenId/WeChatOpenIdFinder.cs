﻿using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using Volo.Abp.MultiTenancy;

namespace LINGYUN.Abp.WeChat.Authorization
{
    [Dependency(ServiceLifetime.Singleton, ReplaceServices = true)]
    [ExposeServices(typeof(IWeChatOpenIdFinder))]
    public class WeChatOpenIdFinder : IWeChatOpenIdFinder
    {
        public ILogger<WeChatOpenIdFinder> Logger { get; set; }
        protected AbpWeChatOptions Options { get; }
        protected ICurrentTenant CurrentTenant { get; }
        protected IHttpClientFactory HttpClientFactory { get; }
        protected IJsonSerializer JsonSerializer { get; }
        protected IUserWeChatCodeFinder UserWeChatCodeFinder { get; }
        protected IDistributedCache<WeChatOpenIdCacheItem> Cache { get; }
        public WeChatOpenIdFinder(
            ICurrentTenant currentTenant,
            IJsonSerializer jsonSerializer,
            IUserWeChatCodeFinder userWeChatCodeFinder,
            IHttpClientFactory httpClientFactory,
            IOptions<AbpWeChatOptions> options,
            IDistributedCache<WeChatOpenIdCacheItem> cache)
        {
            CurrentTenant = currentTenant;
            JsonSerializer = jsonSerializer;
            UserWeChatCodeFinder = userWeChatCodeFinder;
            HttpClientFactory = httpClientFactory;

            Cache = cache;
            Options = options.Value;

            Logger = NullLogger<WeChatOpenIdFinder>.Instance;
        }
        public virtual async Task<WeChatOpenId> FindAsync(string code)
        {
            // TODO: 如果需要获取SessionKey的话呢，需要再以openid作为标识来缓存一下吗
            // 或者前端保存code,通过传递code来获取
            return (await GetCacheItemAsync(code)).WeChatOpenId;
        }

        public virtual async Task<WeChatOpenId> FindByUserIdAsync(Guid userId)
        {
            var code = await UserWeChatCodeFinder.FindByUserIdAsync(userId);
            // TODO: 如果需要获取SessionKey的话呢，需要再以openid作为标识来缓存一下吗
            // 或者前端保存code,通过传递code来获取
            return (await GetCacheItemAsync(code)).WeChatOpenId;
        }

        public virtual async Task<WeChatOpenId> FindByUserNameAsync(string userName)
        {
            var code = await UserWeChatCodeFinder.FindByUserNameAsync(userName);
            // TODO: 如果需要获取SessionKey的话呢，需要再以openid作为标识来缓存一下吗
            // 或者前端保存code,通过传递code来获取
            return (await GetCacheItemAsync(code)).WeChatOpenId;
        }



        protected virtual async Task<WeChatOpenIdCacheItem> GetCacheItemAsync(string code)
        {
            var cacheKey = WeChatOpenIdCacheItem.CalculateCacheKey(code);

            Logger.LogDebug($"WeChatOpenIdFinder.GetCacheItemAsync: {cacheKey}");

            var cacheItem = await Cache.GetAsync(cacheKey);

            if (cacheItem != null)
            {
                Logger.LogDebug($"Found in the cache: {cacheKey}");
                return cacheItem;
            }

            Logger.LogDebug($"Not found in the cache, getting from the httpClient: {cacheKey}");

            var client = HttpClientFactory.CreateClient("WeChatRequestClient");

            var request = new WeChatOpenIdRequest
            {
                BaseUrl = client.BaseAddress.AbsoluteUri,
                AppId = Options.AppId,
                Secret = Options.AppSecret,
                Code = code
            };

            var response = await client.RequestWeChatOpenIdAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            var weChatOpenIdResponse = JsonSerializer.Deserialize<WeChatOpenIdResponse>(responseContent);
            var weChatOpenId = weChatOpenIdResponse.ToWeChatOpenId();
            cacheItem = new WeChatOpenIdCacheItem(code, weChatOpenId);

            Logger.LogDebug($"Setting the cache item: {cacheKey}");

            var cacheOptions = new DistributedCacheEntryOptions
            {
                // 微信官方文档表示 session_key的有效期是3天
                // https://developers.weixin.qq.com/community/develop/doc/000c2424654c40bd9c960e71e5b009
                AbsoluteExpiration = DateTimeOffset.Now.AddDays(3).AddSeconds(-120)
                // SlidingExpiration = TimeSpan.FromDays(3),
            };


            await Cache.SetAsync(cacheKey, cacheItem, cacheOptions);

            Logger.LogDebug($"Finished setting the cache item: {cacheKey}");

            return cacheItem;
        }
    }
}
