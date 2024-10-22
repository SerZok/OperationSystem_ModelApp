using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OperationSystem_ModelApp.Model
{
    internal class TaskManager
    {
        public ObservableCollection<MyProcess> _Processes;

        /// <summary>
        /// Планировщик
        /// </summary>
        public async Task Planner()
        {
            while (_Processes.Any())
            {
                //Загрузка задачи
                await Task.Delay(MyOperationSystem.ConvertTaktToMillisec(T_Load, 900));

                for (int i = 0; i < _Processes.Count; i++)
                {
                    myCPU.cpuState = CpuState.Working;
                    var proc = _Processes[i];
                    bool isIO = (proc.State == ProcessState.InputOutput || proc.State == ProcessState.Init_IO || proc.State == ProcessState.End_IO);

                    //Если надо удалить процесс
                    if (proc.needDelete && !isIO)
                    {
                        proc.State = ProcessState.Completed;
                        _Processes.Remove(proc);
                        _ram_ost += proc.Ram;
                        break;
                    }

                    if (isIO) //Процесс занят IO
                    {
                        myCPU.cpuState = CpuState.Waiting;
                        continue;
                    }
                    else
                    {
                        await myCPU.Execute(proc);

                        // Проверка, завершил ли процесс выполнение всех команд
                        if (proc.State == ProcessState.Paused)
                        {
                            MessageBox.Show("Квант закончился!");
                            proc.PSW = proc.CurrentCommandIndex;
                            // Возвращаем процесс в список готовых задач
                            proc.State = ProcessState.Ready;
                            // ЦП уходит в ожидание после паузы
                            myCPU.cpuState = CpuState.Waiting;
                        }
                        else if (proc.State == ProcessState.Completed)
                        {
                            // Процесс завершен, его больше не нужно планировать
                            _Processes.Remove(proc);
                            _ram_ost += proc.Ram;
                            myCPU.cpuState = CpuState.Waiting;
                        }

                        // Прерываем цикл после обработки одного процесса
                        break;
                    }
                }
                myCPU.cpuState = CpuState.Waiting;

            }
        }

    }

}
