using System;
using System.Collections.Generic;
using System.Text;

namespace XPY.LineBot.Plugins.GitHubWebHook.Models {
    public interface IGitHubSender {
        string Type { get; }
    }
}
