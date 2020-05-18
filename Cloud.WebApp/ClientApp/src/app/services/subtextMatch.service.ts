import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DailyVoltageSummaryDto } from '../dto/matchesDto';

@Injectable()
export class SubtextMatchService {

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
