using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OperationSystem_ModelApp.Model
{
    public class Command
    {
        private int id;
        private Random rnd;
        public TypeCommand TypeCmd { get;  set; }
        public int Id
        {
            get => id;
            set
            {
                id++;
            }
        }
        private bool _islast;

        [JsonConstructor]
        public Command(int i, TypeCommand type)
        {
            Id = i;
            TypeCmd = type;
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
    public enum NameTypeCommand { Close, IO, Arithmetic, IDK }

    public struct TypeCommand
    {
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
            nameTypeCommand = NameTypeCommand.IDK;
            timeTypeCommand = 10;
            sizeTypeCommand = 10;
        }
    }
}
