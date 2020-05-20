export interface DailyVoltageSummaryDto {
  date: number;
  phases: PhaseDto[];
}

export interface PhaseDto {
  label: string;
  data: number[];
}
