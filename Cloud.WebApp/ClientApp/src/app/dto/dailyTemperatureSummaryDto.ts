export interface DailyTemperatureSummaryDto {
  date: number;
  temps: TemperatureDto[];
}

export interface TemperatureDto {
  label: string;
  data: number[];
}
