import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TrailercompanyComponent } from './trailercompany.component';

describe('TrailercompanyComponent', () => {
  let component: TrailercompanyComponent;
  let fixture: ComponentFixture<TrailercompanyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TrailercompanyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TrailercompanyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
