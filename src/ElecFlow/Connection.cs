using System;
using System.Collections.Generic;
using System.Text;

namespace ElecFlow
{
    public class Connection
    {
        public string Name { get; set; }

        public OutputConnector From { get; }

        public InputConnector To { get; }

        public Connection(OutputConnector from, InputConnector to)
        {
            From = from;
            To = to;
        }
    }
}
