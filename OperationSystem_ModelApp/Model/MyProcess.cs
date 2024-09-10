using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OperationSystem_ModelApp.Model
{
    public enum ProcessState
    {
        Ready,
        Running,
        Completed
    }
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
            countCommands = rnd.Next(2, 30); //Максммум 30 комманд

            for (int i = 0; i < countCommands; i++)
            {
                if (i==countCommands) //Последняя команда должна быть Завершающая (Close)
                {
                    var commandLast = new Command(true);
                    Commands.Add(commandLast);
                    break;
                }
                var command = new Command(false);
                Ram += command.TypeCmd.sizeTypeCommand;
                Commands.Add(command);
            }
        }
    }
    
}
