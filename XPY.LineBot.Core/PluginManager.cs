using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Threading;
using Line;
using System.IO;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace XPY.LineBot.Core {
    public class PluginManager {
        public static readonly PluginManager Instance = new PluginManager();


        private ReaderWriterLock _respondersLock = new ReaderWriterLock();
        List<Type> _respondersPipeline { get; set; } = new List<Type>();

        private PluginManager() { }

        /// <summary>
        /// 取得目前回應器的管線流程
        /// </summary>
        public IEnumerable<Type> ResponderPipeline {
            get {
                _respondersLock.AcquireWriterLock(TimeSpan.FromSeconds(1));
                var result = _respondersPipeline.ToList().AsReadOnly();
                _respondersLock.ReleaseWriterLock();

                return result;
            }
        }

        public async Task AddPlugin(Type lineResponderType) {
            _respondersLock.AcquireWriterLock(TimeSpan.FromSeconds(1));
            if (!lineResponderType.GetInterfaces().Contains(typeof(ILineResponder))) {
                _respondersLock.ReleaseWriterLock();

                throw new ArgumentException($"回應器類型必須實作{nameof(ILineResponder)}");
            }
            if (_respondersPipeline.Contains(lineResponderType)) {
                _respondersLock.ReleaseWriterLock();

                throw new ArgumentException($"回應器重複");
            }
            _respondersLock.ReleaseWriterLock();


            _respondersLock.AcquireReaderLock(TimeSpan.FromSeconds(1));
            _respondersPipeline.Add(lineResponderType);
            _respondersLock.ReleaseReaderLock();
        }
        public async Task AddPlugin<TResponder>()
            where TResponder : ILineResponder {
            await AddPlugin(typeof(TResponder));
        }

        public async Task MovePipeline(int index, Type lineResponderType) {
            _respondersLock.AcquireWriterLock(TimeSpan.FromSeconds(1));
            if (!_respondersPipeline.Contains(lineResponderType)) {
                _respondersLock.ReleaseWriterLock();
                throw new ArgumentException($"不存在此回應器");
            }
            _respondersLock.ReleaseWriterLock();

            _respondersLock.AcquireReaderLock(TimeSpan.FromSeconds(1));
            _respondersPipeline.Remove(lineResponderType);
            _respondersPipeline.Insert(index, lineResponderType);
            _respondersLock.ReleaseReaderLock();
        }
        public async Task MovePipeline<TResponder>(int index)
            where TResponder : ILineResponder {
            await MovePipeline(index, typeof(TResponder));
        }

        public async Task RemovePipeline(Type lineResponderType) {
            _respondersLock.AcquireWriterLock(TimeSpan.FromSeconds(1));
            if (!_respondersPipeline.Contains(lineResponderType)) {
                _respondersLock.ReleaseWriterLock();
                throw new ArgumentException($"不存在此回應器");
            }
            _respondersLock.ReleaseWriterLock();

            _respondersLock.AcquireReaderLock(TimeSpan.FromSeconds(1));
            _respondersPipeline.Remove(lineResponderType);
            _respondersLock.ReleaseReaderLock();
        }
        public async Task RemovePipeline<TResponder>()
            where TResponder : ILineResponder {
            await RemovePipeline(typeof(TResponder));
        }


        private async Task<ILineResponder> GetResponderInstance(HttpContext context, Type responder) {
            return await Task.Run(() => {
                var rConstructor = responder.GetConstructors().Single();
                List<object> rConstructorParam = new List<object>();
                foreach (var rParam in rConstructor.GetParameters()) {
                    if (rParam.ParameterType == typeof(PluginManager)) {
                        rConstructorParam.Add(this);
                    } else if (rParam.ParameterType.GetInterfaces().Contains(typeof(ILineResponder))) {
                        rConstructorParam.Add(GetResponderInstance(context, rParam.ParameterType));
                    } else {
                        rConstructorParam.Add(context.RequestServices.GetService(rParam.ParameterType));
                    }
                }

                return (ILineResponder)Activator.CreateInstance(responder, rConstructorParam.ToArray());
            });
        }


        public async Task ConfigurePlugin(IServiceCollection services, params Assembly[] plugins) {
            var mvc = services.AddMvc();
            foreach (var plugin in plugins) {
                mvc.AddApplicationPart(plugin);
            }

            foreach (var responder in plugins.SelectMany(x => x.GetTypes()).Where(x => x.GetInterfaces().Contains(typeof(ILineResponder)))) {
                await AddPlugin(responder);
            }

            foreach (var serviceConfigure in plugins.SelectMany(x => x.GetTypes()).Where(x => x.GetInterfaces().Contains(typeof(ILineServiceConfigure)))) {
                ((ILineServiceConfigure)Activator.CreateInstance(serviceConfigure)).ConfigureServices(services);
            }
        }

        public async Task Run(HttpContext context) {
            if (context.Request.Method != HttpMethods.Post) {
                context.Response.StatusCode = 404;
                return;
            }

            var bot = (ILineBot)context.RequestServices.GetService(typeof(ILineBot));
            var events = await bot.GetEvents(context.Request);

            foreach (var @event in events) {
                foreach (var responder in ResponderPipeline) {
                    ILineResponder responderInstance = await GetResponderInstance(context, responder);

                    var handled = await responderInstance.Handle(@event);
                    if (handled) {
                        continue;
                    }
                }
            }

            context.Response.StatusCode = 200;
        }
    }
}
