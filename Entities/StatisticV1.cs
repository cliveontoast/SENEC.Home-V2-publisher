namespace Entities
{
    public struct StatisticV1
    {
        public decimal Minimum;
        public decimal Maximum;
        public decimal Median;
        public int Failures;

        public StatisticV1(int failures)
        {
            Failures = failures;
            Minimum = 0;
            Maximum = 0;
            Median = 0;
        }
        public StatisticV1(decimal minimum, decimal maximum, decimal median, int failures)
        {
            Minimum = minimum;
            Maximum = maximum;
            Median = median;
            Failures = failures;
        }
    }
}


