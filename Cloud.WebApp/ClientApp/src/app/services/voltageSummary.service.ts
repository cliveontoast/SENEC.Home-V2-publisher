import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DailyVoltageSummaryDto } from '../dto/dailyVoltageSummaryDto';

@Injectable()
export class VoltageSummaryService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }
  
  get(): Observable<DailyVoltageSummaryDto> {
    return this.http.get<DailyVoltageSummaryDto>(this.baseUrl + 'api/VoltageSummary',
      {
        params: {
          date: Date.now().toString(),
        }
      });
  }
}
