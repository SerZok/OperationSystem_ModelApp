using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using OperationSystem_ModelApp.Model;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Xml.Linq;

namespace OperationSystem_ModelApp.ViewModel
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private MyOperationSystem operatingSystem;
        private string textConsole;                 //Текст в консоли
        private RelayCommand startOS;               //Кнопка запуск ОС
        private RelayCommand addCommand;            //Кнопка добавить задачу
        private RelayCommand delCommand;            //Кнопка удалить выбранную задачу
        private RelayCommand autoAddTask;           //Включатель авто-генерации задач
        private RelayCommand offAutoAddTask;        //Выключтель авто-генерации
        private bool isGenerating;                  //Bool для переключателя
        private Thread threadForOS, threadForUI;    //Потоки для обновления и генерации в цикле
        private int downloadCountTasks;             //Количество задач, которые выполняются
        private bool _isVisableProperty;
        private MyProcess selectedTask;             //Выбарнная задача
        private int _ramOS;                         //ОЗУ ОС
        private int _ramOS_ostatok;
        private int takt;                           //Такты
        private int kvant;                          //Кванты
        private bool firstLaunch = true;            //Чтоб не ломать потоки
        private int _countTasks;                    //Задачи на выполнении
        private int _countListTasks;                //Всего задач 
        private CancellationTokenSource cancellationTokenSource; //Для корректной работы генератора заданий
        private Stopwatch _stopwatch;
        private string _strTimeOS;
        private string cpu_State;
        private readonly object lockObject;
        private int speedOS;
        private int numToBeCreate;
        private RelayCommand stopOS;
        private int _osCost;
        private int _competedTasks;
        private int _d_InOut;
        private string _pathToFile;
        private bool _IsEnabled;
        public bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                _IsEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }
        public string PathToFile { get => _pathToFile; set => _pathToFile = value; }


        private int t_next;
        private int t_IntiIO;
        private int t_IntrIO;
        private int t_Load;
        public int D_InOut //Процент команд IO
        {
            get => _d_InOut;
            set
            {
                _d_InOut = value;
                OnPropertyChanged("D_InOut");
            }
        }

        public MainViewModel()
        {
            operatingSystem = new MyOperationSystem();
            _stopwatch = new Stopwatch();
            ProcessesOS = operatingSystem._Processes;
            IsGenerating = false;
            IsVisableProperty = false;
            IsEnabled = true;
            lockObject = new object();

            RamOS = 1024;
            kvant = 40;
            SpeedOS = 200;
            OsCost = 0;
            CompletedTasks = 0;

            threadForOS = new Thread(new ThreadStart(operatingSystem.CountTakt));
            threadForUI = new Thread(UpdateForUI);

            T_next = 2;
            T_IntiIO = 2;
            T_IntrIO = 2;
            T_Load = 4;
            D_InOut = 20;
        }

        public Random random;
        public ObservableCollection<MyProcess> ProcessesOS { get; set; }
        public class ParamsForOS
        {
            public int Ram { get; set; }
            public int TNext { get; set; }
            public int TInitIO { get; set; }
            public int TIntrIO { get; set; }
            public int TLoad { get; set; }
            public int Kvant { get; set; }
            public List<MyProcess> Tasks { get; set; }
            public ParamsForOS(int ram, int tnext, int tinitio, int tintrio, int tload, int kvant, List<MyProcess> tasks)
            {
                Ram = ram;
                TNext = tnext;
                TInitIO = tinitio;
                TIntrIO = tintrio;
                TLoad = tload;
                Kvant = kvant;
                Tasks = tasks;
            }
        }

        //Метод для обновления полей для отображения. Нужен для потока
        private void UpdateForUI()
        {
            while (true)
            {
                Takt = operatingSystem.Takt;
                CountTasks = ProcessesOS.Count;
                CountListTasks = operatingSystem._listMyPros.Count;
                RamOS_ostatok = operatingSystem.Ram_ost;
                StrTimeOS = $"Времени прошло: {_stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.ff")}";
                CPU_State = "Состояние ЦП: " + Enum.GetName(typeof(CpuState), operatingSystem.cpuState);

                //if (RamOS == 0)
                //    IsVisableProperty = false;
                //else
                //    IsVisableProperty = true;

                if (operatingSystem.Ram != RamOS)
                    operatingSystem.Ram = RamOS;

                if (MyOperationSystem.Kvant != Kvant)
                    MyOperationSystem.Kvant = Kvant;

                operatingSystem.Speed = SpeedOS;

                operatingSystem.T_next = T_next;
                operatingSystem.T_IntiIO = T_IntiIO;
                operatingSystem.T_IntrIO = T_IntrIO;
                operatingSystem.T_Load = T_Load;

                OsCost = (100 - (int)Math.Round((double)RamOS_ostatok / RamOS * 100));

                CompletedTasks = operatingSystem.CompetedTasks;
                operatingSystem.D_InOut = D_InOut;
            }
        }
        private bool IsGenerating
        {
            get { return isGenerating; }
            set
            {
                isGenerating = value;
                OnPropertyChanged("IsGenerating");

                if (isGenerating)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    operatingSystem.Generating(cancellationTokenSource.Token);
                }
                else
                {
                    cancellationTokenSource?.Cancel();
                }
            }
        }
        public int SpeedOS
        {
            get => speedOS;
            set
            {
                if (speedOS != value)
                {
                    speedOS = value;
                    OnPropertyChanged("SpeedOS");
                }
            }
        }
        public int Takt
        {
            get { return takt; }
            set
            {
                if (takt != value)
                {
                    takt = value;
                    OnPropertyChanged("Takt");
                }
            }
        }
        public int RamOS
        {
            get => _ramOS;
            set
            {
                if (value != _ramOS)
                {
                    _ramOS = value;
                    OnPropertyChanged("RamOS");
                }
            }
        }
        public int RamOS_ostatok
        {
            get => _ramOS_ostatok;
            set
            {
                _ramOS_ostatok = value;
                OnPropertyChanged("RamOS_ostatok");
            }
        }
        public string StrTimeOS
        {
            get => _strTimeOS;
            set
            {
                if (_strTimeOS != value)
                {
                    _strTimeOS = value;
                    OnPropertyChanged("StrTimeOS");
                }
            }
        }
        public string CPU_State
        {
            get => cpu_State;
            set
            {
                if (cpu_State != value)
                {
                    cpu_State = value;
                    OnPropertyChanged("CPU_State");
                }
            }
        }
        public int Kvant
        {
            get => kvant;
            set
            {
                kvant = value;
                OnPropertyChanged("Kvant");
            }
        }
        public int T_next
        {
            get => t_next;
            set
            {
                if (t_next != value)
                {
                    t_next = value;
                    OnPropertyChanged("T_next");
                }
            }
        }
        public int T_IntiIO
        {
            get => t_IntiIO;
            set
            {
                if (t_IntiIO != value)
                {
                    t_IntiIO = value;
                    OnPropertyChanged("T_IntiIO");
                }
            }
        }
        public int T_IntrIO
        {
            get => t_IntrIO;
            set
            {
                if (t_IntrIO != value)
                {
                    t_IntrIO = value;
                    OnPropertyChanged("T_IntrIO");
                }
            }
        }
        public int T_Load
        {
            get => t_Load;
            set
            {
                if (t_Load != value)
                {
                    t_Load = value;
                    OnPropertyChanged("T_Load");
                }
            }
        }
        public int OsCost
        {
            get => _osCost;
            set
            {
                _osCost = value;
                OnPropertyChanged("OsCost");
            }
        }
        public int CompletedTasks
        {
            get => _competedTasks;
            set
            {
                _competedTasks = value;
                OnPropertyChanged("CompletedTasks");
            }
        }
        public bool IsVisableProperty
        {
            get => _isVisableProperty;
            set
            {
                if (value != _isVisableProperty)
                {
                    _isVisableProperty = value;
                    OnPropertyChanged("IsVisableProperty");
                }
            }
        }
        public MyProcess SelectedTask
        {
            get => selectedTask;
            set
            {
                if (selectedTask != value)
                {
                    selectedTask = value;
                    OnPropertyChanged("SelectedTask");
                }
            }
        }
        public string TextConsole
        {
            get { return textConsole; }
            private set
            {
                textConsole = value;
                OnPropertyChanged("TextConsole");
            }
        }
        public int CountTasks
        {
            get => _countTasks;
            set
            {
                if (_countTasks != value)
                {
                    _countTasks = value;
                    OnPropertyChanged("CountTasks");
                }
            }
        }
        public int CountListTasks
        {
            get { return _countListTasks; }
            set
            {
                if (_countListTasks != value)
                {
                    _countListTasks = value;
                    OnPropertyChanged("CountListTasks");
                }
            }
        }
        public int NumToBeCreate
        {
            get => numToBeCreate;
            set
            {
                if (value != numToBeCreate)
                {
                    numToBeCreate = value;
                    OnPropertyChanged("NumToBeCreate");
                }
            }
        }

        private RelayCommand _parseCommand;

        public RelayCommand ParseCommand
        {
            get
            {
                return _parseCommand ??
                    (_parseCommand ?? new RelayCommand(obj =>
                    {
                        if (PathToFile == null)
                            return;

                        using (FileStream fs = new FileStream(PathToFile, FileMode.OpenOrCreate))
                        {
                            try
                            {

                                var options = new JsonSerializerOptions
                                {
                                    IncludeFields = true // Включение десериализации полей
                                };

                                var json = JsonSerializer.Deserialize<ParamsForOS>(fs, options);
                                //var json = JsonSerializer.Deserialize<ParamsForOS>(fs);

                                if (json != null)
                                {
                                    RamOS = json.Ram;
                                    T_next = json.TNext;
                                    T_IntiIO = json.TInitIO;
                                    T_IntrIO = json.TIntrIO;
                                    T_Load = json.TLoad;
                                    Kvant = json.Kvant;

                                    Debug.WriteLine($"Ram:{json.Ram} Tload:{json.TLoad} Kvant:{json.Kvant} TinitIO:{json.TInitIO} TInitrIO:{json.TIntrIO} TNext:{json.TNext}");
                                    foreach(var pro in json.Tasks)
                                    {
                                        operatingSystem._listMyPros.Add(pro);

                                        Debug.WriteLine($"\nProc id:{pro.Id}");
                                        foreach(var com in pro.Commands)
                                        {
                                            Debug.WriteLine($"Type:{com.TypeCmd.nameTypeCommand}");
                                        }
                                    }

                                    threadForOS.Start();
                                    threadForUI.Start();
                                    IsVisableProperty = true;
                                    IsEnabled = false;
                                    firstLaunch = false;
                                    _stopwatch.Start();

                                }
                                else
                                {
                                    MessageBox.Show("Ошибка десириализации!");
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"{ex.Message}");
                            }
                        }
                    }));
            }
        }
        public RelayCommand AutoAddTask
        {
            get
            {
                return autoAddTask ??
                    (autoAddTask = new RelayCommand(obj =>
                    {
                        lock (lockObject)
                        {
                            if (!isGenerating)
                            {
                                IsGenerating = true; // Запускаем задачу
                            }
                        }
                    }));
            }
        }
        public RelayCommand OffAutoAddTask
        {
            get
            {
                return offAutoAddTask ??
                    (offAutoAddTask = new RelayCommand(obj =>
                    {
                        lock (lockObject) //Чтоб не было гонки на переключатель
                        {
                            if (isGenerating)
                            {
                                IsGenerating = false; // Останавливаем задачу
                            }
                        }
                    }));
            }
        }
        public RelayCommand StartOS
        {
            get
            {
                return startOS ??
                    (startOS = new RelayCommand(obj =>
                    {
                        if (firstLaunch)
                        {
                            threadForOS.Start();
                            threadForUI.Start();
                            IsVisableProperty = true;
                            IsEnabled = false;
                            firstLaunch = false;
                            _stopwatch.Start();

                        }
                        else
                        {
                            _stopwatch.Restart();
                            RamOS = 1024;
                            operatingSystem.Ram_ost = RamOS;
                            operatingSystem.Takt = 0;
                            operatingSystem.CompetedTasks = 0;

                            operatingSystem._listMyPros.Clear();
                            operatingSystem._Processes.Clear();
                        }
                        TextConsole = "Запуск Операционной системы...";
                    }));
            }
        }
        public RelayCommand ADDCommand
        {
            get
            {

                return addCommand ??
                    (addCommand = new RelayCommand(obj =>
                    {
                        operatingSystem.AddProcess(NumToBeCreate);
                    }));
            }
        }
        public RelayCommand DelTask
        {
            get
            {
                return delCommand ??
                    (delCommand = new RelayCommand(obj =>
                    {
                        if (SelectedTask != null)
                        {
                            operatingSystem.RemoveProcess(SelectedTask);
                        }
                    }));
            }
        }
        public RelayCommand StopOS
        {
            get
            {
                return stopOS ??
                    (stopOS = new RelayCommand(obj =>
                    {
                        MainWindow window = new MainWindow();
                        window.Close();
                    }));

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
