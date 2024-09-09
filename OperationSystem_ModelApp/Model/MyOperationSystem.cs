using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;


namespace OperationSystem_ModelApp.Model
{
    class MyOperationSystem
    {/// <summary>
     /// Число тактов
     /// </summary>
        public int Kvant;
        public int _Takt { get; set; }

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
        public MyOperationSystem(ObservableCollection<MyProcess> list)
        {
            _Processes = list;
        }

        public void AddProcess()
        {
            _Processes.Add(new MyProcess());

        }
        public void RemoveProcess(MyProcess proc)
        {
            _Processes.Remove(proc);
        }

        public void Update()
        {
            while (true)
            {
                _Takt++;
                Thread.Sleep(100);
            }
        }
    }
}
