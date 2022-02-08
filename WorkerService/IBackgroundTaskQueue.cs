using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService
{
    public interface IBackgroundTaskQueue
    {
        ValueTask QueueBackgroundWorkItemAsync(TasksDetails workItem);

        ValueTask<TasksDetails> DequeueAsync(
            TasksDetails tasksDetails);

        int getCurrentCount();
    }
}
