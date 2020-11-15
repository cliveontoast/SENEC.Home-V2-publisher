import { Component, OnInit } from '@angular/core';
import { EnergySummaryService } from '../services/energySummary.service';
import * as Highcharts from 'highcharts';
import { InitialDateService } from '../services/initial-date.service';
import { take, tap, switchMap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { DailyEnergySummaryDto } from '../dto/dailyEnergySummaryDto';

@Component({
  selector: 'app-energy',
  templateUrl: './energy.component.html',
  styleUrls: ['./energy.component.css']
})
export class EnergyComponent implements OnInit {
  Highcharts: typeof Highcharts = Highcharts; // required
  chartConstructor: string = 'chart'; // optional string, defaults to 'chart'
  chartOptions: Highcharts.Options = {
    title: { text: 'Battery capacity over the day' },
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
          text: 'Battery capacity'
      },
      plotBands: [
        {
          from: 100,
          to: 10000,
          color: 'rgba(255, 0, 0, 0.1)',
          label: {
              text: 'Far too high',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: 95,
          to: 100,
          color: 'rgba(130, 130, 0, 0.1)',
          label: {
              text: 'High',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: 5,
          to: 95,
          color: 'rgba(0, 0, 0, 0)',
          label: {
              text: 'Good',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: 0,
          to: 5,
          color: 'rgba(0, 170, 120, 0.1)',
          label: {
              text: 'Low',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: -10000,
          to: 0,
          color: 'rgba(255, 0, 0, 0.1)',
          label: {
              text: 'Far too low',
              style: {
                  color: '#606060'
              }
          }
        }
      ]
    },
    series: [
      {
        type: 'area',
        name: 'Battery Percentage'
      }
    ],
    tooltip: {
      split: true,
      // valueSuffix: ' watt'
    },
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
      },
      area: {
        stacking: 'normal',
        lineColor: '#666666',
        lineWidth: 0,
        marker: {
            lineWidth: 4,
            lineColor: '#666666'
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


  constructor(private energySummaryService: EnergySummaryService,
    private initialDateService: InitialDateService) { }

  ngOnInit() {
    this.initialDateService.today.pipe(
      take(1),
      tap(s => this.displayDate = s),
      switchMap(s => this.getData()),
      tap(value => this.applyData(value))
    ).subscribe();
  }

  private getData(): Observable<DailyEnergySummaryDto> {
    return this.energySummaryService.get(this.displayDate);
  }
  private applyData(value: DailyEnergySummaryDto) {
      this.chartOptions.plotOptions.spline.pointStart = Date.UTC(2019, 5, 19, 0, 0, 0);
      this.chartOptions.series[0]['data'] = value.batteryCapacity.data;
    this.updateFlag = true;
  }

  previousDay() {
    var dte = new Date(this.displayDate.toUTCString());
    dte.setDate(dte.getDate()-1);
    this.displayDate = dte;
    this.getData().subscribe(a => this.applyData(a));
  }

  nextDay() {
    var dte = new Date(this.displayDate.toUTCString());
    dte.setDate(dte.getDate()+1);
    this.displayDate = dte;
    this.getData().subscribe(a => this.applyData(a));
  }
}
