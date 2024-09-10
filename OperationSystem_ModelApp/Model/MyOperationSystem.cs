using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Net.WebSockets;


namespace OperationSystem_ModelApp.Model
{
    class MyOperationSystem
    {/// <summary>
     /// Число тактов
     /// </summary>
        public int Kvant;
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
        private int Speed;

        public ObservableCollection<MyProcess> _Processes;
        public List<MyProcess> _listMyPros;

        private CancellationTokenSource _cancellationTokenSource; //Нужен для проверки ОЗУ
        public MyOperationSystem()
        {
            _Processes = new ObservableCollection<MyProcess>();
            _listMyPros = new List<MyProcess>();
            _ram_ost = _ram;
            _cancellationTokenSource = new CancellationTokenSource();
            StartRamCheck(_cancellationTokenSource.Token);
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
                Thread.Sleep(100);
            }
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
                MessageBox.Show($"Eroor:(TaskCanceledException) {e}");
            }
        }   //Старт проверки ОЗУ

        public void StopRamCheck()
        {
            _cancellationTokenSource.Cancel();
        } //Прекратить  проверять ОЗУ (Может быть полезно в будущем)

        //Если хватает свободнлй памяти для задачи,
        //то выгружаем из List и загружаем в ObservableCollection, т.е. там будут выполняться
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
    }
}
