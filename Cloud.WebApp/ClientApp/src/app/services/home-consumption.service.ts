import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DailyHomeConsumptionDto } from '../dto/dailyHomeConsumptionDto';

@Injectable()
export class HomeConsumptionService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }
  
  get(date: Date): Observable<DailyHomeConsumptionDto> {
    return this.http.get<DailyHomeConsumptionDto>(this.baseUrl + 'api/DailyHomeConsumption',
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
