using System;

namespace TeslaEntities
{
    public class MetersAggregates : WebResponse
    {
        public MetersAggregates() : base(default, default)
        {
        }

        /// <summary>
        /// Grid
        /// </summary>
        public MASite? site { get; set; }
        public MABattery? battery { get; set; }
        public MALoad? load { get; set; }
        public MASolar? solar { get; set; }
        // zeros
        public MABusway? busway { get; set; }
        // zeros
        public MAFrequency? frequency { get; set; }
        // zeros
        public MAGenerator? generator { get; set; }
    }

    public class MeterAggregate
    {
        public DateTimeOffset last_communication_time { get; set; }
        public decimal instant_power { get; set; }
        public decimal instant_reactive_power { get; set; }
        public decimal instant_apparent_power { get; set; }
        public decimal frequency { get; set; }
        public decimal energy_exported { get; set; }
        public decimal energy_imported { get; set; }
        public decimal instant_average_voltage { get; set; }
        public decimal instant_total_current { get; set; }
        public decimal i_a_current { get; set; }
        public decimal i_b_current { get; set; }
        public decimal i_c_current{ get; set; }
    }

    public class MAGenerator : MeterAggregate
    {
    }

    public class MAFrequency : MeterAggregate
    {
    }

    public class MABusway : MeterAggregate
    {
    }

    public class MASolar : MeterAggregate
    {
    }

    public class MALoad : MeterAggregate
    {
    }

    public class MABattery : MeterAggregate
    {
    }

    public class MASite : MeterAggregate
    {
    }
}
