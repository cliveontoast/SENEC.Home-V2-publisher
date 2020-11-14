import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DailyTemperatureSummaryDto } from '../dto/dailyTemperatureSummaryDto';

@Injectable()
export class TemperatureSummaryService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }
  
  get(date: Date): Observable<DailyTemperatureSummaryDto> {
    return this.http.get<DailyTemperatureSummaryDto>(this.baseUrl + 'api/TemperatureSummary',
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
