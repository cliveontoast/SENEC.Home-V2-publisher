import { Component, OnInit } from '@angular/core';
import * as Highcharts from 'highcharts';
import { TemperatureSummaryService } from '../services/temperatureSummary.service';
import { InitialDateService } from '../services/initial-date.service';
import { take, tap, switchMap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { DailyEquipmentStatesSummaryDto } from '../dto/dailyEquipmentStatesSummaryDto';
import { EquipmentStateService } from '../services/equipment-state.service';

@Component({
  selector: 'app-equipment-state',
  templateUrl: './equipment-state.component.html',
  styleUrls: ['./equipment-state.component.css']
})
export class EquipmentStateComponent implements OnInit {

  Highcharts: typeof Highcharts = Highcharts; // required
  chartConstructor: string = 'chart'; // optional string, defaults to 'chart'
  chartOptions: Highcharts.Options = {
    title: { text: 'States of the battery' },
    chart: {
      type: 'area',
      zoomType: 'x',
    },
    xAxis: {
      type: 'datetime',
      labels: {
          overflow: 'justify'
      }
    },
    yAxis: {
      title: {
          text: 'Seconds'
      },
    },
    plotOptions: {
      area: {
          stacking: 'percent',
          lineColor: '#ffffff',
          lineWidth: 1,
          marker: {
              lineWidth: 1,
              lineColor: '#ffffff'
          },
          accessibility: {
              pointDescriptionFormatter: function (point) {
                  function round(x) {
                      return Math.round(x * 100) / 100;
                  }
                  return (point.index + 1) + ', ' + point.category + ', ' +
                      point.y + ' seconds, ' + round(point.percentage) + '%, ' +
                      point.series.name;
              }
          },
          pointStart: Date.UTC(2018,1,1),
          pointInterval: 3600000/12, // one hour / 12 = 5 min
      },
    }
  }; // required
  chartCallback: Highcharts.ChartCallbackFunction = function (chart) {  }; // optional function, defaults to null
  updateFlag: boolean = true; // optional boolean
  oneToOneFlag: boolean = true; // optional boolean, defaults to false
  runOutsideAngularFlag: boolean = false; // optional boolean, defaults to false

  displayDate: Date = new Date();


  constructor(private equipmentStateService: EquipmentStateService,
    private initialDateService: InitialDateService) { }

  ngOnInit() {
    this.initialDateService.today.pipe(
      take(1),
      tap(s => this.displayDate = s),
      switchMap(s => this.getData()),
      tap(value => this.applyData(value))
    ).subscribe();
  }

  private getData(): Observable<DailyEquipmentStatesSummaryDto> {
    return this.equipmentStateService.get(this.displayDate);
  }

  private applyData(value: DailyEquipmentStatesSummaryDto){
    //this.chartOptions.plotOptions.spline.pointStart = Date.UTC(2019, 5, 19, 0, 0, 0);
    this.chartOptions.series = value.states as Highcharts.SeriesAreaOptions[];
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

  refresh() {
    this.getData().subscribe(a => this.applyData(a));
  }
}
