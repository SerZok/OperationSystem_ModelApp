using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows;


namespace OperationSystem_ModelApp.Model
{
    class MyOperationSystem
    {/// <summary>
     /// Число тактов
     /// </summary>
        public int Kvant;
        public int Takt { get; set; }

        private int _ram=1024;
        public int Ram { get => _ram; set=>_ram=value; }

        private int _ram_ost;

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
        public MyOperationSystem()
        {
            _Processes = new ObservableCollection<MyProcess>();
            _listMyPros = new List<MyProcess>();
            _ram_ost = _ram;
        }
        public void AddProcess()
        {
            _Processes.Add(new MyProcess());

        }
        public void RemoveProcess(MyProcess proc)
        {
            _Processes.Remove(proc);
            _ram_ost += proc.Ram;
        }
        public void PlusTakt()
        {
            while (true)
            {
                Takt++;
                Thread.Sleep(100);
            }
        }

        public List<MyProcess> _listMyPros;
        public async void Generating(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _listMyPros.Add(new MyProcess());

                    CheckRam();
                    await Task.Delay(1000);
                }
            }
            catch(TaskCanceledException e)
            {
                MessageBox.Show($"Generating error:(TaskCanceledException) {e}");
            }
        }

        //Если хватает свободнлй памяти для задачи,
        //то выгружаем из List и загружаем в ObservableCollection, т.е. там будут выполняться
        public void CheckRam()
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
    }
}
