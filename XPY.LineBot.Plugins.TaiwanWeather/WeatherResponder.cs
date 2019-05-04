using Line;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XPY.LineBot.Core;

namespace XPY.LineBot.Plugins.TaiwanWeather {
    public class WeatherResponder : ILineResponder {
        public ILineBot Bot { get; private set; }
        static HttpClient client = new HttpClient();
        public WeatherResponder(ILineBot bot) {
            Bot = bot;
        }

        public async Task<bool> Handle(ILineEvent e) {
            Regex check = new Regex(@"(今天|現在|當前|當下)?(?<City>.+)天氣((如何|怎樣|狀況|情形)\??)?$");

            if (!check.IsMatch(e.Message.Text ?? "")) {
                return false;
            }

            var cityname = check.Match(e.Message.Text).Groups["City"].Value;
            cityname = cityname.Replace("臺", "台").Replace("縣", "").Replace("市", "");

            if (!WeatherConfig.CityCode.ContainsKey(cityname)) {
                await Bot.Reply(e.ReplyToken, new TextMessage("找不到您說的地區:" + cityname));

                return true;
            }

            var citycode = WeatherConfig.CityCode[cityname];

            var weatherInfo = JObject.Parse(await client.GetStringAsync($"https://works.ioa.tw/weather/api/weathers/{citycode}.json"));

            await Bot.Reply(e.ReplyToken, new TextMessage(
                $"{cityname}地區目前天氣:【{weatherInfo["desc"].Value<string>()}】\r\n" +
                $"溫度:{weatherInfo["temperature"].Value<string>()}度(體感溫度:{weatherInfo["felt_air_temp"].Value<string>()})\r\n" +
                $"濕度:{weatherInfo["humidity"].Value<string>()}%\r\n" +
                $"降雨量:{weatherInfo["rainfall"].Value<string>()}mm"
            ));

            return true;
        }

        public static void Main(string[] args) {
            Regex check = new Regex(@"(今天|現在|當前|當下)?(?<City>.+)天氣((如何|怎樣|狀況|情形)\??)?$");

            if (!check.IsMatch("今天高雄天氣")) {

            }

            var cityname = check.Match("今天高雄天氣").Groups["City"].Value;
            cityname = cityname.Replace("臺", "台").Replace("縣", "").Replace("市", "");


        }
    }
}
