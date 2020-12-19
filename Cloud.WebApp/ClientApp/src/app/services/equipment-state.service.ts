import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DailyEquipmentStatesSummaryDto } from '../dto/dailyEquipmentStatesSummaryDto';

@Injectable({
  providedIn: 'root'
})
export class EquipmentStateService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }
  
  get(date: Date): Observable<DailyEquipmentStatesSummaryDto> {
    return this.http.get<DailyEquipmentStatesSummaryDto>(this.baseUrl + 'api/EquipmentStatesSummary',
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
