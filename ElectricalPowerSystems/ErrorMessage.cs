using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
    public class ErrorMessage
    {
        string message;
        int line;
        int position;
        public string Message
        {
            get { return message; }
        }
        public string Line
        {
            get
            {
                return line == -1 ? "" : line.ToString();
            }
        }
        public string Position
        {
            get
            {
                return position == -1 ? "" : position.ToString();
            }
        }
        public string Error
        {
            get
            {
                return Message + (position == -1?"":$" position: {position.ToString()}") +(line == -1 ? "" : $" line: {line.ToString()}");
            }
        }
        public ErrorMessage(string message, int line = -1, int position = -1)
        {
            this.message = message;
            this.line = line;
            this.position = position;
        }
    }
}
