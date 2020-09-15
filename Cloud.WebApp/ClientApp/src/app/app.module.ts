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

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    EnergyComponent,
    VoltagesComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    HighchartsChartModule,
    RouterModule.forRoot([
      { path: 'voltages', component: VoltagesComponent },
      { path: 'energy', component: EnergyComponent },
      { path: '**', component: VoltagesComponent }
    ])
  ],
  providers: [
    VoltageSummaryService,
    EnergySummaryService,
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
