using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace OperationSystem_ModelApp.Model
{
    public enum CpuState { Waiting, Working }
    internal class MyCPU
    {
        public MyOperationSystem myOS;
        /// <summary>
        /// Слово состояния (счетчик команд)
        /// </summary>
        public int PSW;
        private readonly object _lock = new object();
        public CpuState cpuState;

        public MyCPU(MyOperationSystem MyOS)
        {
            myOS = MyOS;
        }
        async public Task Execute(MyProcess proc)
        {
            if (proc.State != ProcessState.Completed) // Если задача не завершена
            {
                myOS.ChangeProcState(proc , ProcessState.StartTask);
                await Task.Delay(myOS.ConvertTaktToMillisec(myOS.T_next, myOS.Speed));

                // Счетчик оставшегося времени на выполнение (в тактах)
                var remainingKvant = MyOperationSystem.Kvant;

                while (proc.Commands.Any() && remainingKvant > 0) // Выполняем, пока есть команды и квант не исчерпан
                {

                    // Возвращение к работе после паузы
                    cpuState = CpuState.Working;
                    myOS.ChangeProcState(proc, ProcessState.StartTask);

                    var fCommand = proc.Commands.First();
                    // Если команда IO
                    if (fCommand.TypeCmd.nameTypeCommand == NameTypeCommand.IO)
                    {
                        //proc.State = ProcessState.Init_IO;
                        myOS.ChangeProcState(proc, ProcessState.Init_IO);
                        myOS.IOList.Add(proc.Id);
                        await Task.Delay(myOS.ConvertTaktToMillisec(myOS.T_IntiIO, myOS.Speed)); // Инициализация IO
                        new Thread(() => InOut()).Start();

                        ChangeCpuState (this, CpuState.Waiting);

                        break;  // Прерываем цикл при инициализации IO
                    }
                    else  // Если команда не IO
                    {
                        //proc.State = ProcessState.Running;
                        myOS.ChangeProcState(proc, ProcessState.Running);
                        // Выполняем команду с учетом скорости (в тактах)
                        var timeTaken = await proc.DoTask(fCommand, myOS.Speed, remainingKvant);

                        // Уменьшаем оставшийся квант
                        remainingKvant -= timeTaken;
                        proc.Commands.Remove(fCommand);

                        // Если квант закончился, останавливаем процесс
                        if (remainingKvant <= 0)
                        {
                            //MessageBox.Show("Квант закончился!");
                            //proc.State = ProcessState.Paused;
                            myOS.ChangeProcState(proc, ProcessState.Paused);

                            //cpuState = CpuState.Waiting;
                            ChangeCpuState(this, CpuState.Waiting);
                            return;
                        }
                    }
                }

                if (!proc.Commands.Any())  // Все команды выполнены
                {
                    myOS.ChangeProcState(proc, ProcessState.Completed);
                    myOS.RemoveProcess(proc);  // Удаляем завершенный процесс

                    //cpuState = CpuState.Waiting;
                    ChangeCpuState(this, CpuState.Waiting);
                    myOS.CompetedTasks++;
                }
            }
            else  // Если задача уже завершена
            {
                myOS.RemoveProcess(proc);
                ChangeCpuState(this, CpuState.Waiting);
            }
        }

        /// <summary>
        /// Метод изменения состояния процессора
        /// </summary>
        /// <param name="cpu">процессор, у которого надо изменить состояние</param>
        /// <param name="state">состояние, которое надо задать</param>
        public void ChangeCpuState(MyCPU cpu, CpuState state)
        {
            if (cpu.cpuState != state)
            {
                cpu.cpuState = state;
            }
        }

        public void InOut()
        {
            var id = myOS.IOList.First();
            var proc = myOS._Processes.FirstOrDefault(x => x.Id == id);
            if (proc != null)
            {
                myOS.IOList.Remove(id);

                //proc.State = ProcessState.InputOutput;
                myOS.ChangeProcState(proc, ProcessState.InputOutput);

                var fCommand = proc.Commands.First();

                proc.ProcTakt = fCommand.TypeCmd.timeTypeCommand;
                while (proc.ProcTakt > 0)
                {
                    proc.ProcTakt--;
                    Thread.Sleep(myOS.Speed);
                }

                proc.Commands.Remove(fCommand);

                //proc.State = ProcessState.End_IO;
                myOS.ChangeProcState(proc, ProcessState.End_IO);

                Thread.Sleep(myOS.ConvertTaktToMillisec(myOS.T_IntrIO, myOS.Speed));

                if (proc.Commands.Any())
                    proc.State = ProcessState.Ready;
                else
                    proc.State = ProcessState.Completed;
            }
        }
    }
}
