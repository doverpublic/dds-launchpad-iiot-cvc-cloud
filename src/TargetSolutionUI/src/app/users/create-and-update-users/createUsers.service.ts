import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';


@Injectable()
export class CreateUsersService {
  constructor() { }
  RoleType:string;
  firstname:string;
  lastname:string;
  ToggleActive:string;
  file:string;
  username: string;
  email:string;
  address:string;
  country:string;
  state:string;
  city:string;
  zipcode:string;
  phonenumber:number;
  fax:number;
  search:string;
  

  public UsersData:any;
  
}
