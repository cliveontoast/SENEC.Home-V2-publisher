import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HomeConsumptionComponent } from './home-consumption.component';

describe('HomeConsumptionComponent', () => {
  let component: HomeConsumptionComponent;
  let fixture: ComponentFixture<HomeConsumptionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ HomeConsumptionComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(HomeConsumptionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
