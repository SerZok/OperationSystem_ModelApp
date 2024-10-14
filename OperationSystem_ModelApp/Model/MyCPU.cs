using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace OperationSystem_ModelApp.Model
{
    internal class MyCPU
    {
        public MyOperationSystem myOS;
        /// <summary>
        /// Слово состояния (счетчик команд)
        /// </summary>
        public int PSW;
        private readonly object _lock = new object();

        public MyCPU(MyOperationSystem MyOS)
        {
            myOS = MyOS;
        }
        async public void Execute(MyProcess proc)
        {
            //Выполнять 1 квант (Kavnt-=takt)
            if (proc.State != ProcessState.Completed) //Если не завершена задача
            {
                //Запуск задачи
                proc.State = ProcessState.StartTask;
                await Task.Delay(myOS.ConvertTaktToMillisec(myOS.T_next));

                var kvant = MyOperationSystem.Kvant;
                //Выполнение команд
                while (proc.Commands.Any())
                {
                    var fCommand = proc.Commands.First();
                    //Если комманда IO
                    if (fCommand.TypeCmd.nameTypeCommand == NameTypeCommand.IO)
                    {
                        proc.State = ProcessState.Init_IO;
                        myOS.IOList.Add(proc.Id);
                        await Task.Delay(myOS.ConvertTaktToMillisec(myOS.T_IntiIO)); // Инициализация IO
                        new Thread(() => InOut()).Start();
                        myOS.cpuState = CpuState.Waiting;
                        break;
                    }
                    else //Если команда не IO
                    {
                        proc.State = ProcessState.Running;
                        //await Task.Delay(ConvertTaktToMillisec(fCommand.TypeCmd.timeTypeCommand));
                        await proc.DoTask(fCommand, myOS.Speed);
                        proc.Commands.Remove(fCommand);
                    }
                }

                if (!proc.Commands.Any()) //Нет команд
                {
                    myOS.RemoveProcess(proc);
                    myOS.cpuState = CpuState.Waiting;
                    myOS.CompetedTasks++;
                    //break;
                }
            }
            else //Задача завершена
            {
                myOS.RemoveProcess(proc);
                myOS.cpuState = CpuState.Waiting;
                //break;
            }


        }
        public void InOut()
        {
            var id = myOS.IOList.First();
            var proc = myOS._Processes.FirstOrDefault(x => x.Id == id);
            if (proc != null)
            {
                myOS.IOList.Remove(id);
                proc.State = ProcessState.InputOutput;

                var fCommand = proc.Commands.First();
                proc.DoTask(fCommand, myOS.Speed, true);
                proc.Commands.Remove(fCommand);

                proc.State = ProcessState.End_IO;
                Thread.Sleep(myOS.ConvertTaktToMillisec(myOS.T_IntrIO));

                if (proc.Commands.Any())
                    proc.State = ProcessState.Ready;
                else
                    proc.State = ProcessState.Completed;
            }
        }
    }
}
