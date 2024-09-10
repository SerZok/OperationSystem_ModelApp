using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    internal class MyProcess: INotifyPropertyChanged
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
        private ProcessState state;
        public ProcessState State 
        {
            get => state;
            set
            {
                state = value;
                OnPropertyChanged("State");
            }
        }

        public MyProcess()
        {
            rnd = new Random();
            Commands = new List<Command>();
            countCommands = rnd.Next(2, 30); //Максммум 30 комманд

            //Генерация команд
            for (int i = 1; i <= countCommands; i++)
            {
                if (i==countCommands) //Последняя команда должна быть Завершающая (Close)
                {
                    var commandLast = new Command(true); //true -> Последняя
                    Commands.Add(commandLast);
                    Ram += commandLast.TypeCmd.sizeTypeCommand;
                    break;
                }
                var command = new Command(false);
                Ram += command.TypeCmd.sizeTypeCommand;
                Commands.Add(command);
            }

            State = ProcessState.Ready;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
