using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSMud
{
    public class Thing : IDisposable
    {
        private List<string> Commands { get; set; }
        private int Id { get; set; }
        private string Name { get; set; }
        private string Description { get; set; }

        public Thing(List<string> commands, string name, string description, int id)
        {
            this.Commands = commands;
            this.Name = name;
            this.Description = description;
            this.Id = id;
        }

        ~Thing()
        {
            this.Dispose();
        }

        public void Dispose() { }
    }
}
