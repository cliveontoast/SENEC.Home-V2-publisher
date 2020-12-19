export interface DailyEquipmentStatesSummaryDto {
  date: number;
  states: EquipmentStateDto[];
}

export interface EquipmentStateDto {
  name: string;
  data: number[];
}
