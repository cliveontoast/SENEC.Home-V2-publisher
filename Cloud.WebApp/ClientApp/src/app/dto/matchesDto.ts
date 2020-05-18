export interface DailyVoltageSummaryDto {
  date: Date;
  phases: PhaseDto[];
  xLabels: string[];
}

export interface PhaseDto {
  label: string;
  data: number[];
}
