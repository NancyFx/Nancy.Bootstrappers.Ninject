using IKernel = Ninject.IKernel;
using ResolutionExtensions = Ninject.ResolutionExtensions;
using StandardKernel = Ninject.StandardKernel;

namespace Nancy.Bootstrappers.Ninject
{  
  using System;
  using System.Collections.Generic;
  using Nancy.Bootstrapper;  

  /// <summary>
  /// Ninject IoC Container Bootstrapper  
  /// </summary>
  public class NinjectNancyBootStrapper : NancyBootstrapperBase<IKernel>
  {
    /// <summary>
    /// Get all NancyModule implementation instances - should be multi-instance
    /// </summary>
    /// <param name="context">Current request context</param>
    /// <returns>IEnumerable of NancyModule</returns>
    public override sealed IEnumerable<NancyModule> GetAllModules(NancyContext context)
    {
      return ResolutionExtensions.GetAll<NancyModule>(this.ApplicationContainer);
    }

    /// <summary>
    /// Gets a specific, per-request, module instance from the key
    /// </summary>
    /// <param name="moduleKey">Module key of the module to retrieve</param>
    /// <param name="context">Current request context</param>
    /// <returns>NancyModule instance</returns>
    public override sealed NancyModule GetModuleByKey(string moduleKey, NancyContext context)
    {
      return ResolutionExtensions.Get<NancyModule>(this.ApplicationContainer, moduleKey);
    }

    /// <summary>
    /// Configures the container using AutoRegister followed by registration
    /// of default INancyModuleCatalog and IRouteResolver.
    /// </summary>
    /// <param name="container">Container instance</param>
    protected override void ConfigureApplicationContainer(IKernel container)
    {
      if (ResolutionExtensions.TryGet<INancyModuleCatalog>(container) == null)
      {
        container.Bind<INancyModuleCatalog>().ToConstant(this);
      }
    }

    /// <summary>
    /// Resolve INancyEngine
    /// </summary>
    /// <returns>INancyEngine implementation</returns>
    protected override sealed INancyEngine GetEngineInternal()
    {
      return ResolutionExtensions.Get<INancyEngine>(this.ApplicationContainer);
    }

    /// <summary>
    /// Get the moduleKey generator
    /// </summary>
    /// <returns>IModuleKeyGenerator instance</returns>
    protected override sealed IModuleKeyGenerator GetModuleKeyGenerator()
    {
      return ResolutionExtensions.Get<IModuleKeyGenerator>(this.ApplicationContainer);
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
    /// Register the bootstrapper's implemented types into the container.
    /// This is necessary so a user can pass in a populated container but not have
    /// to take the responsibility of registering things like INancyModuleCatalog manually.
    /// </summary>
    /// <param name="applicationContainer">Application container to register into</param>
    protected override sealed void RegisterBootstrapperTypes(IKernel applicationContainer)
    {
      applicationContainer.Bind<INancyModuleCatalog>().ToConstant(this);
    }

    /// <summary>
    /// Register the default implementations of internally used types into the container as singletons
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
    /// Register the various collections into the container as singletons to later be resolved
    /// by IEnumerable{Type} constructor dependencies.
    /// </summary>
    /// <param name="container">Container to register into</param>
    /// <param name="collectionTypeRegistrationsn">Collection type registrations to register</param>
    protected override sealed void RegisterCollectionTypes(IKernel container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrationsn)
    {
      foreach (CollectionTypeRegistration collectionTypeRegistration in collectionTypeRegistrationsn)
      {
        foreach (Type implementationType in collectionTypeRegistration.ImplementationTypes)
        {
          container.Bind(collectionTypeRegistration.RegistrationType).To(implementationType);
        }
      }
    }

    /// <summary>
    /// Register the given module types into the container
    /// </summary>
    /// <param name="container">Container to register into</param>
    /// <param name="moduleRegistrationTypes">NancyModule types</param>
    protected override sealed void RegisterModules(IKernel container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
    {
      foreach (ModuleRegistration registrationType in moduleRegistrationTypes)
      {
        container.Bind(typeof (NancyModule)).To(registrationType.ModuleType).InRequestScope().Named(registrationType.ModuleKey);
      }
    }

    /// <summary>
    /// Register the given instances into the container
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
  }
}