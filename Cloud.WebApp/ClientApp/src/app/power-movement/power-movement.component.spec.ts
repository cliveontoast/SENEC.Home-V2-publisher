import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PowerMovementComponent } from './power-movement.component';

describe('PowerMovementComponent', () => {
  let component: PowerMovementComponent;
  let fixture: ComponentFixture<PowerMovementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PowerMovementComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PowerMovementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
