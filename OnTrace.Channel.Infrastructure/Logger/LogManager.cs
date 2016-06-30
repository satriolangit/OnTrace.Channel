using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnTrace.Channel.Core.Interfaces;
using OnTrace.Channel.Infrastructure.Services;

namespace OnTrace.Channel.Infrastructure.Logger
{
    public static class LogManager
    {
        public static ILogger GetLogger(Type type)
        {
            return new Log4NetLogger(type);
        }
    }
}
