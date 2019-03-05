﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSMud
{
    public class Entity
    {
        private List<string> Commands { get; set; }
        private string Id { get; set; }
        private string Name { get; set; }
        private string Description { get; set; }

        public Entity(List<string> commands, string name, string description, string id)
        {
            this.Commands = commands;
            this.Name = name;
            this.Description = description;
            this.Id = id;
        }
    }
}
