export interface DailyHomeConsumptionDto {
  date: number;
  toHome: HomeConsumptionDto;
  fromGrid: HomeConsumptionDto;
  fromGridToBattery: HomeConsumptionDto;
  fromSolar: HomeConsumptionDto;
  fromBattery: HomeConsumptionDto;
  toBattery: HomeConsumptionDto;
  toCommunity: HomeConsumptionDto;
  moneyPlans: MoneyPlanDto[];
}

export interface HomeConsumptionDto {
  label: string;
  data: number[];
}

export interface MoneyPlanDto {
  name: string;
  cost: number;
}
