import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateAndUpdateDispatchOfficeComponent } from './create-and-update-dispatch-office.component';

describe('CreateAndUpdateDispatchOfficeComponent', () => {
  let component: CreateAndUpdateDispatchOfficeComponent;
  let fixture: ComponentFixture<CreateAndUpdateDispatchOfficeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateAndUpdateDispatchOfficeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateAndUpdateDispatchOfficeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
