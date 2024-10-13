using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        Init_IO,
        End_IO,
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

        private int _procTakt;
        public int ProcTakt { 
            get=>_procTakt;
            set
            {
                _procTakt = value;
                OnPropertyChanged("ProcTakt");
            }
        }

        async public Task DoTask(Command cmd, int speed, bool isThread=false)
        {
            ProcTakt = cmd.TypeCmd.timeTypeCommand;
            while (ProcTakt > 0)
            {
                ProcTakt--;
                if (isThread) 
                    Thread.Sleep(speed);
                else
                    await Task.Delay(speed);
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

        
        public MyProcess(int i, ObservableCollection<Command> commands)
        {
            Id = i;
            Commands = commands;
        }
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
