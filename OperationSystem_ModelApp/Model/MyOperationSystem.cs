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

        public int Ram;

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
        }
        public void AddProcess()
        {
            _Processes.Add(new MyProcess());

        }
        public void RemoveProcess(MyProcess proc)
        {
            _Processes.Remove(proc);
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
        public async void StartGenerating(CancellationToken cancellationToken)
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

        private void CheckRam()
        {
            //Если хватает свободнлй памяти для задачи,
            //то выгружаем из List и загружаем в ObservableCollection, т.е. там будут выполняться
        }
    }
}
