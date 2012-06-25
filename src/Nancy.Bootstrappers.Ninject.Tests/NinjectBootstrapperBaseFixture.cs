#if !__MonoCS__ 
namespace Nancy.Bootstrappers.Ninject.Tests
{
    using Bootstrapper;
    using global::Ninject;

    public class NinjectBootstrapperBaseFixture : Nancy.Tests.Unit.Bootstrapper.Base.BootstrapperBaseFixtureBase<IKernel>
    {
        private readonly NinjectNancyBootstrapper boostrapper;

        public NinjectBootstrapperBaseFixture()
        {
            this.boostrapper = new FakeNinjectNancyBootstrapper(this.Configuration);
        }

        protected override NancyBootstrapperBase<IKernel> Bootstrapper
        {
            get { return this.boostrapper; }
        }
    }
}
#endif