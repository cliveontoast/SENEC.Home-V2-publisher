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
import { PowerMovementComponent } from './power-movement/power-movement.component';
import { PowerMovementService } from './services/power-movement.service';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    EnergyComponent,
    HomeConsumptionComponent,
    PowerMovementComponent,
    VoltagesComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    HighchartsChartModule,
    RouterModule.forRoot([
      { path: 'consumption', component: HomeConsumptionComponent },
      { path: 'power', component: PowerMovementComponent },
      { path: 'battery', component: EnergyComponent },
      { path: 'voltages', component: VoltagesComponent },
      { path: '**', component: HomeConsumptionComponent }
    ])
  ],
  providers: [
    VoltageSummaryService,
    HomeConsumptionService,
    EnergySummaryService,
    PowerMovementService,
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
