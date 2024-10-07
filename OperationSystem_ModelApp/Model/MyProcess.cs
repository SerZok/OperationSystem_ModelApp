using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        Completed,
        InputOutput,
        InitIO,
        StartTask
    }
    internal class MyProcess: INotifyPropertyChanged
    {
        private Random rnd = new Random();

        private int id;
        private static int _count = 0;

        /// <summary>
        /// Количество памяти для выполнения задания;
        /// Сумма памяти команд
        /// </summary>
        public int Ram { get; private set; }

        /// <summary>
        /// Список команд, которые надо выполнить
        /// </summary>
        public ObservableCollection<Command> Commands { get; set; } = new ObservableCollection<Command>();

        private int countCommands;
        private int _countAllCommands;
        public int CountAllCommands
        {
            get => _countAllCommands;
            set { 
                _countAllCommands = value;
                OnPropertyChanged("CountAllCommands");
            }
        }
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                _count++;
                OnPropertyChanged("Id");
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

        static public int DInOut {get; set;}
        public MyProcess()
        {
            Id = _count;
            countCommands = rnd.Next(2, 30); //Максммум 30 комманд
            CountAllCommands = countCommands;

            int ioCommandCount = (int)((double)DInOut / 100 * countCommands);

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
                while (ioCommandCount>0)
                {
                    var IOcommand = new Command(false,true);
                    Ram += IOcommand.TypeCmd.sizeTypeCommand;
                    Commands.Add(IOcommand);
                    ioCommandCount--;
                    i++;
                }

                var command = new Command(false);
                Ram += command.TypeCmd.sizeTypeCommand;
                Commands.Add(command);
            }
            State = ProcessState.Ready;
        }

        public MyProcess(int countCom)
        {
            Id = _count;

            countCommands = countCom;
            CountAllCommands = countCommands;
            int ioCommandCount = (int)((double)DInOut / 100 * countCommands);

            Debug.WriteLine($"*********Task #{id} **********");
            //Генерация команд
            for (int i = 1; i <= countCommands; i++)
            {
                if (i == countCommands) //Последняя команда должна быть Завершающая (Close)
                {
                    var commandLast = new Command(true); //true -> Последняя
                    Commands.Add(commandLast);
                    Ram += commandLast.TypeCmd.sizeTypeCommand;
                    break;
                }
                while (ioCommandCount > 0)
                {
                    var IOcommand = new Command(false, true);
                    Ram += IOcommand.TypeCmd.sizeTypeCommand;
                    Commands.Add(IOcommand);
                    ioCommandCount--;
                    i++;
                }

                var command = new Command(false);
                Ram += command.TypeCmd.sizeTypeCommand;
                Commands.Add(command);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
