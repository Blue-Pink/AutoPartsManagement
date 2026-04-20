using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace APM.Extensions.Interceptor
{
    public class APMExtensionInterceptor : StandardInterceptor
    {
        private static ILogger<APMExtensionInterceptor>? _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public APMExtensionInterceptor(ILogger<APMExtensionInterceptor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 执行进程结果返回时拦截（主要负责构建执行管线并最终执行）
        /// </summary>
        /// <param name="invocation"></param>
        protected override void PerformProceed(IInvocation invocation)
        {
            var action = () => base.PerformProceed(invocation);


            if (invocation.Method.IsDefined(typeof(APMInterceptorAttribute), true))
            {
                action = invocation.Method
                    .GetCustomAttributes<APMInterceptorAttribute>()
                    .Reverse()
                    .Aggregate(action, (currentAction, attribute) => attribute.Invoke(invocation, currentAction));
            }

            action.Invoke();
        }

        /// <summary>
        /// 执行进程前置处理拦截（留作扩展点）
        /// </summary>
        /// <param name="invocation"></param>
        protected override void PreProceed(IInvocation invocation)
        {
            try
            {
                var method = invocation.MethodInvocationTarget ?? invocation.Method;
                var typeName = invocation.TargetType?.FullName ?? invocation.Proxy?.GetType().Name ?? "UnknownType";
                var argsRep = SafeSerializeArguments(invocation.Arguments);
                _logger?.LogDebug("PreProceed {Type}.{Method} Args: {Args}", typeName, method.Name, argsRep);
            }
            catch
            {
                // 记录失败不影响主流程
            }

            base.PreProceed(invocation);
        }

        /// <summary>
        /// 执行进程完成执行时拦截（留作扩展点）
        /// </summary>
        /// <param name="invocation"></param>
        protected override void PostProceed(IInvocation invocation)
        {
            try
            {
                var method = invocation.MethodInvocationTarget ?? invocation.Method;
                var typeName = invocation.TargetType?.FullName ?? invocation.Proxy?.GetType().Name ?? "UnknownType";
                _logger?.LogDebug("PostProceed {Type}.{Method}", typeName, method.Name);
            }
            catch
            {
                // 忽略
            }

            base.PostProceed(invocation);
        }

        [AttributeUsage(AttributeTargets.Method)]
        public abstract class APMInterceptorAttribute : Attribute
        {
            public abstract Action Invoke(IInvocation invocation, Action action);
        }

        public class Monitor : APMInterceptorAttribute
        {
            public override Action Invoke(IInvocation invocation, Action action)
            {
                return () =>
                {
                    if (_logger != null && invocation != null && invocation.TargetType != null)
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        action.Invoke();
                        stopwatch.Stop();
                        _logger.Log(LogLevel.Information, "{Type}.{Method} excution took {Ms}ms",
                            invocation.TargetType.FullName, invocation.Method.Name, stopwatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        action.Invoke();
                    }
                };
            }
        }

        #region Helpers
        private static string SafeSerializeArguments(object?[] args)
        {
            if (args == null || args.Length == 0) return "[]";
            try
            {
                return JsonSerializer.Serialize(args, _jsonOptions);
            }
            catch
            {
                try
                {
                    return "[" + string.Join(", ", args.Select(a => a == null ? "null" : a.ToString())) + "]";
                }
                catch
                {
                    return "[unserializable]";
                }
            }
        }

        private static string SafeSerializeReturnValue(object? returnValue)
        {
            if (returnValue == null) return "null";
            try
            {
                return JsonSerializer.Serialize(returnValue, _jsonOptions);
            }
            catch
            {
                try
                {
                    return returnValue.ToString() ?? "null";
                }
                catch
                {
                    return "unserializable";
                }
            }
        }
        #endregion
    }
}
