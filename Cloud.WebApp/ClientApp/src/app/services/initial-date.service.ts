import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { filter, tap } from 'rxjs/operators';
import { dateFormat } from 'highcharts';

@Injectable()
export class InitialDateService {
  private time: BehaviorSubject<Date>;
  private date: Observable<Date>;

  public get today(): Observable<Date> {
    return this.date;
  }

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
    this.time = new BehaviorSubject<Date>(new Date(2000,1,1));
    this.date = this.time.pipe(
      filter(a => a.getFullYear() != 2000)
    );
    this.get().subscribe(
      a => this.time.next(new Date(+a.substring(0, 4),+a.substring(5,7)-1,+a.substring(8,10))),
      err => this.time.next(new Date())
      );
  }
  
  get(): Observable<string> {
    return this.http.get<string>(this.baseUrl + 'api/Time');
  }
}
