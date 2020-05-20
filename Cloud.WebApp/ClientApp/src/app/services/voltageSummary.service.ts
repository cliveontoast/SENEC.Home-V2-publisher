import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DailyVoltageSummaryDto } from '../dto/dailyVoltageSummaryDto';

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

  private getString(date: Date): string {
    const offset = date.getTimezoneOffset()
    date = new Date(date.getTime() + (offset*60*1000))
    return date.toISOString().split('T')[0]
  }
}
