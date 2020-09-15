import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DailyEnergySummaryDto } from '../dto/dailyEnergySummaryDto';

@Injectable()
export class EnergySummaryService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }
  
  get(date: Date): Observable<DailyEnergySummaryDto> {
    return this.http.get<DailyEnergySummaryDto>(this.baseUrl + 'api/EnergySummary',
      {
        params: {
          date: this.getString(date),
        }
      });
  }

  private getString(date: Date): string {
    return date.toISOString().split('T')[0]
  }
}
