using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace APM.Extensions.Interceptor
{
    public class APMExtensionInterceptor : StandardInterceptor
    {
        private static ILogger<APMExtensionInterceptor>? _logger;

        public APMExtensionInterceptor(ILogger<APMExtensionInterceptor> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// 执行进程结果返回时拦截
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
        /// 执行进程前置处理拦截
        /// </summary>
        /// <param name="invocation"></param>
        protected override void PreProceed(IInvocation invocation)
        {
            var action = () => base.PreProceed(invocation);
            action.Invoke();
        }

        /// <summary>
        /// 执行进程完成执行时拦截
        /// </summary>
        /// <param name="invocation"></param>
        protected override void PostProceed(IInvocation invocation)
        {
            var action = () => base.PostProceed(invocation);
            action.Invoke();
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
                        _logger.Log(LogLevel.Information, $"{invocation.TargetType.FullName}.{invocation.Method.Name} excution took {stopwatch.ElapsedMilliseconds}ms");
                    }
                };
            }
        }
    }
}
