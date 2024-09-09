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
        public int Kvant
        {
            get => kvant;
            set
            {
                kvant = value;
                OnPropertyChanged("Kvant");
            }
        }
        public MainViewModel()
        {
            operatingSystem = new MyOperationSystem();
            ProcessesOS = operatingSystem._Processes;
            IsGenerating = false;
            IsVisableProperty = Visibility.Hidden;
            RamOS = 1024;

            threadForOS = new Thread(new ThreadStart(operatingSystem.PlusTakt));
            threadForUI = new Thread(UpdateForUI);
            
        }

        public Random random;
        public ObservableCollection<MyProcess> ProcessesOS { get; private set; }
        private void UpdateForUI() //Метод для обновления полей для отображения. Нужен для потока
        {
            while (true)
            {
                Takt = operatingSystem.Takt;
                operatingSystem.Ram = RamOS;
                CountTasks = ProcessesOS.Count;
                CountListTasks = operatingSystem._listMyPros.Count;
                
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
                    (autoAddTask = new RelayCommand(obj =>
                    {
                        IsGenerating = true;
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
                        IsGenerating = false;
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
                        }
                        else
                        {
                            //Просто обнуляем счетчик тактов
                            operatingSystem.Takt = 0;
                            ProcessesOS.Clear();
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
