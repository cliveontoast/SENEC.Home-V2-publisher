import { HomeConsumptionDto } from "./dailyHomeConsumptionDto";

export interface PowerMovementDto {
  date: number;
  toHome: HomeConsumptionDto;
  fromGridToHome: HomeConsumptionDto;
  fromGridToBattery: HomeConsumptionDto;
  fromBatteryToHome: HomeConsumptionDto;
  fromBatteryToHomeNeg: HomeConsumptionDto;
  fromBatteryToCommunity: HomeConsumptionDto;
  fromBatteryToCommunityNeg: HomeConsumptionDto;
  fromSunToBattery: HomeConsumptionDto;
  fromSunToGrid: HomeConsumptionDto;
  fromSunToHome: HomeConsumptionDto;
}
