
namespace CafeteriaApp.Common.Commands
{
    public interface ICommand<TResult>
    {
    }

    public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
    {
        Task<TResult> HandleAsync(TCommand command);
    }

    public interface ICommand
    {
    }

    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command);
    }
}