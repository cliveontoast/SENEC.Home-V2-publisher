import { Component } from '@angular/core';
import { PublishersService } from './services/publishers.service';
import { Observable } from 'rxjs';
import { PublishersDto } from './dto/publishersDto';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'app';
  publishers$: Observable<PublishersDto>;

  constructor(private publishersService: PublishersService){
  }

  ngOnInit(){
    this.publishers$ = this.publishersService.publishers;
  }
}
