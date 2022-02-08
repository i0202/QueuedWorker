using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace WorkerService
{
    public sealed class QueuedHostedService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<QueuedHostedService> _logger;
        private readonly int workerCount = 2;
        private readonly int _taskProcessingSimulationDelay = 1;

        public QueuedHostedService(
            IBackgroundTaskQueue taskQueue,
            ILogger<QueuedHostedService> logger) =>
            (_taskQueue, _logger) = (taskQueue, logger);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(QueuedHostedService)} is running.{Environment.NewLine}");

            TasksDetails tasksDetails = new TasksDetails(stoppingToken, new Entity.MigTask());
            return ProcessTaskQueueAsync(tasksDetails);
        }

        private async Task ProcessTaskQueueAsync(TasksDetails tasksDetails)
        {
            int i = 0;
            List<Task> parallelTasks = new List<Task>();
            do
            {
                parallelTasks.Add(AsyncWorker(tasksDetails, "Worker "+i));
                i++;

            } while (i < workerCount);
            //not sure if this is good idea
            Task.WaitAll(parallelTasks.ToArray());
        }

        private async Task AsyncWorker(TasksDetails tasksDetails, String workerName)
        {
            while (!tasksDetails.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine("workerName: " + workerName + " looking for a task.");
                    TasksDetails workItem =
                        await _taskQueue.DequeueAsync(tasksDetails);
                    Console.WriteLine("workerName: " + workerName + " starting to process a task.");

                    await doWork(workItem);
                    //problem here, taskDetails are not available here
                    Console.WriteLine("workerName: " + workerName + " finished a task.");
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken was signaled
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing task work item.");
                }
            }

        }

        private async ValueTask doWork(TasksDetails tasksDetails)
       {
           //Introduce task processing delay
           try
           {
               Console.WriteLine("Processing task:"+ tasksDetails);
               await Task.Delay(TimeSpan.FromSeconds(_taskProcessingSimulationDelay), tasksDetails.CancellationToken);
               BusinessLogic busineslogic = new BusinessLogic();
               busineslogic.UpdateTaskstatus(tasksDetails.MigTask.TaskId.ToString(), "completed");
           }
           catch (OperationCanceledException)
           {
               // Prevent throwing if the Delay is cancelled
           }

           _logger.LogInformation("Task processed successfully.");

       }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(QueuedHostedService)} is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
