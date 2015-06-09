using NeuronDocumentSync.Infrastructure;
using NeuronDocumentSync.Infrastructure.Processors;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;
using NeuronDocumentSyncService.Infrastructure;
using Ninject.Modules;

namespace ConsoleTest
{
    public class TestBindings: NinjectModule
    {
        public override void Load()
        {
            Bind<INeuronLogger>().To<Logger>().InSingletonScope();
            Bind<IGeneralConfig>().To<GeneralConfig>().InSingletonScope();
            Bind<ISyncCore>().To<SyncCore>();
            Bind<FbDbConnectionConfig>().ToSelf();
            Bind<INeuronDocumentProcessor>().To<TestDocumentProcessor>();
            Bind<IConfigProcessor>().To<ConfigProcessor>().InSingletonScope();
            Bind<DocumentConverter>().ToSelf().InSingletonScope();
        }
    }
}