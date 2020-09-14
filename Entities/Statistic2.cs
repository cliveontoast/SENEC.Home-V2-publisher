namespace Entities
{
    public struct Statistic
    {
        public decimal Minimum;
        public decimal Maximum;
        public decimal Median;
        public decimal Average;
        public bool IsValid { get; }

        public Statistic(bool isValid)
        {
            IsValid = isValid;
            Minimum = 0;
            Maximum = 0;
            Median = 0;
            Average = 0;
        }
        public Statistic(decimal minimum, decimal maximum, decimal median, decimal average)
        {
            IsValid = true;
            Minimum = minimum;
            Maximum = maximum;
            Median = median;
            Average = average;
        }

    }
}


