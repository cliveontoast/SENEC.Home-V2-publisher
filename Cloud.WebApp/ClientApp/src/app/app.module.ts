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

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    VoltagesComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    HighchartsChartModule,
    RouterModule.forRoot([
      { path: 'voltages', component: VoltagesComponent },
      { path: '**', component: VoltagesComponent }
    ])
  ],
  providers: [VoltageSummaryService],
  bootstrap: [AppComponent]
})
export class AppModule { }
