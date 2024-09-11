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
using OperationSystem_ModelApp.Model;

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
        private Visibility _isVisableProperty;
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


        public MainViewModel()
        {
            operatingSystem = new MyOperationSystem();
            _stopwatch=new Stopwatch();
            ProcessesOS = operatingSystem._Processes;
            IsGenerating = false;
            IsVisableProperty = Visibility.Hidden;
            lockObject = new object();

            RamOS = 1024;
            kvant = 40;
            
            threadForOS = new Thread(new ThreadStart(operatingSystem.CountTakt));
            threadForUI = new Thread(UpdateForUI);
            
        }

        public Random random;
        public ObservableCollection<MyProcess> ProcessesOS { get;  set; }

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
                CPU_State ="Состояние ЦП: "+ Enum.GetName(typeof(CpuState), operatingSystem.cpuState);

                if (RamOS == 0)
                    IsVisableProperty = Visibility.Hidden;
                else
                    IsVisableProperty = Visibility.Visible;

                if(operatingSystem.Ram!=RamOS)
                    operatingSystem.Ram = RamOS;

                if (MyOperationSystem.Kvant != Kvant)
                    MyOperationSystem.Kvant = Kvant;
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
        public int Takt
        {
            get { return takt; }
            set
            {
                if (takt != value){
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
                if(value != _ramOS) {
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
        public Visibility IsVisableProperty
        {
            get => _isVisableProperty;
            set
            {
                if (value != _isVisableProperty){
                    _isVisableProperty = value;
                    OnPropertyChanged("IsVisableProperty");
                }
            }
        }
        public MyProcess SelectedTask
        {
            get => selectedTask;
            set {
                if (selectedTask != value) {
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
                if (_countTasks != value){
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
        public RelayCommand AutoAddTask
        {
            get
            {
                return autoAddTask ??
                    (autoAddTask = new RelayCommand(obj =>{
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
                    (offAutoAddTask = new RelayCommand(obj =>{
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
                            IsVisableProperty = Visibility.Visible;
                            firstLaunch = false;
                            _stopwatch.Start();

                        }
                        else
                        {
                            _stopwatch.Restart();
                            RamOS = 1024;
                            operatingSystem.Ram_ost = RamOS;
                            operatingSystem.Takt = 0;

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
                        operatingSystem.AddProcess();

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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
