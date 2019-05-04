using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace XPY.LineBot.Core {
    public interface ILineServiceConfigure {
        void ConfigureServices(IServiceCollection services);
    }
}
