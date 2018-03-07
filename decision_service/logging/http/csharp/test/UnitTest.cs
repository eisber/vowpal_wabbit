using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.DecisionService.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
//using Microsoft.Owin.Hosting;

namespace Microsoft.DecisionService.Logging.Tests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public async Task TestReward()
        {
            // 1. EventHub, credentials?
            // 2. C# web server, rewrite tests for python
            // 3. test driver in python, re-use driver for all languages 
            //WebApp.Start("http://localhost:9000", builder => { });

            using (var dataLogger = new DataLogger("Endpoint=sb://ingest-bj3pkxqvkcb72.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Jvu8e/iprIq8lQQil0Yl6D7TCZF0zuK6wKHQc/K7/zU=;EntityPath=interaction")
            {
                BatchingTimeout = TimeSpan.FromSeconds(1)
            })
            using (var rewardLogger = new RewardLogger("Endpoint=sb://ingest-bj3pkxqvkcb72.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Jvu8e/iprIq8lQQil0Yl6D7TCZF0zuK6wKHQc/K7/zU=;EntityPath=observation"))
            {
                // Add version
                var eventId = Guid.NewGuid().ToString("n");
                dataLogger.Log(eventId, "{\"s\":25,\"_multi\":[{\"a\":1},{\"a\":2},{\"a\":3}]}", new[] { .8f, .1f, .1f }, new[] { 2, 0, 1 }, "v1");
                 
                await rewardLogger.RewardAsync(eventId, 25f);

                await dataLogger.StopAndDrainAsync();
            }
        }
    }
}