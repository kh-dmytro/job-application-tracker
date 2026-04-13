namespace CafeteriaApp.Common.Commands
{
    public interface ICommandProcessor
    {
        Task<TResult> Process<TResult>(ICommand<TResult> command);

        Task Process(ICommand command);
    }
}