using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OperationSystem_ModelApp.Model
{
    public class Command
    {
        private Random rnd;
        public TypeCommand TypeCmd { get;  set; }

        private bool _islast;
        public Command() {  }
        public Command(TypeCommand typeCommand)
        {
            TypeCmd= typeCommand;
        }
        public Command(bool islast, bool isIO=false)
        {
            rnd = new Random();
            _islast = islast;
            if (!_islast)
            {
                if (isIO)
                {
                    TypeCmd = new TypeCommand(NameTypeCommand.IO);
                }
                else
                {
                    //Рандомная задача
                    var rndType = (NameTypeCommand)rnd.Next(2, Enum.GetValues(typeof(NameTypeCommand)).Length);
                    TypeCmd = new TypeCommand(rndType);
                }
            }
            else
            {
                TypeCmd = new TypeCommand(NameTypeCommand.Close);
            }
        }
    }
    public enum NameTypeCommand { Close, IO, Arithmetic }

    public class TypeCommand
    {
        public NameTypeCommand nameTypeCommand { get; set; }
        public int timeTypeCommand { get; set; }
        public int sizeTypeCommand { get; set; }

        public TypeCommand(NameTypeCommand name)
        {
            nameTypeCommand = name;
            switch (nameTypeCommand)
            {
                case NameTypeCommand.Close:
                    timeTypeCommand = 0;
                    sizeTypeCommand = 0;
                    break;

                case NameTypeCommand.IO:
                    timeTypeCommand = 50;
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
            //Если надо посмотреть как генерируется
            Debug.WriteLine($"Command {nameTypeCommand}: Time:{timeTypeCommand} Size:{sizeTypeCommand}");
        }

        public TypeCommand()
        {
           switch (nameTypeCommand)
            {
                case NameTypeCommand.Close:
                    timeTypeCommand = 0;
                    sizeTypeCommand = 0;
                    break;

                case NameTypeCommand.IO:
                    timeTypeCommand = 50;
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

        public TypeCommand(NameTypeCommand nameTypeCommand, int timeTypeCommand, int sizeTypeCommand)
        {
            this.nameTypeCommand = nameTypeCommand;
            this.timeTypeCommand = timeTypeCommand;
            this.sizeTypeCommand = sizeTypeCommand;
        }
    }
}
