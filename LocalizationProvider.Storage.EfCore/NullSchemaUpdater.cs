using DbLocalizationProvider.Abstractions;
using DbLocalizationProvider.Sync;

namespace LocalizationProvider.Storage.EfCore;

public class NullSchemaUpdater : ICommandHandler<UpdateSchema.Command>
{
    public void Execute(UpdateSchema.Command command)
    {
        //nop
    }
}