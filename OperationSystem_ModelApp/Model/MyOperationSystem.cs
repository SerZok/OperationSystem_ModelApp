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
using System.Windows.Documents;
using System.Security.RightsManagement;


namespace OperationSystem_ModelApp.Model
{
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

        private int _totalOborotTime;
        private int _totalMonoTime;

        private float _oborotTime;
        private float _t_mono;
        public float ObobrotTime
        {
            get => _oborotTime;
            set
            {
                _oborotTime = value;
                OnPropertyChanged("ObobrotTime");
            }
        }

        public float T_mono
        {
            get => _t_mono;
            set
            {
                _t_mono = value;
                OnPropertyChanged("T_mono");
            }
        }

        public ObservableCollection<MyProcess> _Processes;
        public ObservableCollection<MyProcess> _listMyPros;
        public List<int> IOList;

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
            myCPU.cpuState = CpuState.Waiting;

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
                MessageBox.Show("Для задачи нужно как минимум 2 команды.", 
                    "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
                proc = new MyProcess(CountCommand);

            _listMyPros.Add(proc);
        }
        public void RemoveProcess(MyProcess proc) 
        {
            proc.needDelete = true;
        }
        public void PauseCommand(MyProcess proc)
        {
            proc.IsStopped = !proc.IsStopped;
            var prevState = proc.State;
            //if (proc.IsStopped)
            //    proc.State = ProcessState.Paused;
            //else
            //    proc.State = prevState;
        }
        public void CountTakt() //Счетчик тактов
        {
            while (true)
            {
                Takt++;
                if (_Processes.Any())
                {
                    foreach (var proc in _Processes)
                    {
                        if(proc.State == ProcessState.Ready)
                        {
                            proc.WaitTime++;
                        }
                    }

                }
                Thread.Sleep(Speed);
            }
        }

        public int ConvertTaktToMillisec(int takt, int speed)
        {
            return takt * speed;
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

        /// <summary>
        /// Планировщик
        /// </summary>
        public async Task Planner()
        {
            while (_Processes.Any())
            {
                //Загрузка задачи
                await Task.Delay(ConvertTaktToMillisec(T_Load,Speed));

                for (int i = 0; i < _Processes.Count; i++)
                {
                    //myCPU.cpuState = CpuState.Working;
                    myCPU.ChangeCpuState(myCPU, CpuState.Working);
                    var proc = _Processes[i];

                    if(proc.StartTime==0)
                        proc.StartTime = Takt;

                    bool isIO = (proc.State == ProcessState.InputOutput || proc.State == ProcessState.Init_IO || proc.State == ProcessState.End_IO);

                    //Если надо удалить процесс
                    if (proc.needDelete && !isIO)
                    {
                        ChangeProcState(proc, ProcessState.Completed);
                        _Processes.Remove(proc);
                        _ram_ost += proc.Ram;
                        break;
                    }

                    //Если надо приостановить процесс
                    if(proc.IsStopped && !isIO)
                    {
                        ChangeProcState(proc, ProcessState.Paused);
                        continue;
                    }

                    if (isIO) //Процесс занят IO
                    {
                        myCPU.ChangeCpuState(myCPU, CpuState.Waiting);
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

                            ChangeProcState(proc, ProcessState.Ready);
                            myCPU.ChangeCpuState(myCPU, CpuState.Waiting);
                        }
                        // Процесс завершен, его больше не нужно планировать
                        else if (proc.State == ProcessState.Completed)
                        {
                            int obTime = (Takt - proc.StartTime);
                            _totalOborotTime += obTime;
                            ObobrotTime = _totalOborotTime / CompetedTasks;

                            int Tmono = proc.AllTime + proc.WaitTime + T_Load + T_IntiIO + T_IntrIO + T_next;
                            _totalMonoTime += Tmono;
                            T_mono = _totalMonoTime / CompetedTasks;
                            T_mono = Takt / T_mono;

                            _Processes.Remove(proc);
                            _ram_ost += proc.Ram;
                            myCPU.ChangeCpuState(myCPU, CpuState.Waiting);
                        }

                        // Прерываем цикл после обработки одного процесса
                        break;
                    }
                }
                //myCPU.cpuState = CpuState.Waiting;
                myCPU.ChangeCpuState(myCPU, CpuState.Waiting);
            }
        }

        /// <summary>
        /// Изменение состояния процесса
        /// </summary>
        /// <param name="proc">Процесс, которому надо изменить состояние </param>
        /// <param name="state">Состояние, которое надо присвоить </param>
        public void ChangeProcState(MyProcess proc, ProcessState state)
        {

            if (proc.State != state)
                proc.State = state;
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
        //+ если процессор ожидает.
        public void CheckRam()
        {
            if (_ram_ost > 0 && myCPU.cpuState == CpuState.Waiting)
            {
                for (int i = 0; i < _listMyPros.Count ; i++)
                {
                    if (_ram_ost >= _listMyPros[i].Ram)
                    {
                        _Processes.Add(_listMyPros[i]);
                        _ram_ost -= _listMyPros[i].Ram;
                        _listMyPros.Remove(_listMyPros[i]);
                    }

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
