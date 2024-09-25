﻿using System;
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
        public int T_next;

        /// <summary>
        /// Затраты ОС на изменение состояния процесса	
        /// по обращению ко вводу(выводу) (в числе тактов)
        /// </summary>
        public int T_IntiIO;

        /// <summary>
        ///  Затраты ОС по обслуживанию сигнала окончания T_IntrIO
        ///  (прерывания) ввода(вывода) (в числе тактов)
        /// </summary>
        public int T_IntrIO;

        /// <summary>
        /// Число тактов на загрузку нового задания
        /// </summary>
        public int T_Load;

        /// <summary>
        /// Скорость работы ОС
        /// </summary>
        /// 
        public int Speed { 
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

        public ObservableCollection<MyProcess> _Processes;
        public ObservableCollection<MyProcess> _listMyPros;
        public List<int> IOList;

        public CpuState cpuState;

        private CancellationTokenSource _cancellationTokenSource; //Нужен для проверки ОЗУ
        private CancellationTokenSource _cancellationTokenSourceTasks; //Для проверки запуска заданий
        private Thread _ioThread;
        private readonly object _lock = new object();
        public MyOperationSystem()
        {
            _ioThread = new Thread(InOut);
            

            _Processes = new ObservableCollection<MyProcess>();
            _listMyPros = new ObservableCollection<MyProcess>();
            _ram_ost = _ram;
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSourceTasks = new CancellationTokenSource();
            IOList = new List<int>();


            StartLauncTask(_cancellationTokenSourceTasks.Token);
            StartRamCheck(_cancellationTokenSource.Token);
            cpuState = CpuState.Waiting;

            Speed = 250;
            T_next      = 1;
            T_IntiIO    = 60; //Желательно чтоб было дольше самого кванта
            T_IntrIO    = 3;
            T_Load      = 4;
        }

        public void AddProcess(int CountCommand)
        {
            MyProcess proc;
            if (CountCommand < 2)
            {
                MessageBox.Show("Для задачи нужно как минимум 2 команды.\nЗадание будет сгенерировано со случайным набором команд","Ошибка",MessageBoxButton.OK, MessageBoxImage.Error);
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
            lock (_lock)
            {
                _Processes.Remove(proc);
                _ram_ost += proc.Ram;
            }
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
        //Надо сделать:
        //Если процесс InputOutput, то надо следующую задачу
        public async Task LauncTask()
        {

            while (_Processes.Any())
            {
                //Загрузка задачи
                await Task.Delay(ConvertTaktToMillisec(T_Load));

                for (int i = 0; i<_Processes.Count;i++)
                {
                    cpuState = CpuState.Working;
                    var proc = _Processes[i];
                    if (proc.State == ProcessState.InputOutput)
                    {
                        cpuState = CpuState.Waiting;
                        if (_ioThread.ThreadState == ThreadState.Unstarted || _ioThread.ThreadState == ThreadState.Aborted)
                            _ioThread.Start();
                        Task.Run(() => InOut());

                        continue;
                    }

                    
                    //Выполнять 1 квант (Kavnt-=takt)
                    if (proc.State != ProcessState.Completed) //Если не завершена задача
                    {
                        
                        //Запуск задачи
                        await Task.Delay(ConvertTaktToMillisec(T_next));
                        //Выполнение команд
                        while (proc.Commands.Any())
                        {
                            var fCommand = proc.Commands.First();
                            //Если комманда IO
                            if (fCommand.TypeCmd.nameTypeCommand == NameTypeCommand.IO)
                            {


                                IOList.Add(proc.Id); //Добавляем ID задачи в список, которые обрабатываю IO
                                proc.State = ProcessState.InputOutput;
                                //if (_ioThread.ThreadState == ThreadState.Unstarted || _ioThread.ThreadState==ThreadState.Aborted)
                                //    _ioThread.Start();
                                Task.Run(() => InOut());
                                break;

                            }
                            else //Если команда не IO
                            {
                                proc.State = ProcessState.Running;
                                await Task.Delay(ConvertTaktToMillisec(fCommand.TypeCmd.timeTypeCommand));
                                proc.Commands.Remove(fCommand);
                            }
                        }

                        if (!proc.Commands.Any())
                        {
                            proc.State = ProcessState.Completed; // Помечаем процесс завершенным, если команды кончились
                            cpuState = CpuState.Waiting;
                            break;
                        }
                    }
                    else //Задача завершена
                    {
                        RemoveProcess(proc);
                        i--;
                        cpuState = CpuState.Waiting;
                        break;
                    }
                }
            }
        }

        private void InOut()
        {
            lock (_Processes)
            {
                var procList = _Processes.ToList();
                while (IOList.Any() && _Processes.Any())
                {
                    cpuState = CpuState.Waiting;
                    var id = IOList.First();

                    var proc = procList.FirstOrDefault(x => x.Id == id);
                    if (proc != null)
                    {
                        //await Task.Delay(ConvertTaktToMillisec(T_IntiIO));
                        Thread.Sleep(ConvertTaktToMillisec(T_IntiIO));
                        proc.Commands.Remove(proc.Commands.First());

                        // Если есть еще команды, процесс снова переходит в состояние Running
                        if (proc.Commands.Any())
                            proc.State = ProcessState.Running;
                        else
                            proc.State = ProcessState.Completed; // Помечаем процесс завершенным, если все команды завершены

                        IOList.Remove(id);
                    }
                }
            }
            cpuState = CpuState.Working;
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
