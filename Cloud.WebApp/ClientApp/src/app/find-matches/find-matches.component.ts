import { Component, OnInit, ViewChild } from '@angular/core';
import { ChartDataSets, ChartOptions } from 'chart.js';
import { Color, Label } from 'ng2-charts';
import { SubtextMatchService } from '../services/subtextMatch.service';

@Component({
  selector: 'app-find-matches',
  templateUrl: './find-matches.component.html'
})
export class FindMatchesComponent {
  public lineChartData: ChartDataSets[] = [
    { data: [65, 59, 80, 81, 56, 55, 40], label: 'Series A' },
    { data: [65, 56, 55, 40, 59, 80, 81], label: 'Series B' },
  ];
  public lineChartLabels: Label[] = ['January', 'February', 'March', 'April', 'May', 'June', 'July'];
  public lineChartOptions: (ChartOptions & { annotation: any }) = {
    responsive: true,
    annotation: undefined,
  };
  public lineChartColors: Color[] = [
    {
      borderColor: 'black',
      backgroundColor: 'rgba(255,0,0,0.3)',
    },
  ];
  public lineChartLegend = true;
  public lineChartType = 'line';
  public lineChartPlugins = [];

  constructor(private subtextMatchService: SubtextMatchService) { }

  ngOnInit() {
    this.subtextMatchService.get().subscribe(
      value => {
        this.lineChartData = value.phases;
        this.lineChartLabels = value.xLabels;
      });
  }
}
