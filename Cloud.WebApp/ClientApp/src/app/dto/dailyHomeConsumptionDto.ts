export interface DailyHomeConsumptionDto {
  date: number;
  toHome: HomeConsumptionDto;
  fromGrid: HomeConsumptionDto;
  fromGridToBattery: HomeConsumptionDto;
  fromSolar: HomeConsumptionDto;
  fromBattery: HomeConsumptionDto;
  toBattery: HomeConsumptionDto;
  toCommunity: HomeConsumptionDto;
}

export interface HomeConsumptionDto {
  label: string;
  data: number[];
}
