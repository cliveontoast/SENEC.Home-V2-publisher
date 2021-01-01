import { Injectable, Inject } from '@angular/core';
import { PublishersDto } from '../dto/publishersDto';
import { BehaviorSubject, Observable, timer } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { filter, switchMap, tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PublishersService {
  private publishers$: BehaviorSubject<PublishersDto>;
  private getPublishers$: Observable<PublishersDto>;

  public get publishers(): Observable<PublishersDto> {
    return this.getPublishers$;
  }

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
    this.publishers$ = new BehaviorSubject<PublishersDto>(null);
    this.getPublishers$ = this.publishers$.pipe(
      filter(a => a != null)
    );
    const source = timer(0, 1000 * 60 * 5);
    source.pipe(
      switchMap(() => this.get()),
      tap(a => this.publishers$.next(a))
      ).subscribe(
        null,
        err => this.publishers$.next(<PublishersDto>{ publishers: [] })
        );
  }
  
  get(): Observable<PublishersDto> {
    return this.http.get<PublishersDto>(this.baseUrl + 'api/Publisher');
  }
}
