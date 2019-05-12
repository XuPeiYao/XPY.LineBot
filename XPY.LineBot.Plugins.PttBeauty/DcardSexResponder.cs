using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Line;
using StackExchange.Redis;
using XPY.LineBot.Core;

namespace XPY.LineBot.Plugins.PttBeauty {
    public class DcardSexResponder : ILineResponder {
        public ILineBot Bot { get; private set; }
        public DcardSexResponder(ILineBot bot) {
            Bot = bot;
        }

        public HttpClient httpClient = new HttpClient();
        public async Task<bool> Handle(ILineEvent e) {
            if (e.EventType != LineEventType.Message) return false;

            if (!e.Message?.Text?.StartsWith("開車") ?? true) return false;

            var keyword = e.Message.Text.Substring(2).Replace("!", "").Replace("！", "");

            HttpClient client = new HttpClient();
            List<Uri> images = new List<Uri>();

            if (keyword.Length == 0) {
                for (int i = 0; i < 3; i++) {
                    var url = await GetImageUrl();
                    if (url != null) {
                        url = url.Replace("http://", "https://");

                        images.Add(new Uri(url));
                    }
                    if (keyword.EndsWith("!") || keyword.EndsWith("！")) continue;
                }
            } else {
                for (int i = 0; i < 3; i++) {
                    var url = await GetImageUrlByKeyword(keyword);
                    if (url != null) {
                        url = url.Replace("http://", "https://");

                        images.Add(new Uri(url));
                    }
                    if (keyword.EndsWith("!") || keyword.EndsWith("！")) continue;
                }
            }


            if (images.Count == 0) {
                await Bot.Reply(e.ReplyToken,
                    new TextMessage() {
                        Text = "老司機現在找不到你要的車!"
                    });
            } else {
                ISendMessage message;
                if (e.Message.Text.Contains("!") || e.Message.Text.Contains("！")) {
                    message = new ImageMessage() {
                        Url = images.First(),
                        PreviewUrl = images.First()
                    };
                } else {
                    message = new TemplateMessage() {
                        Template = new CarouselTemplate() {
                            Columns = images.Select(x =>
                                new CarouselColumn() {
                                    Actions = new IAction[]{
                                        new MessageAction() {
                                            Label = "再開一次車",
                                            Text = e.Message.Text
                                        },
                                        new UriAction() {
                                            Label= "開啟圖片",
                                            Url = x
                                        }
                                    },
                                    ThumbnailUrl = x,
                                    Title = "開車",
                                    Text = x.ToString()
                                }).ToArray()
                        },
                        AlternativeText = "您的裝置不支援顯示此內容，請嘗試使用智慧型手機觀看或在指令後加入驚嘆號!"
                    };

                }
                await Bot.Reply(e.ReplyToken, message);
            }
            GC.Collect();
            return true;
        }

        public async Task<string> GetImageUrl() {
            Random rnd = new Random((int)DateTime.Now.Ticks);

            for (int i = 0; i < 10; i++) {
                var key = DcardSexConfig.redisDatabase.KeyRandom(CommandFlags.NoScriptCache);

                var length = await DcardSexConfig.redisDatabase.ListLengthAsync(key);

                int index = rnd.Next((int)length);

                string url = await DcardSexConfig.redisDatabase.ListGetByIndexAsync(key, index);

                try {
                    var response = await httpClient.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                        Console.WriteLine(url);
                        return url;
                    } else {
                        DcardSexConfig.redisDatabase.ListRemove(key, url);
                    }
                } catch {
                    DcardSexConfig.redisDatabase.ListRemove(key, url);
                }
            }
            return null;
        }

        public async Task<string> GetImageUrlByKeyword(string keyword) {
            Random rnd = new Random((int)DateTime.Now.Ticks);

            for (int i = 0; i < 10; i++) {
                var keys = DcardSexConfig.redisServer.Keys(0, $"*{keyword.Replace("的", "")}*", 10).ToList();

                if (keys.Count == 0) {
                    return null;
                }

                int index = rnd.Next(keys.Count);

                var key = keys[index];

                var length = await DcardSexConfig.redisDatabase.ListLengthAsync(key);

                index = rnd.Next((int)length);

                var url = await DcardSexConfig.redisDatabase.ListGetByIndexAsync(key, index);
                try {
                    var response = await httpClient.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                        Console.WriteLine(url);
                        return url;
                    } else {
                        DcardSexConfig.redisDatabase.ListRemove(key, url);
                    }
                } catch {
                    DcardSexConfig.redisDatabase.ListRemove(key, url);
                }
            }
            return null;
        }

        public static void Main(string[] args) {
        }
    }
}
