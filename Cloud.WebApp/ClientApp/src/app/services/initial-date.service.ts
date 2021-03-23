import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, timer } from 'rxjs';
import { filter, tap, switchMap } from 'rxjs/operators';

@Injectable()
export class InitialDateService {
  private time: BehaviorSubject<Date>;
  private date: Observable<Date>;

  public get today(): Observable<Date> {
    return this.date;
  }

  public get refresh$(): Observable<any> {
    return timer(1000 * 60 * 60, 1000 * 60 * 60);
  }

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
    this.time = new BehaviorSubject<Date>(new Date(2000,1,1));
    this.date = this.time.pipe(
      filter(a => a.getFullYear() != 2000),
      filter(a => a.toString() == this.time.value.toString())
    );
    const source = timer(0, 1000 * 60 * 60);
    source.pipe(
      switchMap(() => this.get()),
      tap(a => this.time.next(new Date(+a.substring(0, 4),+a.substring(5,7)-1,+a.substring(8,10))))
      ).subscribe(
        null,
        err => this.time.next(new Date())
        );
  }
  
  get(): Observable<string> {
    return this.http.get<string>(this.baseUrl + 'api/Time');
  }
}
