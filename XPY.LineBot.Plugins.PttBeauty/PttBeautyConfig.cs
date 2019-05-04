using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using XPY.LineBot.Core;

namespace XPY.LineBot.Plugins.PttBeauty {
    public class PttBeautyConfig : ILineServiceConfigure {
        public static IDatabase redisDatabase { get; set; }
        public static IServer redisServer { get; set; }
        public void ConfigureServices(IServiceCollection services) {
            var parser = new FileIniDataParser();
            var iniPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "pttBeauty.ini");
            IniData data = parser.ReadFile(iniPath);

            var config = ConfigurationOptions.Parse(data.GetKey("RedisConnectionString"));
            var redisConnection = ConnectionMultiplexer.Connect(config);
            redisDatabase = redisConnection.GetDatabase();

            redisServer = redisConnection.GetServer(config.EndPoints.First());
        }
    }
}
