import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DispatchofficeComponent } from './dispatchoffice.component';

describe('DispatchofficeComponent', () => {
  let component: DispatchofficeComponent;
  let fixture: ComponentFixture<DispatchofficeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DispatchofficeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DispatchofficeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
