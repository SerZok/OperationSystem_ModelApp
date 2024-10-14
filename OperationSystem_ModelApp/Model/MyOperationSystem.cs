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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;


namespace OperationSystem_ModelApp.Model
{
    public enum CpuState { Waiting, Working }
    class MyOperationSystem : INotifyPropertyChanged
    {    /// <summary>
         /// Сколько тактов в 1 кванте;
         /// Число тактов моделирования, доступных процессу в состоянии «Активен»
         /// Нужно для Processes (Сколько будет выполняться задача) ?
         /// </summary>
        public static int Kvant;
        public int Takt { get; set; }
        private int _ram = 1024;
        private int _ram_ost;
        public int Ram
        {
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
            set => _ram_ost = value;
        }

        private int _d_InOut;
        /// <summary>
        /// Процент команд ВВ в заданиях для генерации
        /// </summary>
        public int D_InOut
        {
            get => _d_InOut;
            set
            {
                _d_InOut = value;
                OnPropertyChanged("D_InOut");
            }
        }

        /// <summary>
        /// Такты для запуска задачи
        /// </summary>
        public int T_next
        {
            get => _t_next;
            set
            {
                if (_t_next != value)
                {
                    _t_next = value;
                    OnPropertyChanged("T_next");
                }
            }
        }
        private int _t_next;

        /// <summary>
        /// Затраты ОС на изменение состояния процесса	
        /// по обращению ко вводу(выводу) (в числе тактов)
        /// </summary>
        public int T_IntiIO
        {
            get => _t_intiIO;
            set
            {
                if (_t_intiIO != value)
                {
                    _t_intiIO = value;
                    OnPropertyChanged("T_IntiIO");
                }
            }
        }
        private int _t_intiIO;

        /// <summary>
        ///  Затраты ОС по обслуживанию сигнала окончания T_IntrIO
        ///  (прерывания) ввода(вывода) (в числе тактов)
        /// </summary>
        public int T_IntrIO
        {
            get => _t_intrIO;
            set
            {
                if (_t_intrIO != value)
                {
                    _t_intrIO = value;
                    OnPropertyChanged();
                }
            }
        }
        private int _t_intrIO;

        /// <summary>
        /// Число тактов на загрузку нового задания
        /// </summary>
        public int T_Load
        {
            get => _t_load;
            set
            {
                if (_t_load != value)
                {
                    _t_load = value;
                    OnPropertyChanged();
                }
            }
        }
        private int _t_load;

        /// <summary>
        /// Скорость работы ОС (скорость тактов)
        /// </summary>
        /// 
        public int Speed
        {
            get => _speed;
            set
            {
                if (_speed != value)
                {
                    _speed = value;
                    OnPropertyChanged("Speed");
                }

            }
        }
        private int _speed;

        private int _competedTasks;
        public int CompetedTasks
        {
            get => _competedTasks;
            set
            {
                _competedTasks = value;
                OnPropertyChanged("CompetedTasks");
            }
        }

        public ObservableCollection<MyProcess> _Processes;
        public ObservableCollection<MyProcess> _listMyPros;
        public List<int> IOList;

        public CpuState cpuState;

        private CancellationTokenSource _cancellationTokenSource; //Нужен для проверки ОЗУ
        private CancellationTokenSource _cancellationTokenSourceTasks; //Для проверки запуска заданий
        private readonly object _lock = new object();

        public MyCPU myCPU;
        public MyOperationSystem()
        {
            myCPU = new MyCPU(this);
            _Processes = new ObservableCollection<MyProcess>();
            _listMyPros = new ObservableCollection<MyProcess>();
            IOList = new List<int>();
            _ram_ost = _ram;
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSourceTasks = new CancellationTokenSource();

            StartLauncTask(_cancellationTokenSourceTasks.Token);
            StartRamCheck(_cancellationTokenSource.Token);
            cpuState = CpuState.Waiting;

            Speed = 250;
            T_next = 1;
            T_IntiIO = 40;
            T_IntrIO = 3;
            T_Load = 4;
        }
        public void AddProcess(int CountCommand)
        {
            MyProcess proc;
            MyProcess.DInOut = D_InOut;

            if (CountCommand < 2)
            {
                MessageBox.Show("Для задачи нужно как минимум 2 команды.\nЗадание будет сгенерировано со случайным набором команд", 
                    "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                proc = new MyProcess();
            }
            else
                proc = new MyProcess(CountCommand);

            if (_ram_ost >= proc.Ram)
            {
                _Processes.Add(proc);
                _ram_ost -= proc.Ram;
            }
            else
            {
                _listMyPros.Add(proc);
            }

        }

        public void RemoveProcess(MyProcess proc) //Убирает с ObservableCollection процесс
        {
            proc.needDelete = true;
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
                    MyProcess.DInOut = D_InOut;
                    _listMyPros.Add(new MyProcess());
                    await Task.Delay(1000);
                }
            }
            catch (TaskCanceledException e)
            {
                MessageBox.Show($"Generating error:(TaskCanceledException) {e}");
            }
        }
        private async void StartLauncTask(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Planner();
                    await Task.Delay(10);
                }
            }
            catch (TaskCanceledException e)
            {
                MessageBox.Show($"Eroor:(TaskCanceledException in Task) {e}");
            }
        }

        private Task _ioTask; // Хранит фоновую задачу для InOut

        /// <summary>
        /// Планировщик
        /// </summary>
        public async Task Planner()
        {
            while (_Processes.Any())
            {
                //Загрузка задачи
                await Task.Delay(ConvertTaktToMillisec(T_Load));

                for (int i = 0; i < _Processes.Count; i++)
                {
                    cpuState = CpuState.Working;
                    var proc = _Processes[i];
                    bool isIO = (proc.State == ProcessState.InputOutput || proc.State == ProcessState.Init_IO || proc.State == ProcessState.End_IO);

                    //Если надо удалить процесс
                    if (proc.needDelete && !isIO)
                    {
                            proc.State = ProcessState.Completed;
                            _Processes.Remove(proc);
                            break;
                    }

                    if (isIO) //Процесс занят IO
                    {
                        cpuState = CpuState.Waiting;
                        continue;
                    }
                    else
                    {
                        myCPU.Execute(proc);
                        break;
                    }

                }
            }
        }
        //
        //в PSW счетчик команд



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
        //+ если процессор ожидает.
        public void CheckRam()
        {
            if (_ram_ost > 0 && cpuState == CpuState.Waiting)
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
