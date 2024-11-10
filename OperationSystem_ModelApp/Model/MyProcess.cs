using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace OperationSystem_ModelApp.Model
{
    public enum ProcessState
    {
        Ready,
        Running,
        Paused,
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

        //флаг на удаление
        public bool needDelete = false;

        /// <summary>
        /// Количество памяти для выполнения задания;
        /// Сумма памяти команд
        /// </summary>
        public int Ram { get; private set; }

        /// <summary>
        /// Список команд, которые надо выполнить
        /// </summary>
        public ObservableCollection<Command> Commands { get; set; }

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

        private int _currentCommandIndex = 0;
        public int CurrentCommandIndex
        {
            get => _currentCommandIndex;
            set
            {
                _currentCommandIndex = value;
                OnPropertyChanged("CurrentCommandIndex");
            }
        }

        private int psw;
        public int PSW
        {
            get => psw;
            set
            {
                psw = value;
                OnPropertyChanged("PSW");
            }
        }

        private bool _stopped;
        public bool IsStopped
        {
            get => _stopped;
            set
            {
                _stopped = value;
                OnPropertyChanged("Stopped");
            }
        }

        public int StartTime { get; set; } = 0;
        public int AllTime { get; set; }
        public int WaitTime {  get; set; } = 0;

        /// <summary>
        /// Выполнение команды с возможностью остановки по завершении кванта
        /// </summary>
        /// <param name="cmd">Команда для выполнения</param>
        /// <param name="speed">Скорость выполнения (в тактах)</param>
        /// <param name="maxTakts">Максимум тактов для выполнения (квант)</param>
        /// <returns>Количество потраченных тактов</returns>
        async public Task<int> DoTask(Command cmd, int speed, int maxTakts, bool isThread = false)
        {
            int taktsUsed = 0;
            ProcTakt = cmd.TypeCmd.timeTypeCommand;

            while (ProcTakt > 0 && taktsUsed < maxTakts)
            {
                ProcTakt--;
                taktsUsed++;

                if (isThread)
                    Thread.Sleep(speed);
                else
                    await Task.Delay(speed);
            }

            return taktsUsed; // Возвращаем количество использованных тактов
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

        static public int DInOut { get; set;}
        public MyProcess(int i, List<Command> commands)
            //для парсинга JSON 
        {
            Id = i;
            Commands = new ObservableCollection<Command>(commands);
            foreach (var cmd in Commands)
            {
                AllTime += cmd.TypeCmd.timeTypeCommand;
            }
        }
        public MyProcess()
        {
            Commands = new ObservableCollection<Command>();
            Id = _count;
            countCommands = rnd.Next(2, 30); //Максммум 30 комманд
            CountAllCommands = countCommands;
            IsStopped = false;

            Debug.WriteLine($"*********Task #{id} **********");
            int ioCommandCount = (int)((double)DInOut / 100 * countCommands);

            // Генерация случайных индексов для команд IO
            HashSet<int> ioIndices = new HashSet<int>();
            while (ioIndices.Count < ioCommandCount)
            {
                int randomIndex = rnd.Next(1, countCommands - 1); // Исключаем последний индекс (последняя команда - Close)
                ioIndices.Add(randomIndex);  // HashSet предотвращает дублирование индексов
            }

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
                if (ioIndices.Contains(i))
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

            foreach (var cmd in Commands)
            {
                AllTime += cmd.TypeCmd.timeTypeCommand;
            }

            State = ProcessState.Ready;
        }
        public MyProcess(int countCom)
        {
            Commands = new ObservableCollection<Command>();
            Id = _count;
            countCommands = countCom;
            CountAllCommands = countCommands;
            int ioCommandCount = (int)((double)DInOut / 100 * countCommands);


            // Генерация случайных индексов для команд IO
            HashSet<int> ioIndices = new HashSet<int>();
            while (ioIndices.Count < ioCommandCount)
            {
                int randomIndex = rnd.Next(1, countCommands - 1); // Исключаем последний индекс (последняя команда - Close)
                ioIndices.Add(randomIndex);  // HashSet предотвращает дублирование индексов
            }

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
                if (ioIndices.Contains(i))
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

            foreach (var cmd in Commands)
            {
                AllTime += cmd.TypeCmd.timeTypeCommand;
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
