import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DailyVoltageSummaryDto } from '../dto/dailyVoltageSummaryDto';
import { DailyVoltageMomentDto } from '../dto/dailyVoltageMomentDto';

@Injectable()
export class VoltageSummaryService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }
  
  get(date: Date): Observable<DailyVoltageSummaryDto> {
    return this.http.get<DailyVoltageSummaryDto>(this.baseUrl + 'api/VoltageSummary',
      {
        params: {
          date: this.getString(date),
        }
      });
  }

  getHighPrecision(date: Date): Observable<DailyVoltageMomentDto> {
    return this.http.get<DailyVoltageMomentDto>(this.baseUrl + 'api/VoltageMoment',
      {
        params: {
          date: this.getString(date),
        }
      });
  }

  private getString(date: Date): string {
    return new Date(date.getTime() - (date.getTimezoneOffset() * 60000 ))
      .toISOString()
      .split("T")[0];
  }
}
