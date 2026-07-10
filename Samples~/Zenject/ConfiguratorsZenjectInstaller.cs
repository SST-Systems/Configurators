using Zenject;

namespace SST.Configurators.Samples.Zenject
{
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