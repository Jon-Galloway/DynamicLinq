﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredSoft.DynamicLinq.Fluent
{
    public class OrderByPart
    {
        public string Path { get; set; }

        public QueryOrderByDirection Direction { get; set; } = QueryOrderByDirection.Ascending;

        public bool Append { get; set; }
    }
}
