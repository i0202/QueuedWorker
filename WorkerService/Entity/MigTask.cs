using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService.Entity
{
    public class MigTask
    {
        public int TaskId { get; set; }
        public int RequestId { get; set; }
        public DateTime Createddate { get; set; }
        public DateTime Modifieddate { get; set; }
        public string WorkToDo { get; set; }
        public string TaskStatus { get; set; }
        public int PriorityOrder { get; set; }

        public override string ToString()
        {
            return $"TaskId :{TaskId} ,RequestId : {RequestId},Createddate : {Createddate},Modifieddate : {Modifieddate} ,WorkToDo: {WorkToDo},TaskStatus:{TaskStatus},PriorityOrder:{PriorityOrder}";
        }

    }
}
