using Serilog;
using Serilog.Events;
using System;
using System.Net.Http.Headers;

namespace TestWebApi
{
    //Visual Studio - run multi programs
    //select solution
    //right click -> Properties
    //Common Properties / Startup Projects
    //select projects to start at same time

    internal class Program
    {
        public static void Main()
        //public static async Task Main()
        {
            ConfigLogger("TestWebApi.log");

            LogWrite($"App Started....Press enter");
            LogWrite($"============================");

            Console.ReadKey();

            // Process - INI

            int totalLoops = 10;
            int totalTasks = 25;

            Task task = ProcessAsync(totalLoops, totalTasks);
            task.Wait();

            // Process - END

            LogWrite($"============================");
            LogWrite($"App Finished....Press enter");
            Console.ReadKey();
        }

        private static async Task ProcessAsync(int totalLoops, int totalTasks)
        {
            LogWrite($"All Tasks Started.");
            DateTime dtt1 = DateTime.Now;


            //task1 - It is just to demonstrate that the tasks are being performed at the same time!!!
            Task task1 = Task.Run(async () =>
            {
                LogWrite("Task Counter Started...");
                DateTime dtm1 = DateTime.Now;

                for (int i = 0; i < 20; i++)
                {
                    DateTime dt1 = DateTime.Now;

                    await Task.Delay(5000);  //note: Why do you pause just with 'Task.Run' ?

                    DateTime dt2 = DateTime.Now;
                    TimeSpan ts = dt1.Subtract(dt2);
                    LogWrite($"Task Counter={i},  Duration={ts}"); 

                }

                DateTime dtm2 = DateTime.Now;
                TimeSpan tsm = dtm1.Subtract(dtm2);
                LogWrite($"Task Counter Complete,  Duration={tsm}");
            });


            //task2
            Task task2 = Task.Run(async () => 
            {
                LogWrite($"Task Loops API Started...");
                DateTime dtm1 = DateTime.Now;

                for (int i = 0; i < totalLoops; i++)
                {
                    LogWrite($"Loop Start: {i} of {totalLoops}");
                    DateTime dt1 = DateTime.Now;

                    //await Task.Delay(2000);  //note: Why do you pause just with 'Task.Run' ?
                    await StartTaskArrayAsync(totalTasks, totalLoops, i);

                    DateTime dt2 = DateTime.Now;
                    TimeSpan ts = dt1.Subtract(dt2);
                    LogWrite($"Task API={i},  Duration={ts}");
                }

                DateTime dtm2 = DateTime.Now;
                TimeSpan tsm = dtm1.Subtract(dtm2);
                LogWrite($"Task Loops API complete,  Duration={tsm}");
            });


            LogWrite($"Wait for All Tasks to complete.");

            await Task.WhenAll(task1, task2); //Version1 OK
            //or
            //await task1;
            //await task2;



            DateTime dtt2 = DateTime.Now;
            TimeSpan tst = dtt1.Subtract(dtt2);
            LogWrite($"All Tasks Completed. Duration={tst}");
        }


        private static async Task StartTaskArrayAsync(int totalTasks, int totalLoops, int currentLoop)
        {
            LogWrite($"Task Array Started with {totalTasks} elements. Loop={currentLoop} of {totalLoops}");
            DateTime dtt1 = DateTime.Now;

            Task[] tasks = new Task[totalTasks];

            for (int i = 0; i < totalTasks; i++)
            {
                int j = i; //NOTE: C# lambdas capture a reference to the variable, not the value of the variable.
                int k = i + (totalLoops * currentLoop);
                //tasks[i] = new Task(async () => await Action(j, k) );
                tasks[i] = new Task(() => ActionCallApi(j, k));
            }

            foreach (Task task in tasks)
                task.Start();

            //Task.WaitAll(tasks);
            await Task.WhenAll(tasks);


            DateTime dtt2 = DateTime.Now;
            TimeSpan tst = dtt1.Subtract(dtt2);
            LogWrite($"Task Array Completed, Duration={tst}, Loop={currentLoop} of {totalLoops}");
        }
        
        private static string ActionCallApi(int id, int total)
        {
            LogWrite($"ActionCallApi Started, id={id} of {total}.");
            DateTime dtt1 = DateTime.Now;


            //curl - X 'GET' 'http://localhost:5029/api/Test/GetExample' -H 'accept: text/plain'
            var url = $"http://localhost:5029/api/Test/GetExample";
                      
            var parameters = ""; // "?query={query}&apiKey={Consts.SpoonacularKey}&number=5";


            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //HttpResponseMessage response = await client.GetAsync(parameters).ConfigureAwait(true);
            Task<HttpResponseMessage> task = client.GetAsync(parameters);
            task.Wait();

            HttpResponseMessage response = task.Result;

            string jsonString = "KO";
            if (response.IsSuccessStatusCode)
            {
                Task<string> taskString = response.Content.ReadAsStringAsync();
                taskString.Wait();

                jsonString = taskString.Result;

                //var recipeList = JsonConvert.DeserializeObject<RecipeList>(jsonString);
                //if (recipeList != null)
                //{
                //    recipes.AddRange(recipeList.Recipes);
                //}
            }
            else
                LogWrite($"ActionCallApi ERROR, id={id} :{response.StatusCode}");

            //LogWrite($"Action Return data={jsonString}");

            DateTime dtt2 = DateTime.Now;
            TimeSpan tst = dtt1.Subtract(dtt2);
            LogWrite($"ActionCallApi Completed, id={id} of {total}, Duration={tst}");

            //response.Dispose();
            //client.Dispose();
            //client = null;

            return jsonString;
        }

        private static void ConfigLogger(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            using var log = new LoggerConfiguration()
                .WriteTo.File(fileName)//, restrictedToMinimumLevel: LogEventLevel.Debug, rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger = log;
        }
        private static void LogWrite(string msg)
        {
            Log.Information(msg); //Serilog
        }
    }
}
