import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewDispatchOfficeComponent } from './view-dispatch-office.component';

describe('ViewDispatchOfficeComponent', () => {
  let component: ViewDispatchOfficeComponent;
  let fixture: ComponentFixture<ViewDispatchOfficeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ViewDispatchOfficeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewDispatchOfficeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
