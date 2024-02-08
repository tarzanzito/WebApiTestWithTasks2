using Serilog;
using System;
using System.Net.Http.Headers;

namespace TestWebApi
{
    internal class ProgramSimple
    {
        public static async Task MainZ()
        {
            Console.WriteLine("App Started....");

            //await TwoProcessesAsync1();
            await TwoProcessesAsync2();

            Console.WriteLine("App Finished....");
        }

        private static async Task TwoProcessesAsync1()
        {
             Task task1 = Task.Run(async () => //- REF1
             {
                for (int i = 0; i < 15; i++)
                {
                    Console.WriteLine("Task 1 INI - iteration {0}", i);
                    await Task.Delay(5000);  //faz a pausa apenas com  Task.Run
                    Console.WriteLine("Task 1 END - iteration {0}", i);

                }
                Console.WriteLine("Task 1 complete");
            });

            /////////////////////////////////////////////////////////////////////////

            Task task2 = Task.Run(async () => //- REF1
            {
                for (int i = 0; i < 10; i++)
                {
                     Console.WriteLine("Task 2 INI - iteration {0}", i);

                    int j = i;
                    await Action(j); 

                    Console.WriteLine("Task 2 END - iteration {0}", i);
                }

                Console.WriteLine("Task 2 complete");
            });

            Console.WriteLine("Waiting for tasks to complete.");

            //await Task.WhenAll(task1, task2); //Version1 OK
            //or
            await task1;
            await task2;

            Console.WriteLine("tasks complete.");
        }

        private static async Task TwoProcessesAsync2()
        {
            Task task1 = new Task(async () =>
            {
                for (int i = 0; i < 15; i++)
                {
                    Console.WriteLine("Task 1 INI - iteration {0}", i);
                    await Task.Delay(5000);  //do not make de delay !!!!
                    Console.WriteLine("Task 1 END - iteration {0}", i);

                }
                Console.WriteLine("Task 1 complete");
            });

            /////////////////////////////////////////////////////////////////////////
            
            Task task2 = new Task(async () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine("Task 2 INI - iteration {0}", i);

                    int j = i;
                    await Action(j); //do not make de delay !!!!

                    Console.WriteLine("Task 2 END - iteration {0}", i);
                }

                Console.WriteLine("Task 2 complete");
            });


            task1.Start();
            task2.Start();

            Console.WriteLine("Waiting for tasks to complete.");

            //await Task.WhenAll(task1, task2); //Version1 OK
            //or
            await task1; 
            await task2;

            Console.WriteLine("tasks complete.");

        }

        private static async Task Action(int id)
        {
            Console.WriteLine("Action INI:" + id.ToString());

            await Task.Delay(5000); //only make pause with "Task.Run(" - REF1

            Console.WriteLine("Action END:" + id.ToString());
        }
    }
}
