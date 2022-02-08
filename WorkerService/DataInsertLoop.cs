using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService
{
    public class DataInsertLoop
    {
        private readonly ILogger<DataInsertLoop> _logger;
        private readonly CancellationToken _cancellationToken;


        //to be taken from env config
        private readonly int _insertDBDelay = 60;
        private readonly int _recInsertsPerBatch=100;
        //to be taken from db table, sahir has to add each entry in table to make this run.
        private readonly DateTime windowStartTime = DateTime.Now.AddMinutes(0);
        private readonly DateTime windowEndTime = DateTime.Now.AddMinutes(10);

        public DataInsertLoop(
           ILogger<DataInsertLoop> logger,
           IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            _cancellationToken = applicationLifetime.ApplicationStopping;

        }

        public void StartInsertLoop()
        {
            _logger.LogInformation($"{nameof(InsertAsync)} loop is starting. Checking for db entries for new tasks...");


            // Run a insert loop in a background thread
            Task.Run(async () => await InsertAsync());
        }

        private async ValueTask InsertAsync()
        {
            int taskId = 528;
            BusinessLogic bs = new BusinessLogic();
            while (!_cancellationToken.IsCancellationRequested)
            {
                /*var keyStroke = Console.ReadKey();
                if (keyStroke.Key == ConsoleKey.W)
                {
                    // Enqueue a background work item
                    await _taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItemAsync);
                }*/
                int requestId = 10005;
                try
                {
                    //Getdata only if queue free queue space available.
                    if (isDBCallAllowed())
                    {
                        //List<TasksDetails> tasks =new List<TasksDetails>();
                        int recCount = 0;
                        do
                        {
                            Entity.MigTask migTask = new Entity.MigTask();
                            migTask.RequestId=requestId;
                            migTask.TaskId = taskId;
                           

                            bs.InsertTask(migTask);
                            taskId++;
                            recCount++;
                        } while (recCount < _recInsertsPerBatch);
                       


                        _logger.LogInformation("Inserted {count} tasks.", recCount);
                    }
                    requestId++;

                    await Task.Delay(TimeSpan.FromSeconds(_insertDBDelay), _cancellationToken);
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

        private bool isDBCallAllowed()
        {
            bool isAllowed = true;
       
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
