using System;
using System.Threading.Tasks;
using Line;
using XPY.LineBot.Core;

namespace XPY.LineBot.Plugins.Echo {
    public class EchoResponder : ILineResponder {
        public ILineBot Bot { get; private set; }

        public EchoResponder(ILineBot bot) {
            Bot = bot;
        }

        public async Task<bool> Handle(ILineEvent e) {
            if (!e.Message.Text?.StartsWith("echo:", StringComparison.CurrentCultureIgnoreCase) ?? true) {
                return false;
            }

            var message = e.Message.Text.Substring(5);
            await Bot.Reply(e.ReplyToken, new TextMessage(message));

            return true;
        }
    }
}
