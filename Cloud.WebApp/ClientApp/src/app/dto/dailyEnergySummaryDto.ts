export interface DailyEnergySummaryDto {
  date: number;
  batteryCapacity: BatteryCapacityDto;
}

export interface BatteryCapacityDto {
  label: string;
  data: number[];
}
