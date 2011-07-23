namespace Nancy.Bootstrappers.Ninject
{
    using System;
    using System.Collections.Generic;
    using Nancy.Bootstrapper;
    using global::Ninject;
    using global::Ninject.Extensions.ChildKernel;

    /// <summary>
    /// Ninject IoC Container Bootstrapper  
    /// </summary>
    public class NinjectNancyBootstrapper : NancyBootstrapperWithRequestContainerBase<IKernel>
    {
        /// <summary>
        /// Gets all registered startup tasks
        /// </summary>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> instance containing <see cref="IStartup"/> instances. </returns>
        protected override IEnumerable<IStartup> GetStartupTasks()
        { 
            return this.ApplicationContainer.GetAll<IStartup>();
        }

        /// <summary>
        /// Configures the container by registrating of default 
        /// INancyModuleCatalog and IRouteResolver.
        /// </summary>
        /// <param name="container">Container instance</param>
        protected override void ConfigureApplicationContainer(IKernel container)
        {
            this.RegisterBootstrapperTypes(container);
        }

        /// <summary>
        /// Get INancyEngine
        /// </summary>
        /// <returns>INancyEngine implementation</returns>
        protected override sealed INancyEngine GetEngineInternal()
        {
            return this.ApplicationContainer.Get<INancyEngine>();
        }

        /// <summary>
        /// Get the moduleKey generator
        /// </summary>
        /// <returns>IModuleKeyGenerator instance</returns>
        protected override sealed IModuleKeyGenerator GetModuleKeyGenerator()
        {
            return this.ApplicationContainer.Get<IModuleKeyGenerator>();
        }

        /// <summary>
        /// Create a default, unconfigured, container
        /// </summary>
        /// <returns>Container instance</returns>
        protected override IKernel GetApplicationContainer()
        {
            return new StandardKernel();
        }

        /// <summary>
        /// Bind the bootstrapper's implemented types into the container.
        /// This is necessary so a user can pass in a populated container but not have
        /// to take the responsibility of registering things like INancyModuleCatalog manually.
        /// </summary>
        /// <param name="applicationContainer">Application container to register into</param>
        protected override sealed void RegisterBootstrapperTypes(IKernel applicationContainer)
        {
            applicationContainer.Bind<INancyModuleCatalog>().ToConstant(this);
        }

        /// <summary>
        /// Bind the default implementations of internally used types into the container as singletons
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="typeRegistrations">Type registrations to register</param>
        protected override sealed void RegisterTypes(IKernel container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            foreach (TypeRegistration typeRegistration in typeRegistrations)
            {
                container.Bind(typeRegistration.RegistrationType).To(typeRegistration.ImplementationType).InSingletonScope();
            }
        }

        /// <summary>
        /// Bind the various collections into the container as singletons to later be resolved
        /// by IEnumerable{Type} constructor dependencies.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="collectionTypeRegistrations">Collection type registrations to register</param>
        protected override sealed void RegisterCollectionTypes(IKernel container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            foreach (CollectionTypeRegistration collectionTypeRegistration in collectionTypeRegistrations)
            {
                foreach (Type implementationType in collectionTypeRegistration.ImplementationTypes)
                {
                    container.Bind(collectionTypeRegistration.RegistrationType).To(implementationType).InSingletonScope();
                }
            }
        }

        /// <summary>
        /// Bind the given module types into the container
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="moduleRegistrationTypes">NancyModule types</param>
        protected override sealed void RegisterRequestContainerModules(IKernel container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            foreach (ModuleRegistration moduleRegistrationType in moduleRegistrationTypes)
            {
                container.Bind(typeof (NancyModule)).To(moduleRegistrationType.ModuleType).InSingletonScope().Named(moduleRegistrationType.ModuleKey);
            }
        }

        /// <summary>
        /// Bind the given instances into the container
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="instanceRegistrations">Instance registration types</param>
        protected override void RegisterInstances(IKernel container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            foreach (InstanceRegistration instanceRegistration in instanceRegistrations)
            {
                container.Bind(instanceRegistration.RegistrationType).ToConstant(instanceRegistration.Implementation);
            }
        }

        /// <summary>
        /// Creates a per request child/nested container
        /// </summary>
        /// <returns>Request container instance</returns>
        protected override sealed IKernel CreateRequestContainer()
        {
            return new ChildKernel(this.ApplicationContainer);
        }

        /// <summary>
        /// Retrieve all module instances from the container
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <returns>Collection of NancyModule instances</returns>
        protected override sealed IEnumerable<NancyModule> GetAllModules(IKernel container)
        {
            return container.GetAll<NancyModule>();
        }

        /// <summary>
        /// Retreive a specific module instance from the container by its key
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <param name="moduleKey">Module key of the module</param>
        /// <returns>NancyModule instance</returns>
        protected override sealed NancyModule GetModuleByKey(IKernel container, string moduleKey)
        {
            return container.Get<NancyModule>(moduleKey);
        }
    }
}