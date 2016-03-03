namespace Nancy.Bootstrappers.Ninject.Tests
{
    using System;
    using System.Collections.Generic;

    using Bootstrapper;
    using global::Ninject;

    using Nancy.Tests.Unit.Bootstrapper.Base;

    /// <summary>
    /// Fake Ninject boostrapper that can be used for testing.
    /// </summary>
    public class FakeNinjectNancyBootstrapper : NinjectNancyBootstrapper
    {
        public bool RequestContainerConfigured { get; set; }
        public bool ApplicationContainerConfigured { get; set; }
        private readonly Func<ITypeCatalog, NancyInternalConfiguration> configuration;

        public FakeNinjectNancyBootstrapper()
            : this(null)
        {
        }

        public FakeNinjectNancyBootstrapper(Func<ITypeCatalog, NancyInternalConfiguration> configuration)
        {
            this.configuration = configuration;
        }

        protected override IEnumerable<Type> RegistrationTasks
        {
            get
            {
                return new[] { typeof(TestRegistrations) };
            }
        }

        protected override Func<ITypeCatalog, NancyInternalConfiguration> InternalConfiguration
        {
            get { return configuration ?? base.InternalConfiguration; }
        }

        protected override void ConfigureApplicationContainer(IKernel existingContainer)
        {
            base.ConfigureApplicationContainer(existingContainer);
            this.ApplicationContainerConfigured = true;
        }

        protected override void ConfigureRequestContainer(IKernel container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            container.Bind(typeof(IFoo)).To(typeof(Foo)).InSingletonScope();
            container.Bind(typeof(IDependency)).To(typeof(Dependency)).InSingletonScope();

            this.RequestContainerConfigured = true;
        }

        public T Resolve<T>()
        {
            return this.ApplicationContainer.Get<T>();
        }
    }
}