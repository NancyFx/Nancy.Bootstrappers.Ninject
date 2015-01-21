namespace Nancy.Bootstrappers.Ninject
{
    using System;
    using System.Collections.Generic;
    using Diagnostics;
    using Nancy.Bootstrapper;
    using global::Ninject;
    using global::Ninject.Extensions.ChildKernel;
    using global::Ninject.Infrastructure;

    /// <summary>
    /// Nancy bootstrapper for the Ninject container.
    /// </summary>
    public abstract class NinjectNancyBootstrapper : NancyBootstrapperWithRequestContainerBase<IKernel>
    {
        /// <summary>
        /// Gets the diagnostics for intialisation
        /// </summary>
        /// <returns>An <see cref="IDiagnostics"/> implementation</returns>
        protected override IDiagnostics GetDiagnostics()
        {
            return this.ApplicationContainer.Get<IDiagnostics>();
        }

        /// <summary>
        /// Gets all registered application startup tasks
        /// </summary>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> instance containing <see cref="IApplicationStartup"/> instances. </returns>
        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            return this.ApplicationContainer.GetAll<IApplicationStartup>();
        }

        /// <summary>
        /// Gets all registered request startup tasks
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> instance containing <see cref="IRequestStartup"/> instances.</returns>
        protected override IEnumerable<IRequestStartup> RegisterAndGetRequestStartupTasks(IKernel container, Type[] requestStartupTypes)
        {
            foreach (var requestStartupType in requestStartupTypes)
            {
                container.Bind(typeof(IRequestStartup)).To(requestStartupType).InSingletonScope();
            }
            
            return container.GetAll<IRequestStartup>();
        }

        /// <summary>
        /// Gets all registered application registration tasks
        /// </summary>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> instance containing <see cref="IRegistrations"/> instances.</returns>
        protected override IEnumerable<IRegistrations> GetRegistrationTasks()
        {
            return this.ApplicationContainer.GetAll<IRegistrations>();
        }

        /// <summary>
        /// Get INancyEngine
        /// </summary>
        /// <returns>An <see cref="INancyEngine"/> implementation</returns>
        protected override sealed INancyEngine GetEngineInternal()
        {
            return this.ApplicationContainer.Get<INancyEngine>();
        }

        /// <summary>
        /// Create a default, unconfigured, container
        /// </summary>
        /// <returns>Container instance</returns>
        protected override IKernel GetApplicationContainer()
        {
            return new StandardKernel(new[] { new FactoryModule() });
        }

        /// <summary>
        /// Bind the bootstrapper's implemented types into the container.
        /// This is necessary so a user can pass in a populated container but not have
        /// to take the responsibility of registering things like <see cref="INancyModuleCatalog"/> manually.
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
            foreach (var typeRegistration in typeRegistrations)
            {
                switch (typeRegistration.Lifetime)
                {
                    case Lifetime.Transient:
                        container.Bind(typeRegistration.RegistrationType).To(typeRegistration.ImplementationType).InTransientScope();
                        break;
                    case Lifetime.Singleton:
                        container.Bind(typeRegistration.RegistrationType).To(typeRegistration.ImplementationType).InSingletonScope();
                        break;
                    case Lifetime.PerRequest:
                        throw new InvalidOperationException("Unable to directly register a per request lifetime.");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
            foreach (var collectionTypeRegistration in collectionTypeRegistrations)
            {
                foreach (var implementationType in collectionTypeRegistration.ImplementationTypes)
                {
                    switch (collectionTypeRegistration.Lifetime)
                    {
                        case Lifetime.Transient:
                            container.Bind(collectionTypeRegistration.RegistrationType).To(implementationType).InTransientScope();
                            break;
                        case Lifetime.Singleton:
                            container.Bind(collectionTypeRegistration.RegistrationType).To(implementationType).InSingletonScope();
                            break;
                        case Lifetime.PerRequest:
                            throw new InvalidOperationException("Unable to directly register a per request lifetime.");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        /// <summary>
        /// Bind the given module types into the container
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="moduleRegistrationTypes"><see cref="INancyModule"/> types</param>
        protected override sealed void RegisterRequestContainerModules(IKernel container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            foreach (var moduleRegistrationType in moduleRegistrationTypes)
            {
                container.Bind(typeof(INancyModule)).To(moduleRegistrationType.ModuleType).Named(moduleRegistrationType.ModuleType.FullName);
            }
        }

        /// <summary>
        /// Bind the given instances into the container
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="instanceRegistrations">Instance registration types</param>
        protected override void RegisterInstances(IKernel container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            foreach (var instanceRegistration in instanceRegistrations)
            {
                container.Bind(instanceRegistration.RegistrationType).ToConstant(instanceRegistration.Implementation);
            }
        }

        /// <summary>
        /// Creates a per request child/nested container
        /// </summary>
        /// <param name="context">Current context</param>
        /// <returns>Request container instance</returns>
        protected override IKernel CreateRequestContainer(NancyContext context)
        {
            return new ChildKernel(this.ApplicationContainer, new NinjectSettings { DefaultScopeCallback = StandardScopeCallbacks.Singleton });
        }

        /// <summary>
        /// Retrieve all module instances from the container
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <returns>Collection of <see cref="INancyModule"/> instances</returns>
        protected override sealed IEnumerable<INancyModule> GetAllModules(IKernel container)
        {
            return container.GetAll<INancyModule>();
        }

        /// <summary>
        /// Retrieve a specific module instance from the container
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <param name="moduleType">Type of the module</param>
        /// <returns>An <see cref="INancyModule"/> instance</returns>
        protected override INancyModule GetModule(IKernel container, Type moduleType)
        {
            container.Bind<INancyModule>().To(moduleType);

            return container.Get(moduleType) as INancyModule;
        }
    }
}