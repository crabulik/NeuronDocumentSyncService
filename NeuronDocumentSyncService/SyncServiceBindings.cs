using System.ServiceProcess;
using NeuronDocumentSync.Infrastructure;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;
using Ninject.Modules;

namespace NeuronDocumentSyncService
{
    public class SyncServiceBindings: NinjectModule
    {
        public override void Load()
        {
            var genConfig = new GeneralConfig();

            Bind<IGeneralConfig>().ToConstant(genConfig);
            Bind<INeuronLogger>().To<Logger>().InSingletonScope();
            Bind<ServiceBase>().To<DocumentSyncService>().InSingletonScope();
            Bind<IServiceManager>().To<ServiceManager>();
            Bind<ISyncCore>().To<SyncCore>();
        }
    }
}