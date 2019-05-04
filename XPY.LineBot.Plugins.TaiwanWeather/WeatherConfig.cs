using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using XPY.LineBot.Core;

namespace XPY.LineBot.Plugins.TaiwanWeather {
    public class WeatherConfig : ILineServiceConfigure {
        public static Dictionary<string, string> CityCode { get; set; } = new Dictionary<string, string>();
        public void ConfigureServices(IServiceCollection services) {
            HttpClient client = new HttpClient();

            var cityList = JArray.Parse(client.GetStringAsync("https://works.ioa.tw/weather/api/all.json").GetAwaiter().GetResult());

            foreach (var city in cityList) {
                CityCode[city["name"].Value<string>()] = city["id"].Value<string>();
            }
        }
    }
}
