using DataClasses;
using Learning.Tactics;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Learning
{
    partial class Tactician
    {
        private Strategist Strategist { get; init; }
        private MarkovGraph MarkovGraph { get; set; }

        public Tactician(Strategist strat)
        {
            Strategist = strat;
        }
}
}
