import { Component } from '@angular/core';
import { VoltageSummaryService } from '../services/voltageSummary.service';
import * as Highcharts from 'highcharts';
import { InitialDateService } from '../services/initial-date.service';
import { take, tap, switchMap } from 'rxjs/operators';
import { DailyVoltageMomentDto } from '../dto/dailyVoltageMomentDto';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-voltages',
  templateUrl: './high-precision-voltages.component.html'
})
export class HighPrecisionVoltagesComponent {

  Highcharts: typeof Highcharts = Highcharts; // required
  chartConstructor: string = 'chart'; // optional string, defaults to 'chart'
  chartOptions: Highcharts.Options = {
    title: { text: 'HP voltages of each phase over the day' },
    chart: {
      zoomType: 'x'
    },
    xAxis: {
      type: 'datetime',
      labels: {
          overflow: 'justify'
      }
    },
    yAxis: {
      title: {
          text: 'Voltage'
      },
      plotBands: [
        {
          from: 258,
          to: 400,
          color: 'rgba(255, 0, 0, 0.1)',
          label: {
              text: 'Far too high',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: 255,
          to: 258,
          color: 'rgba(130, 130, 0, 0.1)',
          label: {
              text: 'Too high',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: 220,
          to: 255,
          color: 'rgba(0, 0, 0, 0)',
          label: {
              text: 'Good',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: 0,
          to: 220,
          color: 'rgba(0, 170, 120, 0.1)',
          label: {
              text: 'Too low',
              style: {
                  color: '#606060'
              }
          }
        }
      ]
    },
    series: [
      {
        type: 'line',
        name: 'L1'
      },
      {
        type: 'line',
        name: 'L2'
      },
      {
        type: 'line',
        name: 'L3'
      }
    ],
    plotOptions: {
      series: {
          marker: {
              radius: 0
          },
      },
      spline: {
        lineWidth: 1,
        states: {
            hover: {
                lineWidth: 2
            }
        },
        marker: {
            enabled: false
        },
        pointInterval: 3600000/60/60, // one hour / 60 minutes / 60 seconds
        pointStart: Date.UTC(2018, 1, 13, 0, 0, 0)
      }
    }
  }; // required
  chartCallback: Highcharts.ChartCallbackFunction = function (chart) {  }; // optional function, defaults to null
  updateFlag: boolean = true; // optional boolean
  oneToOneFlag: boolean = true; // optional boolean, defaults to false
  runOutsideAngularFlag: boolean = false; // optional boolean, defaults to false

  displayDate: Date = new Date();


  constructor(private voltageSummaryService: VoltageSummaryService,
    private initialDateService: InitialDateService) { }

  ngOnInit() {
    this.initialDateService.today.pipe(
      take(1),
      tap(s => this.displayDate = s),
      switchMap(s => this.refresh$)
    ).subscribe();
    this.initialDateService.refresh$.pipe(
      switchMap(() => this.refresh$)
    ).subscribe();
  }

  get refresh$(): Observable<any> {
    return this.getData().pipe(
      tap(a => this.applyData(a))
    );
  }

  refresh() {
    this.refresh$.subscribe();
  }

  private getData(): Observable<DailyVoltageMomentDto> {
    return this.voltageSummaryService.getHighPrecision(this.displayDate);
  }

  private applyData(value: DailyVoltageMomentDto){
    this.chartOptions.plotOptions.spline.pointStart = Date.UTC(2019, 5, 19, 0, 0, 0);
    this.chartOptions.series[0].name = value.phases[0].label;
    this.chartOptions.series[1].name = value.phases[1].label;
    this.chartOptions.series[2].name = value.phases[2].label;
    this.chartOptions.series[0]['data'] = value.phases[0].data;
    this.chartOptions.series[1]['data'] = value.phases[1].data;
    this.chartOptions.series[2]['data'] = value.phases[2].data;
    this.updateFlag = true;
  }

  getDay(addDays: number): Date {
    var dte = new Date(this.displayDate.toUTCString());
    dte.setDate(dte.getDate()+addDays);
    return dte;
  }

  previousDay() {
    this.displayDate = this.getDay(-1);
    this.refresh();
  }

  nextDay() {
    this.displayDate = this.getDay(+1);
    this.refresh();
  }
}
