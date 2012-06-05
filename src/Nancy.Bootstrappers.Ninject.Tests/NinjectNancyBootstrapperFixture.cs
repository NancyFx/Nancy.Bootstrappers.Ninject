namespace Nancy.Bootstrappers.Ninject.Tests
{
    using Nancy.Tests;
    using Xunit;

    public class NinjectNancyBootstrapperFixture
    {
        private readonly FakeNinjectNancyBootstrapper bootstrapper;

        public NinjectNancyBootstrapperFixture()
        {
            this.bootstrapper = new FakeNinjectNancyBootstrapper();
            this.bootstrapper.Initialise();
        }

        [Fact]
        public void Should_be_able_to_resolve_engine()
        {
            // Given
            // When
            var engine = bootstrapper.GetEngine();

            // Then
            engine.ShouldNotBeNull();
        }
    }
}
