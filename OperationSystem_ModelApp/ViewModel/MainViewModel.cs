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
        private string textConsole;
        private RelayCommand startOS;
        private RelayCommand addCommand;
        private RelayCommand delCommand;
        private RelayCommand autoAddTask;
        private RelayCommand offAutoAddTask;
        private int downloadCountTasks;
        private bool isGenerating;
        private Visibility _isVisableProperty;
        private MyProcess selectedTask;
        private int _ramOS;
        private int takt;
        private Thread threadForOS, threadForUI;
        public int kvant;
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
            operatingSystem = new MyOperationSystem(new ObservableCollection<MyProcess>());
            ProcessesOS = operatingSystem._Processes;
            IsGenerating = false;
            IsVisableProperty = Visibility.Hidden;

            threadForOS = new Thread(new ThreadStart(operatingSystem.Update));
            threadForUI = new Thread(UpdateForUI);

        }

        private bool firstLaunch = true;
        private void UpdateForUI() //Метод для обновления Тиков. Нужен для потока
        {
            while (true)
            {
                Takt = operatingSystem._Takt;
                operatingSystem.Ram = RamOS;
            }
        }

        public int Takt
        {
            get { return takt; }
            set
            {
                takt = value;
                OnPropertyChanged("Takt");
            }
        }

        public int RamOS
        {
            get
            {
                return _ramOS;
            }
            set
            {
                _ramOS = value;
                OnPropertyChanged("RamOS");
            }

        }
        public Visibility IsVisableProperty
        {
            get { return _isVisableProperty; }
            set
            {
                if (_isVisableProperty != value)
                {
                    _isVisableProperty = value;
                    OnPropertyChanged("IsVisableProperty");
                }
            }
        }

        public Random random;
        public ObservableCollection<MyProcess> ProcessesOS { get; private set; }
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
        public int DownloadCountTasks
        {
            get => downloadCountTasks;
            set
            {
                downloadCountTasks = value;
                OnPropertyChanged("DownloadCountTasks");
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
                    StartGenerating();
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

        private async void StartGenerating()
        {
            while (IsGenerating)
            {
                operatingSystem.AddProcess();
                DownloadCountTasks++;
                await Task.Delay(1000);
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
                            operatingSystem._Takt = 0;
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
                        DownloadCountTasks++;
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
                            DownloadCountTasks--;
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
