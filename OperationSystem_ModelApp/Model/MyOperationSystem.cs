using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Net.WebSockets;
using System.Net.NetworkInformation;


namespace OperationSystem_ModelApp.Model
{
    public enum CpuState { Waiting, Working }
    class MyOperationSystem
    {    /// <summary>
         /// Сколько тактов в 1 кванте;
         /// Число тактов моделирования, доступных процессу в состоянии «Активен»
         /// Нужно для Processes (Сколько будет выполняться задача) ?
         /// </summary>
        public static int Kvant;
        public int Takt { get; set; }
        private int _ram = 1024;
        private int _ram_ost;
        public int Ram {
            get => _ram; 
            set
            {
                var _ram_used = _ram - _ram_ost;
                _ram = value;
                Ram_ost = _ram - _ram_used;
            }
        }
        public int Ram_ost
        { 
            get => _ram_ost;
            set => _ram_ost=value; 
        }

        /// <summary>
        /// Такты для запуска задачи
        /// </summary>
        private int T_next;

        /// <summary>
        /// Затраты ОС на изменение состояния процесса	
        /// по обращению ко вводу(выводу) (в числе тактов)
        /// </summary>
        private int T_IntiIO;

        /// <summary>
        ///  Затраты ОС по обслуживанию сигнала окончания T_IntrIO
        ///  (прерывания) ввода(вывода) (в числе тактов)
        /// </summary>
        private int T_IntrIO;

        /// <summary>
        /// Число тактов на загрузку нового задания
        /// </summary>
        private int T_Load;

        /// <summary>
        /// Скорость работы ОС
        /// </summary>
        private int Speed; //скорость тиков

        public ObservableCollection<MyProcess> _Processes;
        public List<MyProcess> _listMyPros;

        public CpuState cpuState;

        private CancellationTokenSource _cancellationTokenSource; //Нужен для проверки ОЗУ
        private CancellationTokenSource _cancellationTokenSourceTasks; //Для проверки запуска заданий
        public MyOperationSystem()
        {
            _Processes = new ObservableCollection<MyProcess>();
            _listMyPros = new List<MyProcess>();
            _ram_ost = _ram;
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSourceTasks = new CancellationTokenSource();

            StartLauncTask(_cancellationTokenSourceTasks.Token);
            StartRamCheck(_cancellationTokenSource.Token);
            cpuState = CpuState.Waiting;

            Speed = 10;
            T_next = 10;
            T_IntiIO = 20;
            T_IntrIO = 30;
            T_Load = 10;
        }

        public void AddProcess()    //Добавляю процесс при нажитии на кнопку.
        {
            var proc = new MyProcess();
            if (_ram_ost >= proc.Ram)
            {
                _Processes.Add(proc);
                _ram_ost-= proc.Ram;
            }
            else
            {
                _listMyPros.Add(proc);
            }
        }
        public void RemoveProcess(MyProcess proc) //Убирает с ObservableCollection выбранный процесс
        {
            _Processes.Remove(proc);
            _ram_ost += proc.Ram;
        }
        public void CountTakt() //Счетчик тактов
        {
            while (true)
            {
                Takt++;
                Thread.Sleep(Speed);
            }
        }

        public int ConvertTaktToMillisec(int takt)
        {
            return takt * Speed;
        }
        public async void Generating(CancellationToken cancellationToken)   //Генерация заданий.
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _listMyPros.Add(new MyProcess());
                    await Task.Delay(1000);
                }
            }
            catch(TaskCanceledException e)
            {
                MessageBox.Show($"Generating error:(TaskCanceledException) {e}");
            }
        }
        private async void StartLauncTask(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested) {
                    await LauncTask();
                    await Task.Delay(100);
                }
            }
            catch (TaskCanceledException e)
            {
                MessageBox.Show($"Eroor:(TaskCanceledException in Task) {e}");
            }
        }

        //Запустить задание, если есть (Мб это планировщик)
        public async Task LauncTask()
        {
            while (_Processes.Any())
            {
                var proc = _Processes.First();
                if (proc.State != ProcessState.Completed)
                {
                    await Task.Delay(ConvertTaktToMillisec(T_Load));
                    cpuState = CpuState.Working;
                    await Task.Delay(ConvertTaktToMillisec(T_next));
                    proc.State = ProcessState.Running;
                    await Task.Delay(ConvertTaktToMillisec(T_IntiIO));
                    proc.State = ProcessState.Completed;
                    await Task.Delay(ConvertTaktToMillisec(T_Load));
                }
                else
                {
                    cpuState = CpuState.Waiting;
                    RemoveProcess(proc);
                    break;
                }
            }
        }

        //Старт проверки ОЗУ
        private async void StartRamCheck(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    CheckRam();
                    await Task.Delay(1);
                }
            }
            catch (TaskCanceledException e)
            {
                MessageBox.Show($"Eroor:(TaskCanceledException in RamCheck) {e}");
            }
        }

        //Если хватает свободной памяти для задачи,
        //то выгружаем из List и загружаем в ObservableCollection
        public void CheckRam()
        {
            if (_ram_ost > 0)
            {
                while (_listMyPros.Count > 0)
                {
                    var firstItem = _listMyPros.First();
                    if (_ram_ost >= firstItem.Ram)
                    {
                        _Processes.Add(firstItem);
                        _ram_ost -= firstItem.Ram;
                        _listMyPros.Remove(firstItem);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else if (_ram_ost < 0)
            {
                var item = _Processes.Last();
                if (item != null) //Если список выполняемых задач не пустой
                {
                    _listMyPros.Add(item);
                    _ram_ost += item.Ram;
                    _Processes.Remove(item);
                }
            }
        }

        //Прекратить  проверять ОЗУ (Может быть полезно в будущем)
        public void StopRamCheck()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
