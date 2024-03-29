﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartDietCapstone
{
    public class Food
    {
        public Food() { }
        public Food(double servingSize, double cals, double protein, double fat, double carbs)
        {
            this.servingSize = Math.Round(servingSize,2);
            this.cals = Math.Round(cals,2);
            this.protein = Math.Round(protein,2);
            this.carbs = Math.Round(carbs,2);
            this.fat = Math.Round(fat,2);
        }
        public int fdcId { get; set; }
        public double servingSize { get; set; }
        public string name { get; set; }
        public double cals { get; set; }
        public double protein { get; set; }
        public double carbs { get; set; }
        public double fat { get; set; }

        public string category { get; set; }
    }
}
