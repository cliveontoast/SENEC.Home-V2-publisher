using System.Collections.Generic;

namespace SenecEntities
{
    public class LalaResponseContent : WebResponse
    {
        public LalaResponseContent(): base(default, default)
        {
        }

        /// <summary>
        /// Grid meter
        /// </summary>
        public Meter? PM1OBJ1 { get; set; }
        /// <summary>
        /// Production
        /// </summary>
        public Meter? PM1OBJ2 { get; set; }
        public RealTimeClock? RTC { get; set; }
        public Statistic? STATISTIC { get; set; }
        public Energy? ENERGY { get; set; }
        public Wizard? WIZARD { get; set; }
        public Log? LOG { get; set; }
        public Features? FEATURES { get; set; }
        public Bms? BMS { get; set; }
        public SysUpdate? SYS_UPDATE { get; set; }
        public BatteryObject1? BAT1OBJ1 { get; set; }
        public TemperatureMeasure? TEMPMEASURE { get; set; }
    }

    public class BatteryObject1
    {
        public string? TEMP1 { get; set; }
        public string? TEMP2 { get; set; }
        public string? TEMP3 { get; set; }
        public string? TEMP4 { get; set; }
        public string? TEMP5 { get; set; }
        public string? S { get; set; }
        public string? P { get; set; }
        public string? Q { get; set; }
        public string? SW_VERSION { get; set; }
        public string? SW_VERSION2 { get; set; }
        public string? SW_VERSION3 { get; set; }
        public string? I_DC { get; set; }
    }

    public class TemperatureMeasure
    {
        public string? BATTERY_TEMP { get; set; }
        public string? CASE_TEMP { get; set; }
    }

    public class Meter
    {
        public string? FREQ { get; set; }
        public List<string?>? U_AC { get; set; } // L1 L2 L3
        public List<string?>? I_AC { get; set; } // L1 L2 L3
        public List<string?>? P_AC { get; set; } // L1 L2 L3
        public string? P_TOTAL { get; set; }
    }

    public class SysUpdate
    {
        public string? UPDATE_AVAILABLE { get; set; }
    }

    public class Bms
    {
        public string? MODULE_COUNT { get; set; }
        public string? MODULES_CONFIGURED { get; set; }
    }

    public class Features
    {
    }

    public class Log
    {
        public string? USER_LEVEL { get; set; }
        public string? USERNAME { get; set; }
    }

    public class Wizard
    {
        public string? GUI_LANG { get; set; }
        public string? FEATURECODE_ENTERED { get; set; }
        public string? CONFIG_LOADED { get; set; }
        public string? SETUP_NUMBER_WALLBOXES { get; set; }
        public string? SETUP_WALLBOX_SERIAL0 { get; set; }
        public string? SETUP_WALLBOX_SERIAL1 { get; set; }
        public string? SETUP_WALLBOX_SERIAL2 { get; set; }
        public string? SETUP_WALLBOX_SERIAL3 { get; set; }
    }

    /// <summary>
    /// Invalid for V2 in my house
    /// </summary>
    public class Statistic
    {
        public string? STAT_DAY_E_HOUSE { get; set; } // kWh - House Energy Day
        public string? STAT_DAY_E_PV { get; set; } // kWh - PV Production Day
        public string? STAT_DAY_BAT_CHARGE { get; set; } // kWh - Battery charge day
        public string? STAT_DAY_BAT_DISCHARGE { get; set; } // kWh - Battery discharge day
        public string? STAT_DAY_E_GRID_IMPORT { get; set; } // kWh - Grid Import day
        public string? STAT_DAY_E_GRID_EXPORT { get; set; } // kWh - Grid Export day
        public string? STAT_YEAR_E_PU1_ARR { get; set; }
    }

    public class Energy
    {
        public string? STAT_LICENSCE_IS_OK { get; set; } // variable not available
        public string? STAT_STATE { get; set; }
        public string? STAT_STATE_DECODE { get; set; }
        public string? GUI_BAT_DATA_POWER { get; set; } // in Watts, stored in kW --> negative = draining - Battery load current
        public string? GUI_INVERTER_POWER { get; set; } // Power generation current
        public string? GUI_HOUSE_POW { get; set; } // Power usage house current
        public string? GUI_GRID_POW { get; set; } // value in Watts, stored in kW) --> negative = export into net - Power grid current
        public string? STAT_MAINT_REQUIRED { get; set; }
        public string? MAINT_FILTER_REQUIRED { get; set; } // variable not available
        public string? GUI_BAT_DATA_FUEL_CHARGE { get; set; } // percentage - Battery Level
        public string? GUI_CHARGING_INFO { get; set; }
        public string? GUI_BOOSTING_INFO { get; set; }
    }
}
