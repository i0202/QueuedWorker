using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WorkerService
{

    public class DefaultBackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<TasksDetails> _queue;

        public DefaultBackgroundTaskQueue(int capacity)
        {
            BoundedChannelOptions options = new(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<TasksDetails>(options);
        }

        public async ValueTask QueueBackgroundWorkItemAsync(TasksDetails workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            await _queue.Writer.WriteAsync(workItem);
        }

        public async ValueTask<TasksDetails> DequeueAsync(
            TasksDetails tasksDetails)
        {
            TasksDetails workItem =
                await _queue.Reader.ReadAsync(tasksDetails.CancellationToken);

            return workItem;
        }

        public int getCurrentCount()
        {
            int count = _queue.Reader.Count;
            //needs async? and check threadsafe
            Console.WriteLine("Current queue count is: "+ count);
            return count;
        }
    }
}