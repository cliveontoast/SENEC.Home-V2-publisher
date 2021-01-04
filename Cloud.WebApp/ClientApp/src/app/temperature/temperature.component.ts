import { Component } from '@angular/core';
import { TemperatureSummaryService } from '../services/temperatureSummary.service';
import * as Highcharts from 'highcharts';
import { InitialDateService } from '../services/initial-date.service';
import { take, tap, switchMap } from 'rxjs/operators';
import { DailyTemperatureSummaryDto } from '../dto/dailyTemperatureSummaryDto';
import { Observable, combineLatest } from 'rxjs';

@Component({
  selector: 'app-temperatures',
  templateUrl: './temperature.component.html'
})
export class TemperaturesComponent {

  Highcharts: typeof Highcharts = Highcharts; // required
  chartConstructor: string = 'chart'; // optional string, defaults to 'chart'

  chartOptions: Highcharts.Options = {
    title: { text: 'Temperatures of each sensor over the day' },
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
          text: 'Temperature'
      },
      plotBands: [
        {
          from: 80,
          to: 500,
          color: 'rgba(255, 0, 0, 0.1)',
          label: {
              text: 'Far too high',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: 60,
          to: 80,
          color: 'rgba(130, 130, 0, 0.1)',
          label: {
              text: 'Too high',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: 20,
          to: 60,
          color: 'rgba(0, 0, 0, 0)',
          label: {
              text: 'Good',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: -100,
          to: 20,
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
        type: 'spline',
        name: 'T1'
      },
      {
        type: 'spline',
        name: 'T2'
      },
      {
        type: 'spline',
        name: 'T3'
      },
      {
        type: 'spline',
        name: 'T4'
      },
      {
        type: 'spline',
        name: 'T5'
      }
    ],
    plotOptions: {
      series: {
          marker: {
              radius: 0
          },
      },
      spline: {
        lineWidth: 4,
        states: {
            hover: {
                lineWidth: 5
            }
        },
        marker: {
            enabled: false
        },
        pointInterval: 3600000/12, // one hour / 12 = 5 min
        pointStart: Date.UTC(2018, 1, 13, 0, 0, 0)
      }
    }
  }; // required

  
  historyOptions: Highcharts.Options = {
    title: { text: 'Temperatures T4 and T5 compared to yesterday' },
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
          text: 'Temperature'
      },
      plotBands: [
        {
          from: 80,
          to: 500,
          color: 'rgba(255, 0, 0, 0.1)',
          label: {
              text: 'Far too high',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: 60,
          to: 80,
          color: 'rgba(130, 130, 0, 0.1)',
          label: {
              text: 'Too high',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: 20,
          to: 60,
          color: 'rgba(0, 0, 0, 0)',
          label: {
              text: 'Good',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: -100,
          to: 20,
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
        type: 'spline',
        name: 'T4'
      },
      {
        type: 'spline',
        name: 'T5'
      },
      {
        type: 'spline',
        name: 'T4 yesterday'
      },
      {
        type: 'spline',
        name: 'T5 yesterday'
      }
    ],
    plotOptions: {
      series: {
          marker: {
              radius: 0
          },
      },
      spline: {
        lineWidth: 4,
        states: {
            hover: {
                lineWidth: 5
            }
        },
        marker: {
            enabled: false
        },
        pointInterval: 3600000/12, // one hour / 12 = 5 min
        pointStart: Date.UTC(2018, 1, 13, 0, 0, 0)
      }
    }
  }; // required


  chartCallback: Highcharts.ChartCallbackFunction = function (chart) {  }; // optional function, defaults to null
  updateFlag: boolean = true; // optional boolean
  oneToOneFlag: boolean = true; // optional boolean, defaults to false
  runOutsideAngularFlag: boolean = false; // optional boolean, defaults to false

  displayDate: Date = new Date();


  constructor(private temperatureSummaryService: TemperatureSummaryService,
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

  private getData(): Observable<DailyTemperatureSummaryDto[]> {
    return combineLatest(
      this.temperatureSummaryService.get(this.displayDate),
      this.temperatureSummaryService.get(this.getDay(-1))
    );
  }

  private applyData(values: DailyTemperatureSummaryDto[]){
    const value = values[0];
    const yesterday = values[1];
    this.chartOptions.plotOptions.spline.pointStart = Date.UTC(2019, 5, 19, 0, 0, 0);
    this.chartOptions.series[0]['data'] = value.temps[0].data;
    this.chartOptions.series[1]['data'] = value.temps[1].data;
    this.chartOptions.series[2]['data'] = value.temps[2].data;
    this.chartOptions.series[3]['data'] = value.temps[3].data;
    this.chartOptions.series[4]['data'] = value.temps[4].data;

    this.historyOptions.plotOptions.spline.pointStart = Date.UTC(2019,5,19);
    this.historyOptions.series[0]['data'] = value.temps[3].data;
    this.historyOptions.series[1]['data'] = value.temps[4].data;
    this.historyOptions.series[2]['data'] = yesterday.temps[3].data;
    this.historyOptions.series[3]['data'] = yesterday.temps[4].data;

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
