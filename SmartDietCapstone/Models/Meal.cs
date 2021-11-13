using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartDietCapstone.Models
{
    public class Meal
    {
        public double totalCals;
        public double totalProtein;
        public double totalCarbs;
        public double totalFat;

        public List<Food> foods;
        public Meal() => foods = new List<Food>();


        public void AddFood(Food food) {
            foods.Add(food);
            totalCals += Math.Round(food.cals,2);
            totalCarbs += Math.Round(food.carbs,2);
            totalProtein += Math.Round(food.protein,2);
            totalFat += Math.Round(food.fat,2);
        }
        

    }
}
