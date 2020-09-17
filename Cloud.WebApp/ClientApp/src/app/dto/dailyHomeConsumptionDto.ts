export interface DailyHomeConsumptionDto {
  date: number;
  toHome: HomeConsumptionDto;
  fromGrid: HomeConsumptionDto;
  fromSolar: HomeConsumptionDto;
  fromBattery: HomeConsumptionDto;
}

export interface HomeConsumptionDto {
  label: string;
  data: number[];
}
