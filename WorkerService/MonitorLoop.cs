using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkerService.Entity;

namespace WorkerService
{
    public class MonitorLoop
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<MonitorLoop> _logger;
        private readonly CancellationToken _cancellationToken;
        //to be taken from env config
        private readonly int _retryDBDelay= 60;
        private readonly int _taskProcessingSimulationDelay = 2;
        private readonly int _minQueueCountForFetch = 1;
        //to be taken from db table, sahir has to add each entry in table to make this run.
        private readonly DateTime windowStartTime = DateTime.Now.AddMinutes(0);
        private readonly DateTime windowEndTime = DateTime.Now.AddMinutes(120);

        public MonitorLoop(
            IBackgroundTaskQueue taskQueue,
            ILogger<MonitorLoop> logger,
            IHostApplicationLifetime applicationLifetime)
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        public void StartMonitorLoop()
        {
            _logger.LogInformation($"{nameof(MonitorAsync)} loop is starting. Checking for db entries for tasks...");
            

            // Run a console user input loop in a background thread
            Task.Run(async () => await MonitorAsync());
        }

        private async ValueTask MonitorAsync()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                /*var keyStroke = Console.ReadKey();
                if (keyStroke.Key == ConsoleKey.W)
                {
                    // Enqueue a background work item
                    await _taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItemAsync);
                }*/

                try
                {
                    //Getdata only if queue free queue space available.
                    if (isDBCallAllowed(_taskQueue.getCurrentCount())) {
                        BusinessLogic businesslogic = new BusinessLogic();
                        List<MigTask> MigTasks = businesslogic.getdata(5);
                        

                        _logger.LogInformation("Queued {count} tasks. New Tasks: {tasks}", MigTasks.Count, MigTasks);

                        //Loop and each async task to queue
                        foreach (var task in MigTasks)
                        {
                            TasksDetails t1 = new TasksDetails(_cancellationToken, task);
                            
                            await _taskQueue.QueueBackgroundWorkItemAsync(t1);
                        }
                    }
                        
                    await Task.Delay(TimeSpan.FromSeconds(_retryDBDelay), _cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if the Delay is cancelled
                }
            }
        }

        /*private async ValueTask BuildWorkItemAsync(TasksDetails tasksDetails)
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

        }*/

        private bool isDBCallAllowed(int currentQueueCount)
        {
            bool isAllowed = true;
            //recheck this logic
            if (currentQueueCount > _minQueueCountForFetch) {
                isAllowed = false;
            }
            //bad logic

            DateTime currentime = DateTime.Now;
            if (!(windowStartTime <= currentime && currentime <= windowEndTime))
            {
                isAllowed = false;
                _logger.LogInformation("Time is out of allowed range");
            }


            return isAllowed;
        }
    }
}
