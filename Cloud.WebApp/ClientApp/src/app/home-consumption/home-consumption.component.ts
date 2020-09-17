import { Component } from '@angular/core';
import * as Highcharts from 'highcharts';
import { HomeConsumptionService } from '../services/home-consumption.service';

@Component({
  selector: 'app-home-consumption',
  templateUrl: './home-consumption.component.html'
})
export class HomeConsumptionComponent {

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
          text: 'Total Watt hours'
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
        color: 'lightblue',
        name: 'From battery (from sun)'
      },
      {
        type: 'area',
        color: 'lightgreen',
        name: 'From sun'
      },
      {
        type: 'area',
        color: 'gray',
        name: 'From grid (fossil fuels)'
      },
      {
        type: 'spline',
        color: 'red',
        name: 'Total'
      },
    ],
    tooltip: {
      split: true,
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


  constructor(private homeConsumptionService: HomeConsumptionService) { }

  ngOnInit() {
    this.getData();
  }

  private getData() {
    this.homeConsumptionService.get(this.displayDate).subscribe(value => {
      this.chartOptions.plotOptions.area.pointStart = Date.UTC(2019, 5, 19, 0, 0, 0);
      this.chartOptions.series[0]['data'] = value.fromBattery.data;
      this.chartOptions.series[1]['data'] = value.fromSolar.data;
      this.chartOptions.series[2]['data'] = value.fromGrid.data;
      this.chartOptions.series[3]['data'] = value.toHome.data;
      this.updateFlag = true;
    });
  }

  previousDay() {
    var dte = new Date(this.displayDate.toUTCString());
    dte.setDate(dte.getDate()-1);
    this.displayDate = dte;
    this.getData();
  }

  nextDay() {
    var dte = new Date(this.displayDate.toUTCString());
    dte.setDate(dte.getDate()+1);
    this.displayDate = dte;
    this.getData();
  }
}
