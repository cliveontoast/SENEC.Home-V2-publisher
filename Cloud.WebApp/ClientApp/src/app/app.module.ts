import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { ChartsModule } from 'ng2-charts';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { FindMatchesComponent } from './find-matches/find-matches.component';
import { SubtextMatchService } from './services/subtextMatch.service';
import { HighchartsChartModule } from 'highcharts-angular';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    FindMatchesComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    ChartsModule,
    HttpClientModule,
    FormsModule,
    HighchartsChartModule,
    RouterModule.forRoot([
      { path: 'find-matches', component: FindMatchesComponent },
      { path: '**', component: FindMatchesComponent }
    ])
  ],
  providers: [SubtextMatchService],
  bootstrap: [AppComponent]
})
export class AppModule { }
