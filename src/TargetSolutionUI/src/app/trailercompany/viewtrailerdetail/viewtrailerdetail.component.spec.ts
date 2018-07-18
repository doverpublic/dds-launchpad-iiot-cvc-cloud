import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewtrailerdetailComponent } from './viewtrailerdetail.component';

describe('ViewtrailerdetailComponent', () => {
  let component: ViewtrailerdetailComponent;
  let fixture: ComponentFixture<ViewtrailerdetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ViewtrailerdetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewtrailerdetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
