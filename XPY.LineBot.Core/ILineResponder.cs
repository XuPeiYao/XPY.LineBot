using Line;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XPY.LineBot.Core {
    /// <summary>
    /// LINE機器人響應器
    /// </summary>
    public interface ILineResponder {
        /// <summary>
        /// 處理回應
        /// </summary>
        /// <param name="e">事件內容</param>
        /// <returns>是否攔截，如為<see cref="true"/>則表示不繼續傳遞給其他處理程序</returns>
        Task<bool> Handle(ILineEvent e);
    }
}
