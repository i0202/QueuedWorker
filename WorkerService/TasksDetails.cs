using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WorkerService.Entity;

namespace WorkerService
{
    public class TasksDetails
    {
        public TasksDetails(CancellationToken _CancellationToken, MigTask _MigTask)
        {
            CancellationToken = _CancellationToken;
            MigTask = _MigTask;
        }
        public CancellationToken CancellationToken { get; set; }

        public MigTask MigTask { get; set; }

        public override string ToString()
        {
            return $"CancellationToken:{CancellationToken},MigTask:{MigTask.ToString()}";
        }
    }
}
