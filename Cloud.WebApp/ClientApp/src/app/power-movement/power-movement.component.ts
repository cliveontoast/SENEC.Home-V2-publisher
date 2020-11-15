import { Component, OnInit } from '@angular/core';
import { PowerMovementService } from '../services/power-movement.service';
import * as Highcharts from 'highcharts';
import { InitialDateService } from '../services/initial-date.service';
import { take, tap, switchMap } from 'rxjs/operators';
import { PowerMovementDto } from '../dto/dailyPowerMovementDto';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-power-movement',
  templateUrl: './power-movement.component.html',
  styleUrls: ['./power-movement.component.css']
})
export class PowerMovementComponent implements OnInit {

  Highcharts: typeof Highcharts = Highcharts; // required
  chartConstructor: string = 'chart'; // optional string, defaults to 'chart'

  solarChartOptions: Highcharts.Options = {
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
          text: 'Total Watts'
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
        color: '#C2E0A1',
        lineColor: '#47A530',
        lineWidth: 1,
        name: 'Sun to community'
      },
      {
        type: 'area',
        color: '#FFD145',
        lineColor: '#C4A136',
        lineWidth: 1,
        name: 'Sun to battery'
      },
      // {
      //   type: 'area',
      //   color: 'lightblue',
      //   lineColor: '#60DAFF',
      //   lineWidth: 1,
      //   name: 'Battery to home'
      // },
      {
        type: 'area',
        color: '#5DDD40',
        lineColor: '#47B72C',
        lineWidth: 1,
        name: 'Sun to home'
      },
      // {
      //   type: 'area',
      //   color: 'gray',
      //   lineColor: 'black',
      //   lineWidth: 1,
      //   name: 'From grid (fossil fuels)'
      // },
      // {
      //   type: 'spline',
      //   color: 'red',
      //   name: 'Consumption'
      // },
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


  batteryChartOptions: Highcharts.Options = {
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
          text: 'Total Watts'
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
        color: '#C2E0A1',
        lineColor: '#47A530',
        lineWidth: 1,
        name: 'Sun to battery'
      },
      {
        type: 'area',
        color: '#FFD145',
        lineColor: '#C4A136',
        lineWidth: 1,
        name: 'Grid to battery'
      },
      {
        type: 'area',
        color: 'gray',
        lineColor: 'black',
        lineWidth: 1,
        name: 'Battery to home'
      },
      {
        type: 'area',
        color: '#5DDD40',
        lineColor: '#47B72C',
        lineWidth: 1,
        name: 'Battery to grid'
      },
      // {
      //   type: 'spline',
      //   color: 'red',
      //   name: 'Consumption'
      // },
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
  

  homeChartOptions: Highcharts.Options = {
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
          text: 'Total Watts'
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
        color: '#C2E0A1',
        lineColor: '#47A530',
        lineWidth: 1,
        name: 'Solar to home'
      },
      {
        type: 'area',
        color: '#FFD145',
        lineColor: '#C4A136',
        lineWidth: 1,
        name: 'Battery to home'
      },
      {
        type: 'area',
        color: 'gray',
        lineColor: 'black',
        lineWidth: 1,
        name: 'Grid to home'
      },
      {
        type: 'area',
        color: 'brown',
        lineColor: 'black',
        lineWidth: 1,
        name: 'Grid to battery'
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


  constructor(private powerMovementService: PowerMovementService,
    private initialDateService: InitialDateService) { }

  ngOnInit() {
    this.initialDateService.today.pipe(
      take(1),
      tap(s => this.displayDate = s),
      switchMap(s => this.getData()),
      tap(value => this.applyData(value))
    ).subscribe();
  }

  private getData(): Observable<PowerMovementDto> {
    return this.powerMovementService.get(this.displayDate);
  }

  private applyData(value: PowerMovementDto){
    this.solarChartOptions.plotOptions.area.pointStart = Date.UTC(2019, 5, 19, 0, 0, 0);
    this.solarChartOptions.series[0]['data'] = value.fromSunToGrid.data;
    this.solarChartOptions.series[1]['data'] = value.fromSunToBattery.data;
    this.solarChartOptions.series[2]['data'] = value.fromSunToHome.data;

    this.batteryChartOptions.series[0]['data'] = value.fromSunToBattery.data;
    this.batteryChartOptions.series[1]['data'] = value.fromGridToBattery.data;
    this.batteryChartOptions.series[2]['data'] = value.fromBatteryToHomeNeg.data;
    this.batteryChartOptions.series[3]['data'] = value.fromBatteryToCommunityNeg.data;

    this.homeChartOptions.series[0]['data'] = value.fromSunToHome.data;
    this.homeChartOptions.series[1]['data'] = value.fromBatteryToHome.data;
    this.homeChartOptions.series[2]['data'] = value.fromGridToHome.data;
    this.homeChartOptions.series[3]['data'] = value.fromGridToBattery.data;

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
