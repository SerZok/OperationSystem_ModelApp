using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        public int Ram { get; private set; }

        /// <summary>
        /// Список команд, которые надо выполнить
        /// </summary>
        public List<Command> Commands { get; set; }

        private int countCommands;
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
            Commands = new List<Command>();
            countCommands = rnd.Next(1, 10); //Максммум 10 комманд

            for (int i = 0; i < countCommands; i++)
            {
                var command = new Command();
                Commands.Add(command);
                Ram += command.RamCommand;
            }
            //MessageBox.Show($"Command #{Id}: Ram:{Ram}");
        }
    }

    public class Command
    {
        
        private int id;
        private Random rnd;
        public int Id
        {
            get => id;
            set 
            {
                id++;
            }
        }
        public int TimeForCommand { get; set; }
        public int RamCommand { get; set; }

        public Command()
        {
            rnd = new Random();
            TimeForCommand = rnd.Next(0, 10000);
            RamCommand = rnd.Next(10, 100);
        }
    }
    public enum ProcessState
    {
        Ready,
        Running,
        Completed
    }
}
