import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { VoltagesComponent } from './voltages/voltages.component';
import { VoltageSummaryService } from './services/voltageSummary.service';
import { HighchartsChartModule } from 'highcharts-angular';
import { EnergyComponent } from './energy/energy.component';
import { EnergySummaryService } from './services/energySummary.service';
import { HomeConsumptionComponent } from './home-consumption/home-consumption.component';
import { HomeConsumptionService } from './services/home-consumption.service';
import { HomeAllComponent } from './home-all/home-all.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    EnergyComponent,
    HomeConsumptionComponent,
    VoltagesComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    HighchartsChartModule,
    RouterModule.forRoot([
      { path: 'consumption', component: HomeConsumptionComponent },
      { path: 'energy', component: EnergyComponent },
      { path: 'voltages', component: VoltagesComponent },
      { path: '**', component: HomeConsumptionComponent }
    ])
  ],
  providers: [
    VoltageSummaryService,
    HomeConsumptionService,
    EnergySummaryService,
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
