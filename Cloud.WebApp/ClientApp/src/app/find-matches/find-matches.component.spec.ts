import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { FindMatchesComponent } from './find-matches.component';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Type } from '@angular/core';
import { SubtextMatchService } from '../services/subtextMatch.service';

describe('CounterComponent', () => {
  let component: FindMatchesComponent;
  let fixture: ComponentFixture<FindMatchesComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [FindMatchesComponent],
      imports: [
        FormsModule,
        HttpClientTestingModule
      ],
      providers: [
        SubtextMatchService,
        { provide: 'BASE_URL', useValue: 'baseUrl/' }
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FindMatchesComponent);
    component = fixture.componentInstance;
    httpMock = fixture.debugElement.injector.get<HttpTestingController>(HttpTestingController as Type<HttpTestingController>);
    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display a match', async(() => {
    //const result = {
    //  characterPositions: [0, 1],
    //  error: null,
    //} as MatchesDto;

    //component.text = 'fff';
    //component.subText = 'ff';
    //component.search();

    //const req = httpMock.expectOne('baseUrl/api/SubtextMatch?text=fff&subText=ff');
    //req.flush(result);

    //expect(req.request.method).toBe('GET');
    //expect(component.matches.characterPositions).toEqual(result.characterPositions);
    //expect(component.matches.errorType).toEqual(result.errorType);
  }));

  it('should display no match', async(() => {
    //const nonResult = {
    //  characterPositions: [0, 1],
    //  error: null,
    //} as MatchesDto;

    //component.text = 'foo';
    //component.subText = 'bar';
    //component.matches = nonResult;
    //component.search();

    //const req = httpMock.expectOne('baseUrl/api/SubtextMatch?text=foo&subText=bar');
    //req.error({} as ErrorEvent);

    //expect(req.request.method).toBe('GET');
    //expect(component.matches).toBeFalsy();
  }));
});
