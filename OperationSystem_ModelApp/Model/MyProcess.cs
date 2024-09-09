using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperationSystem_ModelApp.Model
{
    internal class MyProcess
    {
        private Random rnd;
        private static int id = 0;
        /// <summary>
        /// Количество памяти для выполнения задания;
        /// Сумма памяти команд
        /// </summary>
        public int Ram { get; set; }

        /// <summary>
        /// Список команд, которые надо выполнить
        /// </summary>
        public List<Command> Commands { get; set; }
        public int Id
        {
            get
            {
                return id++;
            }
        }

        /// <summary>
        /// Состояние процесса (Готов, Выполняется, Завершен)
        /// </summary>
        public ProcessState State { get; set; }

        public MyProcess()
        {
            rnd = new Random();
            Ram = rnd.Next(1, 1000);
            State = ProcessState.Running;
        }
    }

    public class Command
    {
        public string Name { get; set; }
        public int TaktCompeate { get; set; }
        public int RamCommand { get; set; }

    }

    public enum ProcessState
    {
        Ready,
        Running,
        Completed
    }
}
