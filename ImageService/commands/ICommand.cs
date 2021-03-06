﻿using ImageService.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    public interface ICommand
    {
        /// <summary>
        /// Executes the new specific command.
        /// </summary>
        string Execute(string[] args, out bool result);          // The Function That will Execute The 
    }
}
