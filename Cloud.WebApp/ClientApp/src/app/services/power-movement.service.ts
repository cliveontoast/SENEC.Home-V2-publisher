import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PowerMovementDto } from '../dto/dailyPowerMovementDto';

@Injectable()
export class PowerMovementService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }
  
  get(date: Date): Observable<PowerMovementDto> {
    return this.http.get<PowerMovementDto>(this.baseUrl + 'api/DailyPowerMovement',
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
