using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkerService.DB;
using WorkerService.Entity;

namespace WorkerService
{

    public class BusinessLogic
    {
        Database db = new Database();
        public List<MigTask> getdata(int recordlimit)
        {
            //use bind variables instead
            //2 step process must rollback any of 2 fails, use transactions
            string dbFetchQuery = "select top " + recordlimit + " * from Task where TaskStatus='Ready' order by PriorityOrder,Createddate";
            DataTable dt = db.GetData(dbFetchQuery);
            List<MigTask> MigTasks = new List<MigTask>();
            MigTasks = (from DataRow row in dt.Rows

                        select new MigTask
                        {
                            TaskId = Convert.ToInt32(row["TaskId"]),
                            RequestId = Convert.ToInt32(row["RequestId"]),
                            Createddate = Convert.ToDateTime(row["Createddate"]),
                            Modifieddate = Convert.ToDateTime(row["Modifieddate"]),
                            WorkToDo = row["WorkToDo"].ToString(),
                            PriorityOrder = Convert.ToInt32(row["PriorityOrder"])
                        }).ToList();

            //Shan:Use transactions and after getting the data update the TaskStatus field to "InQueue".
            //Shan:Must have task creation time stamp and also task lastmodified timestamp for each task.
            //Shan:Define task pick rate.
            string selectedtasks = String.Join(",", MigTasks.Select(x => x.TaskId));
            UpdateTaskstatus(selectedtasks, "Queued");
            return MigTasks;

        }
        public void UpdateTaskstatus(string TaskId, string TaskStatus)
        {
            string dbQuery;
            if (TaskStatus.ToLower() == "completed")
            {
                dbQuery = "update Task set TaskStatus='" + TaskStatus + "',Modifieddate='" + DateTime.Now.ToString() + "',NoofExection=NoofExection+1 where TaskId IN (" + TaskId + ") ";

            }
            else
            {
                dbQuery = "update Task set TaskStatus='" + TaskStatus + "',Modifieddate='" + DateTime.Now.ToString() + "' where TaskId IN (" + TaskId + ") ";

            }
            db.ExecuteData(dbQuery);

        }

        public void InsertTask(MigTask Task)
        {
            string dbQuery;

            dbQuery = "INSERT [dbo].[Task] ([TaskId], [RequestId], [Createddate], [Modifieddate], [WorkToDo], [TaskStatus], [PriorityOrder], [NoofExection]) " +
                "VALUES (" + Task.TaskId + ", " + Task.RequestId + ", CAST(N'2022-02-04T13:50:36.860' AS DateTime), CAST(N'2022-02-06T21:23:03.000' AS DateTime), N'Upload me', N'Ready', 999, 0)";
           
           
            db.ExecuteData(dbQuery);

        }

    }
}
