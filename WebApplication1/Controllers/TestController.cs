
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Text.Json;

//for versions read
//https://briancaos.wordpress.com/2022/10/14/c-net-swagger-api-versioning-show-versions-in-your-swagger-page/

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly IStatisticsInfo _statisticsInfo;

        private static readonly object _padlock = new();
        private static readonly Random rnd = new();

        public TestController(ILogger<TestController> logger, IStatisticsInfo statisticsInfo)
        {
            _logger = logger;

            _statisticsInfo = statisticsInfo;

            lock (_padlock)
            {
                _statisticsInfo.InstancesCount++;
                _statisticsInfo.InstancesActives++;
            }

            LogWrite("New {0}: Total:{1} Active:{2}", nameof(TestController), _statisticsInfo.InstancesCount, _statisticsInfo.InstancesActives);
        }

        ~TestController()
        {
            lock (_padlock)
            {
                _statisticsInfo.InstancesActives--;
            }

            LogWrite("DEStructor: Total:{0} Active:{1}", _statisticsInfo.InstancesCount, _statisticsInfo.InstancesActives);
        }



        [HttpGet(template: "GetExample")]
        //public string GetExampleAsync()             //only return 200 OK
        //public async Task<string> GetExampleAsync() //only return 200 OK
        public async Task<ActionResult<string>> GetExampleAsync() //https://code-maze.com/aspnetcore-web-api-return-types/
        {
            lock (_padlock) 
            {
                _statisticsInfo.MethodActives++;
                _statisticsInfo.MethodsCount++;
            }

            DateTime dt1 = DateTime.UtcNow;
            string now1 = dt1.ToString("yyy/MM/ss hh:mm:ss:fff");


            int delay = rnd.Next(1, 10) * 1000;
            await Task.Delay(delay); //operation


            DateTime dt2 = DateTime.UtcNow;
            string now2 = dt2.ToString("yyy/MM/ss hh:mm:ss:fff");
            string timeSpan = (dt2 - dt1).ToString();

            var resp = new
            {
                GetExample = "GetExample",
                Started = now1,
                Ended = now2,
                TimeSpan = timeSpan,
                Delay = delay,
                TotalInstances = _statisticsInfo.InstancesCount,
                ActiveInstances = _statisticsInfo.InstancesActives,
                MethodsCount = _statisticsInfo.MethodsCount,
                MethodActives = _statisticsInfo.MethodActives
            };

            LogWrite("GetExample: Started:{0}, Ended:{1}, TimeSpan:{2}, Delay:{3}, TotalInstances:{4}, ActiveInstances:{5}, MethodsCount:{6}, MethodActives:{7}",
              now1, now2, timeSpan, delay, _statisticsInfo.InstancesCount, _statisticsInfo.InstancesActives, _statisticsInfo.MethodsCount, _statisticsInfo.MethodActives);

            lock (_padlock)
            {
                _statisticsInfo.MethodActives--;
            }

            //Is good or bad ?!?!??!
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            var options = new JsonSerializerOptions { WriteIndented = true  };
            return JsonSerializer.Serialize(resp, options);
            //return Ok(msg);//if return is an IActionResult
            //return BadRequest("Name should be between 3 and 30 characters.");
        }

        private void LogWrite(LogLevel logLevel, string? msg, params object?[] args)
        {
            _logger?.Log(logLevel, msg, args);
        }

        private void LogWrite(string? msg, params object?[] args)
        {
            LogWrite(LogLevel.Information, msg, args);
        }

    }
}

//lock (_padlock)
//{ }

//OR

// Mutex mutex = new Mutex();
//if (mutex.WaitOne())
//{
//    //....
//    mutex.ReleaseMutex();
//}


//Mutex mutex = new Mutex();
//if (mutex.WaitOne(5000)) //espera 5 segundos se nao acabar em 5 segundo sai por false
//{
//   
//    try { }
//          ....
//    catch { }
//    finally { mutex.ReleaseMutex(); }
//}
//else
//    Console.WriteLine("Mutex has not been released in 5 seconds");
//}
