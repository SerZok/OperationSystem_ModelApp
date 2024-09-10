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
            countCommands = rnd.Next(2, 10); //Максммум 10 комманд

            for (int i = 0; i < countCommands; i++)
            {
                if (i==countCommands)
                {
                    var commandLast = new Command(true);
                    Commands.Add(commandLast);
                    break;
                }
                var command = new Command(false);
                Commands.Add(command);
            }
            //MessageBox.Show($"Command #{Id}: Ram:{Ram}");
        }
    }

    public class Command
    {
        private int id;
        private Random rnd;
        public TypeCommand TypeCmd { get; private set; }
        public int Id
        {
            get => id;
            set 
            {
                id++;
            }
        }
        private bool _islast;
        public Command(bool islast)
        {
            rnd = new Random();
            _islast = islast;
            if (!_islast)
            {
                TypeCmd = new TypeCommand(Enum.);
            }
            else
            {
                TypeCmd = new TypeCommand(NameTypeCommand.Close);
            }
        }
    }
    public enum ProcessState
    {
        Ready,
        Running,
        Completed
    }
    public enum NameTypeCommand {Close, IO, Arithmetic}
    public struct TypeCommand{
        public NameTypeCommand nameTypeCommand;
        public int timeTypeCommand;
        public int sizeTypeCommand;

        public TypeCommand(NameTypeCommand name)
        {
            nameTypeCommand = name;
            switch (nameTypeCommand)
            {
                case NameTypeCommand.Close:
                    timeTypeCommand = 0;
                    sizeTypeCommand = 1;
                    break;

                case NameTypeCommand.IO:
                    timeTypeCommand = 6;
                    sizeTypeCommand = 5;
                    break;

                case NameTypeCommand.Arithmetic:
                    timeTypeCommand = 4;
                    sizeTypeCommand = 4;
                    break;

                default:
                    timeTypeCommand = 1;
                    sizeTypeCommand = 1;
                    break;
            }
        }
        public TypeCommand()
        {
            nameTypeCommand = "DefaultType";
            timeTypeCommand = 0;
            sizeTypeCommand = 0;
        }
    }
}
