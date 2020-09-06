namespace Entities
{
    public struct Statistic
    {
        public decimal Minimum;
        public decimal Maximum;
        public decimal Median;
        public int Failures;

        public Statistic(int failures)
        {
            Failures = failures;
            Minimum = 0;
            Maximum = 0;
            Median = 0;
        }
        public Statistic(decimal minimum, decimal maximum, decimal median, int failures)
        {
            Minimum = minimum;
            Maximum = maximum;
            Median = median;
            Failures = failures;
        }
    }
}


