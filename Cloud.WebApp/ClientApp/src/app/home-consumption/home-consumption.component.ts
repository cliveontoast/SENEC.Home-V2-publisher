import { Component, OnInit } from '@angular/core';
import * as Highcharts from 'highcharts';
import { HomeConsumptionService } from '../services/home-consumption.service';
import { InitialDateService } from '../services/initial-date.service';
import { take, tap, switchMap } from 'rxjs/operators';
import { DailyHomeConsumptionDto } from '../dto/dailyHomeConsumptionDto';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-home-consumption',
  templateUrl: './home-consumption.component.html'
})
export class HomeConsumptionComponent implements OnInit {

  Highcharts: typeof Highcharts = Highcharts; // required
  chartConstructor: string = 'chart'; // optional string, defaults to 'chart'
  chartOptions: Highcharts.Options = {
    title: { text: 'Energy use by source' },
    chart: {
      // type: 'area',
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
          text: 'Total watts'
      },
      plotBands: [
        {
          from: 6000,
          to: 20000,
          color: 'rgba(255, 0, 0, 0.1)',
          label: {
              text: 'Far too high',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: 3000,
          to: 6000,
          color: 'rgba(130, 130, 0, 0.1)',
          label: {
              text: 'Too high',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: 0,
          to: 3000,
          color: 'rgba(0, 0, 0, 0)',
          label: {
              text: 'Good',
              style: {
                  color: '#606060'
              }
          }
        }, {
          from: -20000,
          to: 0,
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
        type: 'area',
        color: '#DCE072',
        lineColor: 'black',
        lineWidth: 1,
        name: 'Grid to battery'
      },
      {
        type: 'area',
        color: '#FFD145',
        lineColor: '#C4A136',
        lineWidth: 1,
        name: 'Sun to battery'
      },
      {
        type: 'area',
        color: '#C2E0A1',
        lineColor: '#47A530',
        lineWidth: 1,
        name: 'Sun to community'
      },
      {
        type: 'area',
        color: 'lightblue',
        lineColor: '#60DAFF',
        lineWidth: 1,
        name: 'Battery to home'
      },
      {
        type: 'area',
        color: '#5DDD40',
        lineColor: '#47B72C',
        lineWidth: 1,
        name: 'Sun to home'
      },
      {
        type: 'area',
        color: 'gray',
        lineColor: 'black',
        lineWidth: 1,
        name: 'From grid (fossil fuels)'
      },
      {
        type: 'spline',
        color: 'red',
        name: 'Consumption'
      },
    ],
    tooltip: {
      split: true,
      valueSuffix: ' watts'
    },
    plotOptions: {
      series: {
          marker: {
              radius: 0
          },
      },
      spline: {
        lineWidth: 2,
        states: {
            hover: {
                lineWidth: 2
            }
        },
        marker: {
            enabled: false
        },
        pointInterval: 3600000/12, // one hour / 12 = 5 min
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
      }
    }
  }; // required
  chartCallback: Highcharts.ChartCallbackFunction = function (chart) {  }; // optional function, defaults to null
  updateFlag: boolean = true; // optional boolean
  oneToOneFlag: boolean = true; // optional boolean, defaults to false
  runOutsideAngularFlag: boolean = false; // optional boolean, defaults to false

  displayDate: Date = new Date();


  constructor(private homeConsumptionService: HomeConsumptionService,
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

  private getData(): Observable<DailyHomeConsumptionDto> {
    return this.homeConsumptionService.get(this.displayDate);
  }
  
  private applyData(value: DailyHomeConsumptionDto) {
    const date = Date.UTC(
      this.displayDate.getFullYear(),
      this.displayDate.getMonth(),
      this.displayDate.getDate());
    this.chartOptions.plotOptions.area.pointStart = date; 
    this.chartOptions.plotOptions.spline.pointStart = date;
    this.chartOptions.series[0]['data'] = value.fromGridToBattery.data;
    this.chartOptions.series[1]['data'] = value.toBattery.data;
    this.chartOptions.series[2]['data'] = value.toCommunity.data;
    this.chartOptions.series[3]['data'] = value.fromBattery.data;
    this.chartOptions.series[4]['data'] = value.fromSolar.data;
    this.chartOptions.series[5]['data'] = value.fromGrid.data;
    this.chartOptions.series[6]['data'] = value.toHome.data;
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
