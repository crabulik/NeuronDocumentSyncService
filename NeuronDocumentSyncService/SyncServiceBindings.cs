using System.ServiceProcess;
using NeuronDocumentSync.Infrastructure;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;
using NeuronDocumentSyncService.Infrastructure;
using Ninject.Modules;

namespace NeuronDocumentSyncService
{
    public class SyncServiceBindings: NinjectModule
    {
        public override void Load()
        {
            Bind<IGeneralConfig>().To<GeneralConfig>().InSingletonScope();
            Bind<INeuronLogger>().To<Logger>().InSingletonScope();
            Bind<ServiceBase>().To<DocumentSyncService>().InSingletonScope();
            Bind<IServiceManager>().To<ServiceManager>();
            Bind<ISyncCore>().To<SyncCore>();
            Bind<IConfigProcessor>().To<ConfigProcessor>().InSingletonScope();
            Bind<FbDbConnectionConfig>().ToSelf();
        }
    }
}