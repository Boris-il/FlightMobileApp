using System.Threading.Tasks;

namespace FlightMobileApp.Models
{
    public class AsyncCommand
    {

        public Command Command { get; private set; }
        public Task<Result> Task { get => Completion.Task; }
        public TaskCompletionSource<Result> Completion { get; private set; }

        public AsyncCommand(Command input)
        {
            this.Command = input;
            this.Completion = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
