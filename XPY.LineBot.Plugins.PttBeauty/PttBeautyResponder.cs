﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Line;
using StackExchange.Redis;
using XPY.LineBot.Core;

namespace XPY.LineBot.Plugins.PttBeauty {
    public class PttBeautyResponder : ILineResponder {
        public ILineBot Bot { get; private set; }
        public PttBeautyResponder(ILineBot bot) {
            Bot = bot;
        }

        public HttpClient httpClient = new HttpClient();
        public async Task<bool> Handle(ILineEvent e) {
            if (e.EventType != LineEventType.Message) return false;

            Regex command = new Regex(@"\S*妹子[!！]?$");

            if (!command.IsMatch(e.Message?.Text ?? "")) return false;

            var keyword = e.Message.Text.Substring(0, e.Message.Text.IndexOf("妹子"));

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
                        Text = "對不起!!現在找不到你要的妹子"
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
                                            Label = "再來一個妹子",
                                            Text = e.Message.Text
                                        },
                                        new UriAction() {
                                            Label= "開啟圖片",
                                            Url = x
                                        }
                                    },
                                    ThumbnailUrl = x,
                                    Title = "妹子",
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
                var key = PttBeautyConfig.redisDatabase.KeyRandom(CommandFlags.NoScriptCache);

                var length = await PttBeautyConfig.redisDatabase.ListLengthAsync(key);

                int index = rnd.Next((int)length);

                string url = await PttBeautyConfig.redisDatabase.ListGetByIndexAsync(key, index);

                var response = await httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                    Console.WriteLine(url);
                    return url;
                } else {
                    PttBeautyConfig.redisDatabase.ListRemove(key, url);
                }
            }
            return null;
        }

        public async Task<string> GetImageUrlByKeyword(string keyword) {
            Random rnd = new Random((int)DateTime.Now.Ticks);

            for (int i = 0; i < 10; i++) {
                var keys = PttBeautyConfig.redisServer.Keys(0, $"*{keyword.Replace("的", "")}*", 10).ToList();

                if (keys.Count == 0) {
                    return null;
                }

                int index = rnd.Next(keys.Count);

                var key = keys[index];

                var length = await PttBeautyConfig.redisDatabase.ListLengthAsync(key);

                index = rnd.Next((int)length);

                var url = await PttBeautyConfig.redisDatabase.ListGetByIndexAsync(key, index);
                var response = await httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                    Console.WriteLine(url);
                    return url;
                } else {
                    PttBeautyConfig.redisDatabase.ListRemove(key, url);
                }
            }
            return null;
        }

        public static void Main(string[] args) {
        }
    }
}
