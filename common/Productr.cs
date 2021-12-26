using System;

namespace winformasync.common
{

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double WeightTon { get; set; }

        public override string ToString()
        {
            return $"id:{Id}, name:{Name}, weight:{WeightTon:0.0}";
        }
    }
}