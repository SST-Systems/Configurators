using Zenject;

namespace SST.Configurators.Samples.ZenjectForConfigurators
{
    /// <summary>
    /// Zenject <see cref="MonoInstaller"/> that wires Configurators into a DI container instead of the
    /// ServiceLocator + Bootstrap approach. Binds <see cref="IHandlerFactory"/> (backed by
    /// <see cref="ZenjectHandlerFactory"/>) and the four managers — instruction, modification, condition and
    /// extension — as singletons so the whole system is resolved through the container.
    /// </summary>
    public class ConfiguratorsZenjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IHandlerFactory>().To<ZenjectHandlerFactory>().AsSingle();
            Container.Bind<IInstructionManager>().To<InstructionManager>().AsSingle();
            Container.Bind<IModificationManager>().To<ModificationManager>().AsSingle();
            Container.Bind<IConditionManager>().To<ConditionManager>().AsSingle();
            Container.Bind<IExtensionManager>().To<ExtensionManager>().AsSingle();
        }
    }
}