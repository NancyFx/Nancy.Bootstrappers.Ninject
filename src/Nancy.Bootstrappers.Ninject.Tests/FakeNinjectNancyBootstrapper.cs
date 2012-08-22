namespace Nancy.Bootstrappers.Ninject.Tests
{
    using Bootstrapper;

    /// <summary>
    /// Fake Ninject boostrapper that can be used for testing.
    /// </summary>
    public class FakeNinjectNancyBootstrapper : NinjectNancyBootstrapper
    {
        private readonly NancyInternalConfiguration configuration;

        public FakeNinjectNancyBootstrapper()
            : this(null)
        {
        }

        public FakeNinjectNancyBootstrapper(NancyInternalConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get { return configuration ?? base.InternalConfiguration; }

        }
    }
}