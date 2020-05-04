using Autofac;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace SenecSourceWebAppTest
{
    public static class SyncExtensions
    {
        public static TResult RunWait<TResult>(this Task<TResult> asyncTask)
        {
            var task = Task.Run(() => asyncTask);
            task.Wait();
            return task.Result;
        }

        public static void RunWait(this Task asyncTask)
        {
            var task = Task.Run(() => asyncTask);
            task.Wait();
        }

        public static void RunWait<TRequest>(this ILifetimeScope scope, TRequest request) where TRequest : IRequest<Unit>
        {
            var handler = scope.IsRegistered<IRequestHandler<TRequest>>()
                ? scope.Resolve<IRequestHandler<TRequest>>()
                : scope.Resolve<IRequestHandler<TRequest, Unit>>();

            handler.Handle(request, CancellationToken.None).RunWait();
        }

        public static TResponse RunSync<TRequest, TResponse>(this ILifetimeScope scope, TRequest request) where TRequest : IRequest<TResponse>
        {
            var handler = scope.Resolve<IRequestHandler<TRequest, TResponse>>();
            return handler.Handle(request, CancellationToken.None).RunWait();
        }
    }
}
