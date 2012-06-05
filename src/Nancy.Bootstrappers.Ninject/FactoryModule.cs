namespace Nancy.Bootstrappers.Ninject
{
    using System;
    using System.Linq;
    using global::Ninject;
    using global::Ninject.Activation;
    using global::Ninject.Modules;
    using global::Ninject.Parameters;

    /// <summary>
    /// Helper module for Ninject, in order to be able to resolve <see cref="Func{T}"/> dependencies.
    /// </summary>
    public class FactoryModule : NinjectModule
    {
        /// <summary>
        /// Loads the module into the kernel.
        /// </summary>
        public override void Load()
        {
            this.Kernel.Bind(typeof(Func<>)).ToMethod(CreateFunc).When(VerifyFactoryFunction);
        }

        private static bool VerifyFactoryFunction(IRequest request)
        {
            var genericArguments = request.Service.GetGenericArguments();
            if (genericArguments.Count() != 1)
            {
                return false;
            }

            var instanceType = genericArguments.Single();
            return request.ParentContext.Kernel.CanResolve(new Request(genericArguments[0], null, new IParameter[0], null, false, true)) ||
                   TypeIsSelfBindable(instanceType);
        }

        private static object CreateFunc(IContext ctx)
        {
            var functionFactoryType = typeof(FunctionFactory<>).MakeGenericType(ctx.GenericArguments);
            var ctor = functionFactoryType.GetConstructors().Single();
            var functionFactory = ctor.Invoke(new object[] { ctx.Kernel });
            return functionFactoryType.GetMethod("Create").Invoke(functionFactory, new object[0]);
        }

        private static bool TypeIsSelfBindable(Type service)
        {
            return !service.IsInterface
                   && !service.IsAbstract
                   && !service.IsValueType
                   && service != typeof(string)
                   && !service.ContainsGenericParameters;
        }

        public class FunctionFactory<T>
        {
            private readonly IKernel kernel;

            public FunctionFactory(IKernel kernel)
            {
                this.kernel = kernel;
            }

            public Func<T> Create()
            {
                return () => this.kernel.Get<T>();
            }
        }
    }
}