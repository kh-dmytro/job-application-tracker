using CafeteriaApp.Common.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationTracker.Common.Commands;

public class CommandProcessor : ICommandProcessor
{
    private readonly IServiceProvider _serviceProvider;

    public CommandProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> Process<TResult>(ICommand<TResult> command)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        return await handler.HandleAsync((dynamic)command);
    }

    public async Task Process(ICommand command)
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        await handler.HandleAsync((dynamic)command);
    }
}