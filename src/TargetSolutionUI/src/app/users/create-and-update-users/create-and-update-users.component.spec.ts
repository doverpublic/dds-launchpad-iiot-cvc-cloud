import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateAndUpdateUsersComponent } from './create-and-update-users.component';

describe('CreateAndUpdateUsersComponent', () => {
  let component: CreateAndUpdateUsersComponent;
  let fixture: ComponentFixture<CreateAndUpdateUsersComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateAndUpdateUsersComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateAndUpdateUsersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
